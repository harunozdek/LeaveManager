# Security

## Secrets
- Secrets never enter the repo, specs, prompts, or chat. `.env` is gitignored; provide `.env.example`.
- Agents never print secret values, even when debugging.
- Bugün uygulamada gizli bilgi yok. Veritabanı geldiğinde bağlantı dizesi geliştirmede `dotnet user-secrets`,
  diğer ortamlarda ortam değişkeninden okunur; `appsettings*.json`'a yazılmaz.

## Input & output
- **Çıktı:** kullanıcıdan ya da veriden gelen her değer HTML'e `WebUtility.HtmlEncode` / `HtmlEncoder`
  ile basılır — personel adı, neden, birim dahil. Ham string birleştirme ile encode'suz HTML yasak.
- **Girdi:** form değerleri sınırda (Web) doğrulanır: personel var mı, tarihler ayrıştırılıyor mu,
  bitiş ≥ başlangıç mı (OD-6 kapanınca), `reason` uzunluk sınırı içinde mi. Geçersiz girdi kullanıcıya
  Türkçe mesajla döner, sessizce yutulmaz.
- POST formlarında antiforgery token kullanılır.
- Hata mesajlarında stack trace, istisna metni, dahili kod ya da dahili iletişim bilgisi sızmaz (`docs/conventions.md`).

## AuthN / AuthZ
- **Mevcut duruş:** kimlik doğrulama yok; tüm endpoint'ler anonimdir. Uygulama **yalnızca güvenilir iç
  ağda** çalışır ve dışarı açılmaz (varsayım — bootstrap'te teyit edilmedi; açık karar).
- Kimlik doğrulama ve rol modeli (personel / yönetici / İK) ayrı bir spec + ADR ile gelir ve
  **onay/ret akışından önce** eklenmek zorundadır — kimliksiz onay = herkes herkesin iznini onaylar.
- Geldiğinde varsayılan reddet: `FallbackPolicy = RequireAuthenticatedUser`; yetki Application katmanında kontrol edilir.

## Known vulnerabilities (bootstrap, 2026-10-23)
- **SEC-1 Stored XSS:** `Reason` (ve diğer alanlar) `/requests` sayfasına encode edilmeden basılıyor.
  Kusur değil güvenlik açığıdır → ilk sıradaki iş, `/fix-bug`.
- **SEC-2** Kimlik doğrulama/yetkilendirme yok (yukarıda).
- **SEC-3** CSRF koruması yok (`POST /new`).
- **SEC-4** Hata mesajı dahili bilgi sızdırıyor ("ERR-412 … dahili 4127").

## Dependencies
- Yeni NuGet paketi yalnızca insan onayıyla eklenir; plan'da gerekçesi yazılır.
- Kontrol: lisans (MIT / Apache-2.0 / BSD tercih), bakım durumu (son sürüm, aktif depo), bilinen açıklar.
- `dotnet list package --vulnerable` ağ gerektirdiği için `scripts/check`'te değil; CI'da ayrı adım olarak önerilir (açık karar).

## Review lens
Security is a mandatory dimension of every independent review (see `prompts/review.md`), not a
separate afterthought phase.
