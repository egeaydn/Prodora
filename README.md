# Prodora

![Prodora Banner](https://img.shields.io/badge/.NET%20Core-6.0-blue?style=for-the-badge) ![MVC](https://img.shields.io/badge/MVC-Pattern-green?style=for-the-badge) ![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)

Prodora, modern e-ticaret ihtiyaçlarına yönelik olarak geliştirilmiş, güçlü ve esnek bir yönetim panelidir. Katmanlı mimarisi, güvenli kimlik doğrulama sistemi ve kullanıcı dostu arayüzüyle hem yöneticilere hem de son kullanıcılara kusursuz bir deneyim sunar.

---

## 🚀 Özellikler

- **Katmanlı Mimari:** Temiz, sürdürülebilir ve test edilebilir kod yapısı.
- **Gelişmiş Ürün Yönetimi:** Ürün ekleme, güncelleme, silme ve detaylı ürün kategorilendirme.
- **Dinamik Kategori Sistemi:** Kategoriler arası ilişki yönetimi ve hızlı erişim.
- **Güçlü Kimlik Doğrulama:** E-posta ile hesap onayı, şifre sıfırlama ve güvenli oturum yönetimi.
- **Modern UI/UX:** Responsive ve sezgisel arayüz, özelleştirilebilir tema desteği.
- **Rol Bazlı Yetkilendirme:** Admin ve kullanıcı rolleriyle güvenli erişim.
- **Gelişmiş Filtreleme & Arama:** Ürünler üzerinde hızlı filtreleme ve arama fonksiyonları.
- **E-posta Bildirimleri:** Hesap aktivasyonu ve şifre işlemleri için otomatik e-posta gönderimi.
- **Unit Test Desteği:** Sağlam ve güvenilir kod için test altyapısı.

---

## 🛠️ Teknolojiler

- **Backend:** ASP.NET Core MVC, Entity Framework Core
- **Frontend:** Bootstrap 5, jQuery, CSS3, HTML5
- **Veritabanı:** SQL Server (EF Core ile Code-First)
- **Kimlik Yönetimi:** ASP.NET Core Identity
- **E-posta Servisi:** SMTP (Gmail)
- **Diğer:** LINQ, AutoMapper, Layered Architecture

---

## 📦 Kurulum

1. **Projeyi Klonlayın:**
   ```sh
   git clone https://github.com/egeaydn/prodora.git
   cd prodora
   ```

2. **Bağımlılıkları Yükleyin:**
   - .NET 6.0 veya 8.0 SDK ve Node.js yüklü olmalı.
   - NuGet paketlerini ve npm modüllerini yükleyin:
     ```sh
     dotnet restore
     cd Prodora.WebUI
     npm install
     ```

3. **Veritabanını Oluşturun:**
   ```sh
   dotnet ef database update --project Prodora.DataAccess
   ```

4. **Projeyi Çalıştırın:**
   ```sh
   dotnet run --project Prodora.WebUI
   ```

---

## 👤 Kullanıcı Rolleri

- **Admin:** Ürün ve kategori yönetimi, kullanıcı yönetimi, sistem ayarları.
- **Kullanıcı:** Ürünleri görüntüleme, sepete ekleme, sipariş verme, profil yönetimi.

---

## 📸 Ekran Görüntüleri

> Modern ve şık arayüz, kolay yönetim paneli ve kullanıcı dostu alışveriş deneyimi!

![Dashboard](https://via.placeholder.com/900x300?text=Prodora+Dashboard)
![Product List](https://via.placeholder.com/900x300?text=Product+List)
![Category Management](https://via.placeholder.com/900x300?text=Category+Management)

---

## 📄 Lisans

Bu proje [MIT Lisansı](https://opensource.org/licenses/MIT) ile lisanslanmıştır.

---

## 💡 Katkıda Bulunmak

Katkılarınızı memnuniyetle karşılıyoruz! Lütfen önce bir [issue](https://github.com/egeaydn/prodora/issues) açın ve ardından bir pull request gönderin.

---

## 📬 İletişim

Her türlü soru ve öneriniz için:  
**E-posta:** egeaydinn31@gmail.com 
**LinkedIn:** [linkedin.com/company/prodora]([https://linkedin.com/company/prodora](https://www.linkedin.com/in/ege-ayd%C4%B1n-156704317/))

---

> **Prodora** – Güçlü, modern ve güvenli e-ticaret yönetimi.
