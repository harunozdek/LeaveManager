# Spec 0001 — Mevcut davranışın karakterizasyon testleri (mini)

- Status: Shipped
- Mode: lite
- Plan: `specs/plans/0001-plan.md`
- Source: bootstrap 2026-10-23, open decision — characterization
- Design: —
- Supersedes: —

## Intent
İK ve geliştirme ekibi, LeaveManager'ın bugünkü davranışını — kusurlarıyla birlikte — otomatik
testlerle sabitlemek istiyor. Böylece SEC-1 düzeltmesi, .NET 10 geçişi, TimeProvider refactor'ü
(0002) ve açık kararların (OD-n) uygulanması fark edilmeden regresyona yol açamaz. Bu iş yalnızca
test ekler: uygulamanın gözlenebilir davranışı ve üretim kodu **değişmez**; hiçbir kusur düzeltilmez,
hiçbir OD kapatılmaz.

## Changed behavior
- [x] CB-1 — none — no observable behavior change

## Preserved behavior
Hepsi 2026-10-23'te çalışan uygulamada gözlendi (seed verisiyle, yeni başlatılmış uygulama).
Bakiye beklentileri yalnızca kıdemi yıllarca sabit kalan personel için yazılır (Ayşe, Mehmet, Ahmet).
Arayüzdeki metinler ASCII'dir; testler sayfada görünen dizeyle eşleşir (`Ayse Yilmaz`, `Mehmet Demir`, `Ahmet Celik`, `Kullanilan`).
- [x] PB-1 — (BR-10, BR-9) Yeni başlatılan uygulamanın ana sayfası: Bekleyen **4**, Onaylanan **1**, Reddedilen **3**; Talepler sayfası seed'deki 8 talebi seed sırasıyla ve durumlarıyla listeler; Yeni Talep formu seed'deki 5 personeli seçenek olarak listeler.
- [x] PB-2 — (BR-1, BR-4, BR-5) Bakiye tablosu: Ayşe Yılmaz Hak **26** / Kullanılan **12** / Kalan **14**; Mehmet Demir **20 / 0 / 20** (bekleyen ve reddedilen talepleri sayılmaz); Ahmet Çelik **20 / 0 / 20**.
- [x] PB-3 — (BR-3) Talep günü takvim günüdür, hafta sonları dahil: 2026-07-06 → 2026-07-17 talebi **12** gün, tek günlük talep **1** gün gösterilir.
- [x] PB-4 — (BR-6) Bakiyesi yeten geçerli talep (Mehmet, 2 gün): kullanıcı Talepler sayfasına yönlendirilir, listenin sonunda **Beklemede** durumunda yeni satır görünür; ana sayfada Bekleyen bir artar, Mehmet'in Kullanılan ve Kalan değerleri **değişmez** (OD-5).
- [x] PB-5 — (BR-6) Kalan < talep günü (Ayşe, kalan 14, 15 günlük talep): sayfada aynen "HATA: ERR-412 - bakiye yetersiz. IT'ye bildirin: dahili 4127" görünür, talep kaydedilmez.
- [x] PB-6 — (BR-6, OD-4) Kalan = talep günü (Ayşe, 14 gün): hata mesajı **gösterilmez**, kullanıcı Talepler sayfasına yönlendirilir, talep **kaydedilmez** (sessiz ret). Bir gün eksik (13 gün) talep ise kaydedilir.
- [x] PB-7 — (BR-7, OD-6) Bitişi başlangıçtan önce olan talep (2026-11-10 → 2026-11-08) kaydedilir ve Gün sütununda **-1** görünür; geçmiş tarihli talep (2020-01-01, 1 gün) kaydedilir.
- [x] PB-8 — (BR-8) Ayrıştırılamayan tarih, bilinmeyen personel (Id 99) ya da boş form: hata mesajı gösterilmeden Talepler sayfasına yönlendirilir, talep sayısı değişmez.
- [x] PB-9 — (BR-10) Uygulama yeniden başlatıldığında önceki çalıştırmada eklenen talepler kaybolur; liste yeniden seed'deki 8 talepten oluşur.
- [x] PB-10 — (SEC-1, **kusur olarak sabitlenir**) Neden alanına girilen `<script>alert(1)</script>` metni Talepler sayfasının HTML'inde encode edilmeden, aynen yer alır.

## Out of scope
- **Üretim koduna dokunulmaz** (`Program.cs`, `LeaveManager.csproj`). Yalnızca `tests/` altında değişiklik.
- BR-1 sınır değerleri (0/1/5/6/15/16 yıl), BR-2 kıdem hassasiyeti, Zeynep ve Elif'in bakiyeleri — bugünün tarihine
  bağlılar; TimeProvider seam'i ile **spec 0002**'de.
- PB-2 geçerlilik ufku: Mehmet (2019) ve Ahmet (2020) kıdemleri 16 yıla ulaşınca (2035, 2036) hakları 26 olur;
  PB-2 o tarihe kadar kararlıdır. 0002'deki sabit saat bu bağımlılığı kaldırır.
- Durum renkleri (yeşil/turuncu/kırmızı) ve sayfa düzeni sabitlenmez — sunum detayı.
- Hiçbir kusur düzeltilmez, hiçbir OD kapatılmaz; "bugün"ün Europe/Istanbul olması da kapsam dışı.
- **PB-10 notu:** XSS bilinçli olarak mevcut davranış diye sabitlenir (karar: kullanıcı, 2026-10-23). SEC-1'i düzelten
  `/fix-bug` bu kriteri ve testini **kendi onaylı spec'i ile** değiştirecektir — bu, kural 5 ihlali değil, spec'li davranış değişikliğidir.
- Split: proposed 0001 (characterization, test-only) + 0002 (TimeProvider seam refactor + BR-1/BR-2 boundary characterization); accepted — user, 2026-10-23, 0001 ships alone as the safety net for SEC-1 and the .NET 10 migration.

## Definition of Done
- [x] `scripts/check` green
- [x] Independent review done; real findings fixed, noise rejected with written rationale
- [x] Criterion ↔ evidence table complete for CB-* **and** PB-* (UI: before/after screenshots)
- [x] Spec moved to `specs/done/` (immutable there)
