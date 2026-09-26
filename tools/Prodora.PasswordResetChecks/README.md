# Şifre yenileme kontrolü

Proje ana klasöründe:

```powershell
dotnet run --project tools/Prodora.PasswordResetChecks
```

Kontrol programı gerçek AccountController, Razor görünümü, Identity şifre/token sağlayıcısı, oturum çerezi yapılandırması ve antiforgery doğrulamasını geçici bir yerel HTTP sunucusunda çalıştırır. Kullanıcı deposu bellektedir; SQL Server'a bağlanmaz, e-posta göndermez ve gerçek kullanıcı şifrelerini değiştirmez. Sunucu kontrollerden sonra kapanır.

HTTP istemcisi, e-postadan gelen üst düzey gezinmede çerezleri SameSite politikasına göre göndererek senaryoyu modeller. Bu kontrol gerçek tarayıcı otomasyonu değildir.

## Hatanın yeniden üretimi

Önceki `SameSite=Strict` ayarıyla, e-posta bağlantısını açan oturum sahibi kullanıcının GET isteği anonim görünür. Form gönderiminde oturum çerezi yeniden gelir; anonim kimlik için üretilen antiforgery tokenı reddedilir. Eski ayarla test sonucu `POST=400`, `SameSite=Lax` ile `POST=302` ve başarılı şifre değişimidir.

Çerez ayarı `ApplicationCookieSettings.Configure` üzerinden hem uygulama hem kontrol programı tarafından kullanılır. Antiforgery filtresi açık kalır.

## Kontroller

- Oturum açıkken e-posta bağlantısından şifre yenileme.
- Oturumsuz kullanıcıyla yenileme.
- Zayıf şifre ve kullanılmış reset tokenının reddedilmesi; şifrenin korunması.
- Eski Strict çerezi veya oturum değişikliğiyle açılan formun güvenli biçimde yenilenmesi.
- Yenilenen formun tekrar gönderiminde başarılı şifre değişikliği.
- Reddedilen şifrenin HTML yanıtına geri yazılmaması.
- Antiforgery tokensız isteğin 400 ile reddedilmesi.
- Eksik reset tokenının anlaşılır doğrulama mesajı / yeni bağlantı yönlendirmesi.
- Şifre formunun önbelleğe alınmaması.

Eski formun CSRF doğrulaması başarısızsa işlem yapılmaz; HTTP 400 korunur fakat boş hata sayfası yerine yeni form tokenı ve tekrar deneme açıklaması gösterilir. Kullanıcının şifreyi yeniden yazması gerekir. Önceki sürümden kalan Strict çerezleri için de bu kurtarma yolu geçerlidir.

Microsoft'un [antiforgery açıklaması](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-8.0), form tokenının kullanıcı kimliğine bağlı olduğunu ve oturum değişikliklerinin doğrulamayı etkilediğini açıklar.
