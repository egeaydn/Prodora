# Prodora arayüz kontrolleri

Playwright ile yerel uygulamada gerçek Chromium kontrolleri. Testler 320–1440 px ekranları, sabit navbar/blur, mobil menü odağı ve kapanışını, footer gruplarını, JavaScript olmadan gezinmeyi, azaltılmış hareketi, kart hizalamasını ve form gönderim geri bildirimini kapsar.

Normal sayfalar yalnızca okunur. Yüklenme göstergesi testindeki POST tarayıcıda yakalanıp sentetik cevapla tamamlanır; gerçek giriş, e-posta, sipariş veya ödeme yapılmaz. Araç yalnızca localhost/loopback adreslerini kabul eder.

Kart hizalama kontrolü için ana sayfada en az üç ürün bulunmalıdır; araç katalog verisi eklemez.

## Çalıştırma

Bir terminalde güncel uygulamayı başlat:

```powershell
dotnet run --project Prodora.WebUI --configuration Release --launch-profile http --urls http://localhost:5097
```

İkinci terminalde, depo kökünden:

```powershell
Push-Location tools/Prodora.UiChecks
npm ci
$env:PLAYWRIGHT_BROWSERS_PATH = Join-Path (Get-Location) '../../artifacts/browser-cache'
npx playwright install chromium --only-shell
npm run check
Pop-Location
```

Başka bir yerel portta çalışan uygulama için önce `PRODORA_UI_BASE_URL` ortam değişkenini ayarla; varsayılan `http://localhost:5097` adresidir. Bu doğrulama Node.js 22 ve araçta kilitlenen Playwright sürümüyle yapıldı.

Ekran görüntüleri `artifacts/ui-previews` klasörüne, test tarayıcısı `artifacts/browser-cache` klasörüne yazılır. İkisi de Git dışında tutulur. Tarayıcı kontrolü tamamlanınca test tarayıcısı kapatılır; uygulama sunucusunu kendi terminalinden durdurabilirsin.
