# Marka ve e-posta şablonları

## Logo

Marka yazımı **.Prodora**: başta sarı-turuncu nokta (`#efb33f`), ardından siyah yazı (`#191919`). Segoe UI Bold çizgilerinden üretilen SVG font yüklemesine ihtiyaç duymaz.

- `Prodora.WebUI/wwwroot/brand/prodora-wordmark.svg`: asıl yatay logo.
- `prodora-wordmark.png`: aynı logonun raster kopyası.
- `prodora-icon.svg`: küçük alanlar için .P simgesi.
- `apple-touch-icon.png`: 180 × 180 cihaz simgesi.
- `wwwroot/favicon.ico`: 32 × 32 tarayıcı simgesi.

Üst menü ve footer `_BrandLogo.cshtml` bileşenini kullanır. Hesap sayfalarının yan alanı da aynı logoyu içerir. Ortak layout, tüm sayfalarda favicon ve Apple simgesini yükler. Siyah logonun okunması için üst alan beyaz, kategori menüsü koyu bırakılmıştır.

Vektör ve raster dosyaları Windows'ta `scripts/Generate-BrandAssets.ps1` ile yeniden üretilebilir; Segoe UI gerektirir.

## Mevcut e-posta türleri

1. **Hesap onayı:** yeni kayıt sonrasında gönderilir. Yumuşak yeşil başlık alanı, karşılama metni ve hesabı onaylama düğmesi.
2. **Şifre yenileme:** şifremi unuttum isteğinde gönderilir. Krem başlık alanı, yeni şifre düğmesi ve isteği kullanıcı yapmadıysa ne olacağını açıklayan metin.

Sipariş, kargo veya ödeme e-postası tetikleyicisi mevcut değil; bu çalışmada eklenmedi. Eski hesap düzenleme kodundaki hiçbir zaman çalışmayan şifre e-postası bloğu kaldırıldı. Genel `Notification` şablonu ve mevcut `SendModernEmail` yardımcı metodu gelecekteki bildirimler için hazırdır.

## Yapı

`EmailServices/EmailTemplates.cs` konu, HTML ve düz metin sürümünü birlikte üretir. İsim ve dinamik metinler HTML olarak kodlanır; işlem adresleri yalnızca mutlak HTTP/HTTPS bağlantıları kabul eder. Hesap bağlantıları hâlâ çalışmakta olan uygulamanın adresi ve portundan oluşturulur.

E-postalar tablo düzeni ve satır içi stiller kullanır; mobil aralıklar için ek medya sorgusu bulunur. İşlem düğmesinin altında kopyalanabilir bağlantı vardır. Logo e-postada aynı siyah/altın renklerle **metin olarak** çizilir; görüntüler yüklenmese de marka adı görünür. Sitedeki logo gerçek SVG'dir.

`MailHelper` UTF-8 ve alternatif `text/plain` / `text/html` gövdeleri gönderir, mesaj kaynaklarını kapatır. SMTP ayarları değiştirilmedi. Gönderim başarısızsa kullanıcıya başarılı gönderim mesajı gösterilmez.

## Önizleme ve kontrol

Proje ana klasöründe:

```powershell
dotnet run --project tools/Prodora.EmailPreview
```

`artifacts/email-previews/index.html` dosyasını tarayıcıda aç. Üç şablon ve masaüstü/mobil genişlik seçimi vardır. Bu araç gerçek şablon kodunu kullanır; SMTP, veritabanı veya gerçek kullanıcı hesabına bağlanmaz. Geçersiz örnek token kullanır.

Araç ayrıca dinamik metinlerin güvenli kodlanmasını, işlem/fallback bağlantılarında tokenın korunmasını, düz metin sürümünü ve geçersiz bağlantıların reddedilmesini kontrol eder. Üretilen dosyalar git tarafından dışlanan `artifacts` dizinindedir.

Bu oturumda tarayıcı yüzeyi bulunmadığından gerçek Gmail/Outlook/mobil istemci görsel kontrolü yapılamadı. Logo raster önizlemeleri incelendi; şablon kontrolleri, çözüm derlemesi ve HTTP kaynak kontrolleri yapıldı. Gerçek e-posta gönderilmedi.
