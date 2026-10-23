# Domain

İki bölüm vardır ve karıştırılmaz: **BR-n = kodun bugünkü davranışı** (kusurlarıyla birlikte;
karakterizasyon testleri bunlara bağlanır) ve **OD-n = açık kararlar** (kodun yasa, yorum ya da
kendisiyle çeliştiği yerler). Bir OD kapatıldığında `/change` ile ilgili BR güncellenir.
Kaynak: `Program.cs` incelemesi, bootstrap 2026-10-23.

## Ubiquitous language

| Term | Meaning | Notes / not to be confused with |
|---|---|---|
| Personel (`Employee`) | İzin hakkı olan çalışan: ad, birim, işe giriş tarihi | — |
| Birim (`Department`) | Personelin bölümü (IT, Muhasebe, IK, Satis) | Yetki/rol değildir |
| İzin talebi (`LeaveRequest`) | Bir personelin başlangıç–bitiş tarihli izin isteği ve nedeni | Onaylanmış izinle aynı şey değildir |
| Durum (`Status`) | `Beklemede` / `Onaylandi` / `Reddedildi` — yalnızca bu üç değer (kodda ASCII yazılır) | Arayüzden değiştirilemez (BR-9) |
| Kıdem (`Seniority`) | İşe girişten bugüne hizmet yılı | Bugün yıl farkıdır, tam yıl değil (BR-2, OD-2) |
| Hak (`Entitlement`) | Kıdeme göre bir yıllık izin gün sayısı | Devreden izin modellenmez (OD-7) |
| Talep günü (`CalcDays`) | Bitiş − Başlangıç + 1 | Bugün **takvim günü**; yorumdaki "iş günü" değil (OD-3) |
| Kullanılan (`UsedDays`) | **Yalnızca onaylanmış** taleplerin talep günü toplamı | Bekleyen talepler dahil değil (OD-5) |
| Kalan (`Remaining`) | Hak − Kullanılan | Negatif olabilir |

## Business rules
Mevcut davranış — "doğru" olduğu iddia edilmez, yalnızca gözlenmiştir.
- **BR-1** Hak tablosu (kıdem yılına göre): `< 1` → 0 gün; `1–5` → 14; `6–15` → 20; `> 15` → 26.
- **BR-2** Kıdem = (bugünün yılı − işe giriş yılı); ay ve gün dikkate alınmaz; "bugün" = `DateTime.Now`.
- **BR-3** Talep günü = (bitiş − başlangıç) gün farkı + 1; hafta sonu ve tatiller dahil sayılır.
- **BR-4** Kullanılan = durumu `Onaylandi` olan taleplerin talep günü toplamı.
- **BR-5** Kalan = Hak − Kullanılan.
- **BR-6** Yeni talep (`POST /new`): Kalan < talep günü ise "ERR-412 bakiye yetersiz" mesajı gösterilir
  ve kayıt yapılmaz. Değilse servis tekrar kontrol eder: Kalan ≤ talep günü ise **sessizce kaydetmez**
  ve `/requests`'e yönlendirir; aksi halde talep `Beklemede` durumunda, artan Id ile kaydedilir.
- **BR-7** Geçmiş tarihli talepler ve bitişi başlangıçtan önce olan talepler reddedilmez.
- **BR-8** Ayrıştırılamayan form verisi ya da bilinmeyen personel: kayıt yapılmaz, hata gösterilmeden
  `/requests`'e yönlendirilir.
- **BR-9** Durum değişikliği (onay/ret) için arayüz yoktur; durumlar yalnızca seed verisinde farklıdır.
- **BR-10** Uygulama her başlangıçta seed verisiyle (5 personel, 8 talep) başlar; yeniden başlatmada
  sonradan girilen talepler kaybolur.

## Open decisions (karar sahibi: İK; yasal olanlar hukukla)
- **OD-1 Hak tablosu.** Kod 1–5 yıl için 14 gün verirken yorum "15 gün" diyor. 4857 sayılı İş Kanunu
  md. 53'e göre: 1–5 yıl (5 dahil) en az 14, 5–15 yıl arası en az 20, **15 yıl dahil ve üzeri** en az
  26 gün; kod 15. yılda 20 veriyor. Yasa ayrıca 18 yaş ve altı ile 50 yaş ve üstüne en az 20 gün
  öngörür — modellenmemiş. (Yasal sınırlar İK/hukuk tarafından doğrulanmalı.)
- **OD-2 Kıdem hassasiyeti.** Yıl farkı mı, işe giriş yıldönümüne göre tam yıl mı?
- **OD-3 Gün hesabı.** Takvim günü mü iş günü mü? Hafta tatili ve resmi tatiller izinden sayılmalı mı (İş Kanunu md. 56)?
- **OD-4 Bakiye eşitliği.** Kalan = talep günü olduğunda talep kabul mü? Bugün endpoint `<`, servis `<=`
  kullanıyor; sonuç sessiz ret. Tek kural, tek yer, kullanıcıya görünür mesaj gerekiyor.
- **OD-5 Bekleyen talepler** bakiyeden düşülsün mü? (2018'den beri kararsız.)
- **OD-6 Geriye dönük ve ters tarih.** İK'nın geçmiş tarihli giriş ihtiyacı; bitiş < başlangıç reddedilmeli mi?
- **OD-7 Devreden izin.** Yorum satırındaki yıl sonu devir (en çok 5 gün) kuralı istenecek mi?
- **OD-8 Kalıcılık.** Veriler bugün yeniden başlatmada kayboluyor; SQL Server'a geçiş ne zaman?

## Key domain invariants
- Bir talebin durumu yalnızca `Beklemede`, `Onaylandi`, `Reddedildi` değerlerinden biridir.
- Kullanılan gün yalnızca onaylanmış taleplerden hesaplanır (OD-5 kapanana kadar).
- Hedef değişmez (bugün **ihlal ediliyor**, BR-7 / OD-6): bitiş tarihi başlangıç tarihinden önce olamaz.
