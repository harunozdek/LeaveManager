# ADR 0001 — Tek projeli, katmanlı modüler monolit

- Status: Accepted
- Date: 2026-10-23

## Context
LeaveManager 2016'dan kalma, tek dosyalık (`Program.cs`) bir Minimal API uygulaması: endpoint'ler,
bellek içi veri, iş mantığı ve HTML aynı yerde. Test yok. İzin hesapları (hak, kıdem, gün) hukuken
hassas ve bugün `DateTime.Now`'a bağlı olduğu için deterministik test edilemiyor. Bootstrap
mülakatında hedef mimari kararlaştırıldı.

## Decision
Uygulama tek proje olarak kalır; içi Domain / Application / Persistence / Web katmanlarına
namespace ve klasörle ayrılır, bağımlılık yönü Web → Application → (Domain, Persistence) olur.
Ayrıştırma karakterizasyon testlerinden sonra `/refactor` ile yapılır.

## Consequences
- Kazanç: Domain saf ve `TimeProvider` ile deterministik test edilebilir; kalıcılık değişimi yalnızca Persistence'ı etkiler.
- Maliyet: sınırlar derleyici tarafından değil, mimari testle korunur — o test refactor'a kadar yok, arada sınırlar yalnızca review ile korunur.
- Maliyet: refactor için önce kapsamlı karakterizasyon testi yazılması gerekir.

## Alternatives considered
- **Ayrı projeler (Domain.csproj, Web.csproj …):** derleyici sınırları korur, ama ~200 satırlık bir uygulama için tören; ileride büyürse geçiş kolay.
- **Olduğu gibi bırakmak (tek dosya):** değişim maliyeti yok, ama izin hesapları test edilemez ve tutarsızlıklar (ör. iki farklı bakiye kontrolü) büyümeye devam eder.

## Revisit triggers
- Uygulama birden fazla ekip tarafından geliştirilmeye başlarsa ya da mimari test sık ihlal yakalarsa → ayrı projeler.
- Başka sistemlerin LeaveManager verisini doğrudan kullanması gerekirse → API sınırı ADR'si.
