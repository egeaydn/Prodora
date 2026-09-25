# Prodora arayüz yenilemesi

25 Eylül 2026

## Tasarım yaklaşımı

Amazon benzeri arama ve kategori odaklı mağaza düzeni; sade, sıcak ve düşük doygunluklu renkler. Beyaz logo/arama alanı, koyu gri kategori menüsü, açık kırık beyaz zemin, beyaz ürün kartları ve yumuşak kum rengi alışveriş düğmeleri kullanılır. Mevcut ürün fotoğrafları korunur.

- Ortak renkler, tipografi, boşluklar, formlar, tablolar ve mobil düzen: `Prodora.WebUI/wwwroot/css/storefront.css`.
- Ortak davranışlar: `Prodora.WebUI/wwwroot/js/storefront.js`.
- Tüm sayfaların çatısı: `Views/Shared/_Layout.cshtml`, `_navbar`, `_footer`, `_Icons`.
- Ürün kartları: `_products.cshtml`; yönetim ürün formu: `Views/Admin/_ProductFields.cshtml`.
- Sayfalara özel eski CSS dosyaları artık layout tarafından yüklenmez. Yeni font veya CDN bağımlılığı eklenmedi.
- Ana kırılım noktaları 1150, 850 ve 600 px. Mobilde kategoriler yatay, katalog iki sütun; ödeme ve hesap formları tek sütundur.
- Klavye odağı, içeriğe geç bağlantısı, etiketli formlar, erişilebilir ürün sekmeleri ve azaltılmış hareket tercihi desteklenir.

## Kapsanan ekranlar

Ana sayfa; arama, kategori ve ürün listesi; ürün detayı, galeri ve yorumlar; giriş, kayıt, şifre sıfırlama ve hesap; sepet, ödeme ve siparişler; ürün/kategori yönetiminin liste, oluşturma ve düzenleme ekranları; hakkımızda, yardım, iletişim, SSS, gizlilik, kullanım koşulları, site haritası, erişilebilirlik, alıntı ve hata ekranları.

İletişim sayfasındaki çalışmayan mesaj gönderme deneyimi kaldırıldı. Ekran artık mevcut yardım ve hesap akışlarına yönlendirir; aktif destek kanalı bulunmadığını belirtir.

## Eşlik eden işlev düzeltmeleri

- Arama ve sıralama sunucu tarafında çalışır. Kategori, arama, sıralama ve sayfa bilgileri korunur. Liste ve toplam kayıt sayısı aynı filtreyi kullanır.
- Ürün detay rotası ve kategori bağlantıları düzeltildi. Ana sayfa anonim ziyaretçiye açıktır.
- Ürün düzenleme mevcut görselleri korur; kategori seçimlerini günceller. Görsel yüklemelerine dosya türü ve boyut sınırı eklendi.
- Kategori güncellemesinin kaydı silen eski davranışı düzeltildi; yalnızca kategori adı güncellenir.
- Boş sepet ve görsel bulunamaması için ortak görünümler eklendi.
- Sipariş geçmişi güncel ürün fiyatı yerine sipariş anındaki fiyatı gösterir.
- Yönetim, hesap, sepet ve yorum yazma işlemlerine erişim kontrolleri; form işlemlerine ortak antiforgery kontrolü uygulandı.
- Şifre sıfırlamada eksik token yönlendirmesi ve bulunamayan kullanıcı durumu düzeltildi.

## MSSQL ve iyzico

Şema veya entity değişikliği yapılmadı; migration uygulanmadı. Mevcut ürün/kategori verileri başarıyla okundu.

Ürün tarafındaki sabit `sa` bağlantısı yerelde giriş hatası veriyordu. `DataContext` artık DI üzerinden `IDbContextFactory<DataContext>` ile yapılandırılır. Repository'ler işlem başına context oluşturma/kapama davranışını korur. Bağlantı `ConnectionStrings:CommerceConnection` ayarından, bu yoksa çalışan `IdentityConnection` ayarından alınır. Bağlantı parolası kaynak kodda tutulmaz.

iyzico sandbox adresi ve mevcut test ayarları değiştirilmedi. Ödeme ekranı test ortamını açıkça belirtir. Havale/EFT seçiminde kart alanları devre dışı kalır. Kart numarası ve CVV, hatalı gönderim sonrasında HTML'e geri yazılmaz. Ödeme toplamları API'ye kültürden bağımsız ondalık biçimde gönderilir.

Bu çalışma ödeme altyapısının kapsamlı yenilenmesi değildir. Önceki inceleme raporundaki canlı ödeme, e-posta yapılandırması ve diğer mimari bulgular ayrıca ele alınmalıdır.

## Doğrulama

- Çözüm derlemesi: 0 hata; mevcut nullable/eski paket uyarıları devam ediyor.
- 20 genel sayfa/arama senaryosu, 12 ürün detay adresi ve 8 kategori adresi HTTP 200 döndü.
- Fiyat artan sıralama 12 ürün üzerinde doğrulandı; arama ve boş sonuç görünümü kontrol edildi.
- Giriş gerektiren 6 adres giriş sayfasına yönlendiriyor. Olmayan ürün 404 döndürüyor.
- Geçersiz giriş formu alan hatalarını gösteriyor; antiforgery tokensız POST 400 ile reddediliyor.
- Katalogdaki 17 yerel kaynak başarıyla yüklendi; JavaScript sözdizimi kontrolü geçti.
- Hesap, sepet, kart/EFT ödeme, sipariş ve yönetim ekranları için 16 dolu/boş/oluşturma/düzenleme senaryosu örnek verilerle Razor üzerinden render edildi.
- Doğrulama sırasında sipariş, ödeme, e-posta gönderimi veya yönetim kaydı oluşturulmadı.
- Bu oturumda tarayıcı yüzeyi bulunmadığından gerçek tarayıcıda görsel/mobil ve JavaScript etkileşim kontrolü yapılamadı. HTTP ve Razor kontrolleri bu kontrolün yerine geçmez.

## Yerelde çalıştırma

```powershell
dotnet run --project Prodora.WebUI --launch-profile http
```

Adres: http://localhost:5047

Logo ve e-posta şablonları için [Marka ve e-posta notları](MARKA_VE_EPOSTA.md).
