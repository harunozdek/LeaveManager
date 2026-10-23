#!/bin/sh
# tools/exec-bits.sh — spec 0003: ilk satiri #! olan her takip edilen dosya git'te 100755 kayitli olmali.
# Dosya sisteminin izinlerine degil git index'ine bakar; Windows (core.fileMode=false) ve Linux ayni
# sonucu verir. Staged dosyalar dahildir. Symlink (120000) ve submodule (160000) kayitlari yok sayilir.
# Resource hygiene: yalnizca kisa omurlu git/sed surecleri; hicbir sey yazmaz.
ROOT=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$ROOT" || exit 1

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "exec-bits: git deposu değil, atlandı"
  exit 0
fi

# Index'teki icerikte ilk satiri #! olan dosyalar: tek git grep sureci, yalnizca 1. satir eslesmeleri.
bad=$(git grep --no-color -G --cached -I -n -e '^#!' 2>/dev/null | sed -n 's/^\(.*\):1:#!.*$/\1/p' |
  while IFS= read -r f; do
    mode=$(git ls-files -s -- "$f" | cut -d' ' -f1)
    case "$mode" in
      100755|120000|160000) ;;
      *) printf '%s %s\n' "$mode" "$f" ;;
    esac
  done)

if [ -z "$bad" ]; then
  echo "exec-bits: ok — shebang'li tum dosyalar git'te calistirilabilir"
  exit 0
fi

echo "exec-bits: calistirilamaz olarak kayitli script(ler) — Linux'ta 'exit code 126' verir:"
printf '%s\n' "$bad" | while IFS=' ' read -r mode f; do
  echo "  $f ($mode) -> duzeltme: git update-index --chmod=+x $f"
done
exit 1
