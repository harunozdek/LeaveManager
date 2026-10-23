# Spec 0003 — Çalıştırılabilir script koruması (mini)

- Status: In progress
- Mode: lite
- Plan: `specs/plans/0003-plan.md`
- Source: PR #2 review, finding 3 (CI run 39982856457 — exit code 126)
- Design: —
- Supersedes: —

## Intent
Windows'tan commit edilen script'ler git'e çalıştırma biti olmadan (100644) girebiliyor; bu
bootstrap'ta CI'ın tamamını belirsiz bir "exit code 126" ile kırdı ve ancak push'tan sonra fark edildi.
Geliştiriciler bu hatayı, commit'ten önce yerel `./scripts/check` çalıştırırken, dosya adı ve düzeltme
komutuyla birlikte görmek istiyor. ANEW çekirdek dosyalarının davranışı değişmez.

## Changed behavior
- [ ] CB-1 — Git'te takip edilen (henüz commit'lenmemiş ama `git add` ile eklenmiş olanlar dahil) ve ilk
  satırı `#!` ile başlayan bir dosya git'te çalıştırılabilir olarak kayıtlı değilse, `./scripts/check` hem full hem `--fast` profilde kırmızı biter; çıktı her ihlal eden
  dosyanın yolunu ve düzeltme komutunu (`git update-index --chmod=+x <yol>`) gösterir.
- [ ] CB-2 — Bu kontrol, dosyanın diskteki izinlerine değil git'teki kaydına bakar: aynı commit Windows'ta
  ve CI'da (Linux) aynı sonucu verir.
- [ ] CB-3 — Shebang'siz dosyalar (ör. `scripts/check.conf`, `.cs`, `.md`) kontrolü etkilemez.
- [ ] CB-4 — Depodaki shebang'li dosyaların tamamı çalıştırılabilir olarak kayıtlıdır; bugün 100644 olan
  `adapters/claude-code/.claude/hooks/protect-shipped.sh` dahil. Kontrol bu durumda yeşildir.
- [ ] CB-5 — Git deposu olmayan bir kopyada (ör. zip) kontrol kırmızı olmaz; "git deposu değil, atlandı"
  notuyla geçer.

## Preserved behavior
Gözlem: `main` @ `abbff8e`, 2026-10-23.
- [ ] PB-1 — Mevcut check adımları (build, format, test, test-slow) aynı sırayla çalışır ve aynı sayıda
  test koşar (main'de 1; spec 0001 merge edildiyse 20). `CHECK GREEN` full ve fast profilde sürer.
- [ ] PB-2 — ANEW çekirdek dosyalarının (`scripts/check`, `scripts/doctor`, `scripts/init`,
  `scripts/residue`, `scripts/selftest`, `.claude/`, `adapters/`) **içeriği** değişmez; yalnızca CB-4'teki
  tek dosyanın modu değişir.
- [ ] PB-3 — CI'da `doctor` ve `check` job'ları yeşil kalır.
- [ ] PB-4 — Kalıntı testi (`./scripts/residue`, `--fast`) iki profilde de temiz kalır.

## Out of scope
- `scripts/doctor` ve `scripts/selftest`'e kontrol eklemek (reviewer önerisi) — ANEW çekirdeği; yükseltmelerle
  çakışır (karar: kullanıcı, 2026-10-23). İleride ANEW'e upstream öneri olarak gidebilir.
- Shebang'siz çalıştırılabilirler (derlenmiş ikili dosyalar) — depoda yok.
- Sembolik link'ler (git modu 120000) ve submodule'ler yok sayılır — depoda yok.
- CRLF satır sonlu ya da BOM'lu shebang'ler — ayrı sorun; `.gitattributes` script'ler için LF'i zaten zorluyor.
- Geliştirici makinelerinin git ayarlarını (`core.fileMode`) değiştirmek.
- Numara: 0002 TimeProvider işi için ayrılmıştır (spec 0001'de atıf); bu iş bilinçli olarak 0003 (karar: kullanıcı, 2026-10-23).

## Definition of Done
- [ ] `scripts/check` green
- [ ] Independent review done; real findings fixed, noise rejected with written rationale
- [ ] Criterion ↔ evidence table complete for CB-* **and** PB-* (UI: before/after screenshots)
- [ ] Spec moved to `specs/done/` (immutable there)
