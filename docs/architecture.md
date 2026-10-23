# Architecture

## System overview
LeaveManager, Polsan iç kullanımı için personel yıllık izin takip sistemidir (kullanıcılar: personel
ve İK). Tek bir ASP.NET Core Minimal API uygulamasıdır (`net8.0`); sayfalar sunucu tarafında HTML
olarak üretilir. Mimari stil: **tek projeli, katmanlı modüler monolit** — sınırlar ayrı projelerle
değil namespace/klasörle çizilir (`docs/decisions/0001-modular-monolith.md`). Gerekçe: uygulama
küçük; ayrı projeler tören olur, ama hukuken hassas izin hesaplarının saf ve deterministik test
edilebilmesi için Domain'in diğer her şeyden ayrılması şarttır.

## Current state (bootstrap, 2026-10-23)
Aşağıdaki hedef yapı **kararlaştırılmıştır ama henüz kodda yoktur.** Bugün her şey tek bir
`Program.cs` dosyasındadır: endpoint'ler, statik `Db`, iş mantığı ve HTML üreten `LeaveService`,
`Html.Page`. Ayrıştırma yalnızca karakterizasyon testlerinden **sonra**, `/refactor` iş akışıyla
(davranış değişmez, testle kanıtlanır) yapılır. Bilinen sapmalar refactor kalemi olarak:
- `/` ve `/requests` endpoint'leri `Db`'yi doğrudan okuyor (kural 2 ihlali).
- `LeaveService.StatusBadge` HTML üretiyor (kural 3 ihlali).
- Bakiye kontrolü hem `POST /new` hem `CreateRequest` içinde, farklı operatörlerle (kural 4 ihlali, OD-4).
- `GetSeniorityYears` `DateTime.Now` kullanıyor (kural 1 ihlali).

## Modules / components and ownership

| Module | Single responsibility | Owns |
|---|---|---|
| Domain (`LeaveManager.Domain`) | Saf iş kuralları: hak, kıdem, talep günü, bakiye. I/O yok; "bugün" `TimeProvider` ile dışarıdan gelir | Hak tablosu, hesap kuralları, durum değerleri |
| Application (`LeaveService`) | Kullanım senaryoları: talep oluştur, listele, (ileride) onay/ret. Domain'i ve Persistence'ı orkestre eder | İş akışları, kural ihlali sonuçları |
| Persistence (`Db`, ileride repository) | Veri saklama; bugün bellekte ve seed'li | Personel ve izin talebi verisi |
| Web (endpoint'ler, `Html`) | HTTP, form okuma ve sınır doğrulaması, HTML üretimi ve encode | Sunum, kullanıcı mesajları |

## Communication rules
- Bağımlılık yönü tek yönlüdür: Web → Application → (Domain, Persistence). Domain hiçbir şeye bağımlı değildir.
- Modüller arası iletişim doğrudan metot çağrısıyladır; event/mesaj altyapısı yoktur ve şimdilik gerekmez.
- Web, veri okumak için de Application servisinden geçer.

## Forbidden dependencies (make them testable)
Mimari test (reflection ile namespace bağımlılık kontrolü) **refactor ile birlikte** eklenecek;
bugün tek dosya olduğu için kırmızı başlayacağından şimdi eklenmez (korunan testler kuralı).
1. Domain; `Microsoft.AspNetCore.*`, `HttpContext`, Persistence tipleri (`Db`) ve `Html` kullanmaz.
   `DateTime.Now` / `DateTime.Today` / `DateTime.UtcNow` kullanmaz — saat `TimeProvider` ile gelir.
2. Web, Persistence'a (`Db`) doğrudan erişmez; yalnızca Application servisini çağırır.
3. Application HTML üretmez (HTML/CSS string'i içermez).
4. Bir iş kuralı tek yerde yaşar: aynı kural (ör. bakiye kontrolü) Web'de ve Application'da tekrarlanmaz.
5. Yeni NuGet paketi insan onayı olmadan eklenmez (`docs/security.md`, Dependencies).

## Deliberately out of scope
- Mikroservisler, ayrı frontend (SPA), mesaj kuyruğu.
- Kalıcı veritabanı — SQL Server'a geçiş ayrı bir spec + ADR ile gelir (OD-8).
- Kimlik doğrulama/yetkilendirme — ayrı spec + ADR; onay/ret akışından önce gelmek zorundadır (`docs/security.md`).
