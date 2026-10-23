# Conventions

Kurallar **yeni ve değiştirilen kod** için geçerlidir. `Program.cs`'teki mevcut ihlaller kendiliğinden
düzeltilmez; `/refactor` ya da `/change` kalemi olarak ele alınır.

## Language
- Chat language (interviews, sessions): Turkish (tr)
- Document language (docs/, specs, plans, ADRs, review and verify reports): Turkish (tr)
- Always English (protocol, not prose): ANEW core files, template headings and field labels,
  `Status:` values (Draft / Approved / In progress / Shipped), `Approved by / on:`, `Source:`.
- Code identifiers, branch names, commit messages: English. Kullanıcıya görünen metinler Türkçe
  (doğru Türkçe karakterlerle — mevcut ASCII metinler bir `/change` ile düzeltilir).
- Agent messages (handoff summaries, refusals, `Next:` lines): chat language; command names, file
  paths and protocol fields verbatim.

## Language & framework versions
- Hedef: `net8.0`, ASP.NET Core Minimal API. Derleme makinede ve CI'da .NET 10 SDK ile; testler .NET 8 runtime'ında koşar.
- **Dikkat:** .NET 8'in desteği 2026-11-10'da biter — `net10.0`'a geçiş açık karardır (ayrı spec + ADR).
- `Nullable` = disable, `TreatWarningsAsErrors` kapalı — ikisi de refactor ile açılacak (açık karar).
- `global.json` yok (SDK sabitlenmez).

## Naming
- .NET standardı: tipler, metotlar, property'ler PascalCase; yereller ve parametreler camelCase; private alanlar `_camelCase`.
- Domain terimlerinin İngilizce adları `docs/domain.md` tablosundaki kod adlarıdır (`Entitlement`, `UsedDays`, …); eşanlamlı uydurulmaz.
- Testler: `tests/LeaveManager.Tests/`, sınıf `<Subject>Tests`, metot `Method_Condition_Expected`.
- Branch ve commit: `docs/git.md`.

## Error handling
- **Boş `catch` yasak.** Bir istisna ya `ILogger` ile loglanır ve kullanıcıya genel bir Türkçe mesaj
  gösterilir, ya da hiç yakalanmaz (ASP.NET Core'un hata işleyicisine bırakılır).
- İş kuralı ihlalleri (yetersiz bakiye, geçersiz tarih) istisna değil, **sonuç değeri** olarak döner;
  kullanıcıya anlaşılır Türkçe mesajla gösterilir, form verisi kaybolmaz. Kural ihlali asla sessiz yönlendirmeye dönüşmez.
- Kullanıcıya asla stack trace, istisna mesajı, dahili hata kodu ya da dahili telefon numarası gösterilmez.
- Metotlar başarısızlıkta `true` döndürmez; dönüş değeri anlamını korur.

## Data rules
- İzin tarihleri `DateOnly` (saat bileşeni anlamsız) — mevcut `DateTime` refactor ile geçer.
- "Bugün" yalnızca `TimeProvider` ile alınır; takvim **Europe/Istanbul** yerel tarihidir. `DateTime.Now` yeni kodda yasak.
- Gün sayıları `int`. Yarım gün gelirse ayrı karar + ADR.
- Durum değerleri tek bir yerde tanımlanır (hedef: `enum`); magic string tekrarlanmaz. Mevcut string
  değerler (`Beklemede` / `Onaylandi` / `Reddedildi`) veri ve görünen davranış olarak korunur.
- HTML'e basılan her dinamik değer encode edilir (`docs/security.md`).

## Enforced by tooling
- Derleyici: C# derleme hataları (`build` adımı).
- `.editorconfig` + `dotnet format --verify-no-changes` (whitespace ve stil, iki proje için) — `scripts/check` `format` adımı.
- Review'da kontrol edilen (henüz araçla değil): boş catch yasağı, `DateTime.Now` yasağı, katman kuralları
  (`docs/architecture.md`; mimari test refactor ile gelecek).
