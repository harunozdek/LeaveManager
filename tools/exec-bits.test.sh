#!/bin/sh
# tools/exec-bits.test.sh — spec 0003: tools/exec-bits.sh senaryo testleri (T1-T7, plan 0003).
# Gecici git deposu repo DISINDA (mktemp -d) kurulur ve her durumda silinir (docs/testing.md, rule 5).
# Modlar yalnizca git index'iyle verilir (git add --chmod / update-index), chmod ile asla (CB-2).
set -u
HERE=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
GUARD="$HERE/exec-bits.sh"
TMP=$(mktemp -d) || exit 1
trap 'rm -rf "$TMP"' EXIT
trap 'exit 130' INT TERM HUP   # dash sinyalde EXIT trap'ini calistirmaz; exit onu tetikler
GIT_CEILING_DIRECTORIES=$(dirname -- "$TMP")   # R2: ust dizindeki bir depoya cikilmasin
export GIT_CEILING_DIRECTORIES
# Gelistiricinin global/sistem git ayarlari (gpgsign, hooksPath, color...) testleri etkilemesin.
GIT_CONFIG_GLOBAL=/dev/null
GIT_CONFIG_NOSYSTEM=1
export GIT_CONFIG_GLOBAL GIT_CONFIG_NOSYSTEM
fails=0
runs=0

# new_repo <ad>: bos depo + kontrolun calistirilabilir kopyasi; depo yolunu yazar
new_repo() {
  r="$TMP/$1"
  mkdir -p "$r/tools"
  git -C "$r" init -q
  git -C "$r" config core.autocrlf false   # yalnizca gecici depo; gelistiricinin global ayari degismez
  cp "$GUARD" "$r/tools/exec-bits.sh" || return 1
  git -C "$r" add --chmod=+x tools/exec-bits.sh
  echo "$r"
}

# expect <ad> <beklenen cikis> <dizin> [ciktida bulunmasi gereken metin...]
expect() {
  name=$1 want=$2 dir=$3
  shift 3
  runs=$((runs + 1))
  out=$(sh "$dir/tools/exec-bits.sh" 2>&1)
  rc=$?
  ok=1
  [ "$rc" -eq "$want" ] || ok=0
  for s in "$@"; do printf '%s' "$out" | grep -qF -- "$s" || ok=0; done
  if [ "$ok" -eq 1 ]; then
    echo "ok    $name"
  else
    echo "FAIL  $name (cikis $rc, beklenen $want)"
    printf '%s\n' "$out" | sed 's/^/      /'
    fails=$((fails + 1))
  fi
}

# T1 shebang + 100644, commit'li -> kirmizi; yol ve duzeltme komutu gorunur (CB-1)
r=$(new_repo t1) && printf '#!/bin/sh\necho hi\n' > "$r/run.sh" && git -C "$r" add run.sh
if git -C "$r" -c user.name=t -c user.email=t@t -c commit.gpgsign=false commit -q --no-verify -m t; then
  expect "T1 shebang 100644 commit'li -> 1" 1 "$r" "run.sh" "git update-index --chmod=+x 'run.sh'"
else
  echo "FAIL  T1 hazirlik: commit basarisiz, senaryo sinanamadi"
  fails=$((fails + 1))
fi

# T2 shebang + 100644, yalnizca git add (commit yok) -> kirmizi (CB-1, staged)
r=$(new_repo t2) && printf '#!/bin/sh\necho hi\n' > "$r/run.sh" && git -C "$r" add run.sh
expect "T2 shebang 100644 staged -> 1" 1 "${r:-$TMP/t2}" "run.sh"

# T3 shebang + 100755 -> yesil
r=$(new_repo t3) && printf '#!/bin/sh\necho hi\n' > "$r/run.sh" && git -C "$r" add --chmod=+x run.sh
expect "T3 shebang 100755 -> 0" 0 "${r:-$TMP/t3}"

# T4 shebang'siz + 100644 -> yesil (CB-3)
r=$(new_repo t4) && printf 'merhaba\n' > "$r/notes.txt" && git -C "$r" add notes.txt
expect "T4 shebang'siz 100644 -> 0" 0 "${r:-$TMP/t4}"

# T5 icerigi #! ile baslayan symlink kaydi (120000) -> yok sayilir (git grep --cached symlink aramaz;
#    guard'da ayrica bir dal yok — bu senaryo davranisi dogrular, review bulgu 6)
r=$(new_repo t5) && sha=$(printf '#!hedef' | git -C "$r" hash-object -w --stdin) &&
  git -C "$r" update-index --add --cacheinfo "120000,$sha,link"
expect "T5 symlink 120000 -> 0" 0 "${r:-$TMP/t5}"

# T6 git deposu olmayan dizin -> yesil, not yazilir (CB-5)
mkdir -p "$TMP/t6/tools" && cp "$GUARD" "$TMP/t6/tools/exec-bits.sh"
expect "T6 git deposu degil -> 0" 0 "$TMP/t6" "git deposu değil, atlandı"

# T7 #! ilk satirda degil -> shebang sayilmaz -> yesil
r=$(new_repo t7) && printf 'echo hi\n#!/bin/sh\n' > "$r/run.sh" && git -C "$r" add run.sh
expect "T7 #! ikinci satirda -> 0" 0 "${r:-$TMP/t7}"

# T8 kullanicinin grep/renk ayarlari kontrolu koremez -> kirmizi (review bulgu 1)
r=$(new_repo t8) && printf '#!/bin/sh\necho hi\n' > "$r/run.sh" && git -C "$r" add run.sh &&
  git -C "$r" config color.ui always && git -C "$r" config color.grep always &&
  git -C "$r" config grep.patternType fixed
expect "T8 color/patternType ayarli depo -> 1" 1 "${r:-$TMP/t8}" "run.sh"

# T9a Turkce karakterli yol, 100755 -> yesil (review bulgu 2: tirnakli yol yanlis kirmizi veriyordu)
r=$(new_repo t9a) && printf '#!/bin/sh\necho hi\n' > "$r/izin-ş.sh" && git -C "$r" add --chmod=+x "izin-ş.sh"
expect "T9a Turkce yol 100755 -> 0" 0 "${r:-$TMP/t9a}"

# T9c Turkce karakterli yol, 100644 -> kirmizi (sessizce atlanmamali)
r=$(new_repo t9c) && printf '#!/bin/sh\necho hi\n' > "$r/izin-ş.sh" && git -C "$r" add "izin-ş.sh"
expect "T9c Turkce yol 100644 -> 1" 1 "${r:-$TMP/t9c}" "izin-ş.sh (100644)"

# T9b bosluk ve glob karakterli yol, 100644 -> kirmizi, yol oldugu gibi yazilir
r=$(new_repo t9b) && printf '#!/bin/sh\necho hi\n' > "$r/a b[1].sh" && git -C "$r" add "a b[1].sh"
expect "T9b bosluk/glob karakterli yol 100644 -> 1" 1 "${r:-$TMP/t9b}" "a b[1].sh (100644)" "git update-index --chmod=+x 'a b[1].sh'"

# T10 icerikte ':1:#!' gecen, shebang'siz (#! 2. satirda) dosya -> yesil (review bulgu 3)
r=$(new_repo t10) && printf 'echo hi\n#! not a:1:#!b\n' > "$r/y.sh" && git -C "$r" add y.sh
expect "T10 icerikte ':1:#!' -> 0" 0 "${r:-$TMP/t10}"

# T11 git grep hata verirse (bozuk index) sessizce "ok" denmez -> cikis 2 (review bulgu 4)
r=$(new_repo t11) && printf 'bozuk' > "$TMP/bozuk-index"
GIT_INDEX_FILE="$TMP/bozuk-index"
export GIT_INDEX_FILE
expect "T11 git grep hatasi -> 2" 2 "${r:-$TMP/t11}" "git grep basarisiz"
unset GIT_INDEX_FILE

rm -rf "$TMP"
if [ -d "$TMP" ]; then echo "FAIL  gecici depo silinemedi: $TMP"; fails=$((fails + 1)); fi

echo "exec-bits-test: $runs senaryo, $fails basarisiz"
[ "$fails" -eq 0 ]
