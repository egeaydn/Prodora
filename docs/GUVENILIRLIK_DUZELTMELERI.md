# Mağaza güvenilirliği düzeltmeleri

26 Eylül 2026: ilk düzeltmeler (`fix/store-reliability`). 3 Ekim 2026: güvenlik ve CI güncellemesi (`fix/checkout-security-ci`).

## Öncelik sırası ve tamamlananlar

1. **Sipariş geçmişinin korunması:** Ürün silme yerine arşivleme ve yeniden yayına alma eklendi. Arşivlenen ürün mağazada görünmez; eski siparişlerde kalır. Sipariş kalemlerine ürün adı ve görseli kaydedilir. Satılmış ürünü fiziksel olarak silmeye karşı veritabanı ilişkisi artık `Restrict` kullanır.
2. **Hesap ve e-posta doğrulaması:** E-posta değişikliği mevcut şifreyi ve yeni adrese gönderilen bağlantıyı gerektirir. Onaylanana kadar eski adres geçerlidir. Kayıt hataları gösterilir; onay e-postası yeniden istenebilir. Aynı onay bağlantısı ikinci sepet oluşturmaz. E-posta gönderen işlemler IP başına beş dakikada beş istekle sınırlandırılır.
3. **Sepet ve stok:** Adet sınırı istek başına değil, sepetteki ürün toplamı için 1–99'dur. Stok dışı/arşivlenmiş ürün hem ekleme hem ödeme sırasında reddedilir. SQL Server işlem kilitleri eşzamanlı eklemelerin birbirini ezmesini engeller. Mevcut `Stock` alanı satışa uygunluk bilgisidir; adetli depo takibi eklenmedi.
4. **Sipariş ve ödeme tutarlılığı:** Her ödeme formunun tekil `RequestId` değeri vardır. Sipariş ödeme çağrısından önce kaydedilir; aynı istek tekrar ödeme başlatmaz. Tutar ve ürünler sunucudaki sepetten alınır. Gerçek sağlayıcı ödeme referansı saklanır. Sipariş sonucu ve sepet temizliği tek veritabanı işleminde tamamlanır. EFT siparişi ödeme bekler; ödenmiş sayılmaz.
5. **Belirsiz ödeme sonucu:** Ağ hatası veya eksik sağlayıcı yanıtı yeniden ödeme için güvenli kabul edilmez. Sepet ve yeni ödeme girişimleri bekletilir. Yönetici `/PaymentReview` ekranından iyzico sonucunu sorgular. Doğrulanmış başarı siparişi tamamlar; doğrulanmış başarısızlık yeniden denemeye izin verir. Başarısız bir sorgu tek başına kilidi kaldırmaz.
6. **Yorum akışı:** Eski adresler mevcut `_PartialComments` görünümünü kullanır. Eksik ürün/puan kontrol edilir. Kullanıcı adı Identity üzerinden kullanıcı kimliğine çevrilir; uygulanmamış kullanıcı adı DAL metotları kaldırıldı. Yorum hata yanıtları istisna veya stack trace içermez.
7. **Yerel gizli ayarlar:** SMTP ve iyzico anahtarları kaynak koddan çıkarıldı. Geliştirme ortamında `Prodora.WebUI/appsettings.Local.json` okunur; dosya Git'e ve publish çıktısına dahil edilmez. Ortam değişkenleri yerel dosyayı geçersiz kılabilir. Önceden kaynakta bulunan admin başlangıç ayarı da yerel dosyaya taşındı; mevcut kullanıcıların parolaları değiştirilmedi.

## Çalıştırma

Mevcut bilgisayardaki yerel ayarlar korundu. Yeni bir çalışma kopyasında `appsettings.Local.example.json` dosyasını `appsettings.Local.json` adıyla kopyalayıp SMTP ve **sandbox** anahtarlarını doldur. Mevcut yerel dosyanın üzerine yazma.

```powershell
dotnet run --project Prodora.WebUI --launch-profile http
```

Adres: `http://localhost:5047`. Yeni kodu almak için önceden çalışan uygulamayı yeniden başlat.

`CommerceConnection` tanımlanmazsa ticaret tabloları `IdentityConnection` veritabanını kullanır. Üretim ortamında yerel dosya okunmaz; örneğin `Smtp__Password`, `Smtp__UserName`, `Smtp__From`, `Iyzico__ApiKey`, `Iyzico__SecretKey` ortam değişkenlerini kullan. Ödeme adresi kodda yalnızca iyzico sandbox olarak sabitlenmiştir.

Geliştirmede e-posta bağlantıları yalnızca yerel (`localhost`/loopback) istek adresinden türetilir. Yayına çıkarken `Site__PublicBaseUrl=https://alan-adiniz.example` ve uygun `AllowedHosts` değeri gerekir; yapılandırma yoksa uygulama açılmaz. Hesap e-postası bağlantıları artık istekteki serbest `Host` başlığına göre gönderilmez. Daha önce Git'e alınmış yerel yapılandırma ve `Prodora.bak` bu dalda Git takibinden çıkarıldı; iki dosyanın yerel kopyası korundu.

## Migration ve yedek

Migration: `20260926170409_PreserveOrdersAndCheckout`. Yeni alanlar ve tekil istek indeksi ekler, ürün–sipariş silme davranışını değiştirir. Mevcut siparişlerin ürün adını/görselini mevcut katalogdan doldurur. Geçmişte silinmiş verileri veya geçmiş katalog değişikliklerini geri oluşturmaz; eski ödeme durumlarını tahmin ederek değiştirmez.

Yerel Prodora veritabanına uygulanmadan önce SQL Server'ın varsayılan yedek klasöründe `COPY_ONLY` ve `CHECKSUM` ile yedek alındı; `RESTORE VERIFYONLY` geçti. Bu kontrol, tam geri yükleme provası değildir. Migration sonrasında 53 ürün, 7 sipariş, 11 sipariş kalemi, 8 sepet, 4 sepet kalemi ve 13 yorum korundu.

Başka bir mevcut veritabanını kontrol etmek için:

```powershell
dotnet run --project tools/Prodora.StoreChecks -- --inspect
```

Yedek alarak uygulamak için:

```powershell
dotnet run --project tools/Prodora.StoreChecks -- --migrate
```

Bu yardımcı yalnızca yukarıdaki migration tek başına bekliyorsa günceller. Başka bir migration geçmişinde durur. Yedek oluşturma yetkisi gerekir; kayıt sayılarını önce/sonra karşılaştırır. Sıfırdan kurulum için README'deki iki DbContext migration komutlarını kullan.

## Tekrarlanabilir kontroller

İlk çalışmanın doğrulamasında 52 kontrol geçmişti. Son güvenlik güncellemesinde buna güvenilir bağlantı adresi ve sahte `Host` kontrolleri eklendi; kapsamlı paket **55 kontrol** içerir. WebUI dahil test projesi derlemesi **0 hata** ile tamamlandı ve model ile migration arasında fark bulunmadı. Gerçek uygulamada ana sayfa, ürün listesi, giriş, onay e-postası isteme, şifremi unuttum ve şifre yenileme sayfaları HTTP 200 döndü. Dört e-posta şablonunun önizleme/bağlantı kontrolleri geçti.

```powershell
dotnet run --project tools/Prodora.StoreChecks
dotnet run --project tools/Prodora.EmailPreview
```

StoreChecks gerçek SQL Server üzerinde rastgele adlandırılmış `Prodora_Checks_*` veritabanları oluşturur ve sonunda yalnızca kendi oluşturduğu veritabanlarını kaldırır. Test hesabı, e-posta alıcıları ve ödeme yanıtları sentetiktir. Gerçek SMTP veya iyzico çağrısı yapmaz. SQL kullanıcısının test veritabanı oluşturma/kaldırma yetkisi olmalıdır. GitHub Actions, SQL Server gerektirmeyen şifre yenileme ve e-posta şablonu kontrollerini her gönderimde çalıştırır; StoreChecks yerel SQL Server ile ayrıca çalıştırılır.

Kontroller migration ile eski siparişlerin korunmasını, fiziksel silme kısıtını, eşzamanlı sepet/ödeme isteklerini, stok ve adet kurallarını, EFT/başarısız/belirsiz ödemeleri, yönetici yetkilerini, ödeme sorgulamasını, gerçek MVC antiforgery davranışını, kayıt/onay/e-posta değişikliği/şifre yenilemeyi ve yorum adreslerini kapsar.

E-posta önizlemeleri `artifacts/email-previews/index.html` içinde: hesap onayı, şifre yenileme, yeni e-posta adresi onayı ve genel bildirim. Genel bildirim yalnızca şablondur; yeni bir otomatik gönderim tetikleyicisi eklenmedi.

## Sınırlar

- Doğrulamalar gerçek banka/iyzico sandbox uçtan uca ödeme testi veya gerçek posta kutusunda teslimat testi değildir. Sağlayıcı çağrısı sözleşmeleri için [iyzico ödeme sorgulama dokümanı](https://docs.iyzico.com/en/advanced/retrieve-payment) ve [resmî .NET SDK](https://github.com/iyzico/iyzipay-dotnet) esas alındı.
- Sağlayıcı işlemi doğrulayamıyorsa otomatik kilit açılmaz. Yönetici sandbox paneliyle karşılaştırmalıdır; doğrulanmamış işlemi ödenmemiş sayan bir düğme eklenmedi.
- Kullanılmayan `Microsoft.CrmSdk.CoreAssemblies` bağımlılığı kaldırıldı. Derlemede önceden mevcut nullability uyarıları sürüyor.
- `appsettings.Local.json` ile `Prodora.bak` Git takibinden çıkarıldı; yerel dosyalar korundu. Eski Git commitlerinde bulunan SMTP uygulama parolası/sandbox anahtarları ve eski yedek bu işlemle geçmişten silinmez. Daha önce paylaşılmış anahtarlar sağlayıcı tarafında yenilenmelidir.
