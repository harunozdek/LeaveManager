#!/bin/sh
# tools/exec-bits.sh — spec 0003: ilk satiri #! olan her takip edilen dosya git'te 100755 kayitli olmali.
# Dosya sisteminin izinlerine degil git index'ine bakar; Windows (core.fileMode=false) ve Linux ayni
# sonucu verir. Staged dosyalar dahildir. Symlink (120000) ve submodule (160000) kayitlari aday olmaz:
# git grep --cached yalnizca normal dosya kayitlarini arar (T5 bu davranisi dogrular).
# Resource hygiene: yalnizca kisa omurlu git/sed surecleri; hicbir sey yazmaz.
ROOT=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$ROOT" || exit 1

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "exec-bits: git deposu değil, atlandı"
  exit 0
fi

# Adaylar: index'te herhangi bir satiri #! ile baslayan dosyalar (tek git grep sureci; satir ciktisi
# ayristirilmaz). Calistirilabilir olmayan her aday icin ilk satir index blob'undan dogrulanir.
# -z: yollar tirnaklanmaz (Turkce/ozel karakterler); literal pathspec: [ * ? glob sayilmaz.
GIT_LITERAL_PATHSPECS=1
export GIT_LITERAL_PATHSPECS
# Pipe icinde git grep'in cikis kodu kaybolur; once -q ile olculur: 0 eslesme, 1 eslesme yok, digeri hata.
err=$(git grep --no-color -G --cached -I -q -e '^#!' 2>&1 >/dev/null)
rc=$?
if [ "$rc" -gt 1 ]; then
  echo "exec-bits: git grep basarisiz (cikis $rc) — kontrol yapilamadi: $err"
  exit 2
fi
candidates=$(git grep --no-color -G --cached -I -l -z -e '^#!' | tr '\000' '\n')

bad=""
while IFS= read -r f; do
  [ -n "$f" ] || continue
  mode=$(git ls-files -s -- "$f" | cut -d' ' -f1)
  case "$mode" in
    100755) continue ;;
    '') bad="${bad}? $f
"; continue ;;   # mod okunamadi: sessizce atlama, ihlal say (fail-closed)
  esac
  [ "$(git cat-file blob ":$f" | head -c 2)" = '#!' ] || continue
  bad="$bad$mode $f
"
done <<EOF
$candidates
EOF

if [ -z "$bad" ]; then
  echo "exec-bits: ok — shebang'li tum dosyalar git'te calistirilabilir"
  exit 0
fi

echo "exec-bits: calistirilamaz olarak kayitli script(ler) — Linux'ta 'exit code 126' verir:"
printf '%s' "$bad" | while IFS=' ' read -r mode f; do
  echo "  $f ($mode) -> duzeltme: git update-index --chmod=+x '$f'"
done
exit 1
