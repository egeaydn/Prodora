# 🛍️ PRODORA - Modern E-Ticaret Platformu

<div align="center">

![Prodora Logo](https://img.shields.io/badge/🛍️-PRODORA-FF6000?style=for-the-badge&labelColor=000000)

**🚀 Next Generation E-Commerce Platform with Advanced Features**

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://docs.microsoft.com/en-us/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

![GitHub Stars](https://img.shields.io/github/stars/egegeegege/Prodora?style=social)
![GitHub Forks](https://img.shields.io/github/forks/egegeegege/Prodora?style=social)
![GitHub Issues](https://img.shields.io/github/issues/egegeegege/Prodora?style=social)

</div>

---

## 📸 Ekran Görüntüleri

<div align="center">

### 🏠 Ana Sayfa
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144223.png" alt="Ana Sayfa" width="45%" style="border-radius: 10px; margin: 5px;">
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144250.png" alt="Ürün Listesi" width="45%" style="border-radius: 10px; margin: 5px;">

### 🛒 Ürün Detay & Sepet
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144312.png" alt="Ürün Detay" width="45%" style="border-radius: 10px; margin: 5px;">
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144341.png" alt="Sepet" width="45%" style="border-radius: 10px; margin: 5px;">

### 👨‍💼 Admin Panel & Kullanıcı Hesabı
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144410.png" alt="Admin Panel" width="45%" style="border-radius: 10px; margin: 5px;">
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144435.png" alt="Kullanıcı Hesabı" width="45%" style="border-radius: 10px; margin: 5px;">

### 💳 Ödeme Sistemi
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/Ekran görüntüsü 2025-09-10 144459.png" alt="Ödeme" width="45%" style="border-radius: 10px; margin: 5px;">

</div>

---

## 🎯 Proje Özeti

**Prodora**, modern e-ticaret ihtiyaçlarına yönelik geliştirilmiş, tam özellikli bir online alışveriş platformudur. Katmanlı mimari (N-Tier Architecture) kullanılarak geliştirilmiş, güvenli ve ölçeklenebilir bir yapıya sahiptir.

---

## 🏗️ Mimari Yapı

<div align="center">

```mermaid
graph TB
    subgraph "🎨 Presentation Layer"
        UI[Web UI - ASP.NET Core MVC]
        CTRL[Controllers]
        VIEWS[Views & Models]
        API[Web API Endpoints]
    end
    
    subgraph "⚡ Business Layer"
        BL[Business Logic]
        SERV[Services]
        VAL[Validation Rules]
        MGR[Managers]
    end
    
    subgraph "💾 Data Access Layer"
        DAL[Data Access]
        REPO[Repository Pattern]
        EF[Entity Framework Core]
        MIGR[Migrations]
    end
    
    subgraph "🏛️ Entity Layer"
        ENT[Entities]
        PROD[Product]
        CAT[Category]
        USER[User]
        ORDER[Order]
    end
    
    subgraph "🗄️ Database"
        SQL[(SQL Server)]
        IDENT[(Identity DB)]
    end
    
    UI --> CTRL
    CTRL --> SERV
    SERV --> REPO
    REPO --> EF
    EF --> SQL
    EF --> IDENT
    
    VIEWS -.-> UI
    MGR -.-> BL
    VAL -.-> BL
    DAL -.-> REPO
    MIGR -.-> EF
    ENT -.-> PROD
    ENT -.-> CAT
    ENT -.-> USER
    ENT -.-> ORDER
```

</div>

---

## 💻 Teknoloji Stack'i

<div align="center">

### �️ Backend Teknolojileri

| Teknoloji | Versiyon | Kullanım Alanı | Açıklama |
|-----------|----------|----------------|-----------|
| ![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat&logo=.net) | **6.0** | 🏗️ **Core Framework** | Ana uygulama framework'ü |
| ![C#](https://img.shields.io/badge/C%23-11.0-239120?style=flat&logo=c-sharp) | **11.0** | 💻 **Programming Language** | Ana programlama dili |
| ![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-6.0-512BD4?style=flat&logo=.net) | **6.0** | 🌐 **Web Framework** | MVC pattern, Web API |
| ![Entity Framework](https://img.shields.io/badge/EF%20Core-6.0-512BD4?style=flat&logo=.net) | **6.0** | 🗄️ **ORM** | Code-First, Migration, LINQ |
| ![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server) | **2019+** | 💾 **Database** | İlişkisel veritabanı |
| ![ASP.NET Identity](https://img.shields.io/badge/Identity-Core-512BD4?style=flat&logo=.net) | **6.0** | 🔐 **Authentication** | Kullanıcı yönetimi ve güvenlik |

### 🎨 Frontend Teknolojileri

| Teknoloji | Versiyon | Kullanım Alanı | Açıklama |
|-----------|----------|----------------|-----------|
| ![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat&logo=html5&logoColor=white) | **5** | 📝 **Markup** | Semantic HTML yapısı |
| ![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat&logo=css3) | **3** | 🎨 **Styling** | Modern CSS, Flexbox, Grid |
| ![JavaScript](https://img.shields.io/badge/JavaScript-ES6+-F7DF1E?style=flat&logo=javascript&logoColor=black) | **ES6+** | ⚡ **Scripting** | DOM manipulation, AJAX |
| ![Bootstrap](https://img.shields.io/badge/Bootstrap-5.2-563D7C?style=flat&logo=bootstrap) | **5.2** | 📱 **UI Framework** | Responsive design, Components |
| ![jQuery](https://img.shields.io/badge/jQuery-3.6-0769AD?style=flat&logo=jquery) | **3.6** | 🔧 **Library** | DOM operations, AJAX calls |
| ![Font Awesome](https://img.shields.io/badge/Font%20Awesome-6.0-339AF0?style=flat&logo=font-awesome) | **6.0** | 🎭 **Icons** | Modern icon library |

### 🔧 Geliştirme Araçları

| Araç | Kullanım Alanı | Açıklama |
|------|----------------|-----------|
| ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2022-5C2D91?style=flat&logo=visual-studio) | 🛠️ **IDE** | Ana geliştirme ortamı |
| ![SQL Server Management Studio](https://img.shields.io/badge/SSMS-18+-CC2927?style=flat&logo=microsoft-sql-server) | 🗄️ **Database Tool** | Veritabanı yönetimi |
| ![Git](https://img.shields.io/badge/Git-F05032?style=flat&logo=git&logoColor=white) | 📝 **Version Control** | Kaynak kod yönetimi |
| ![NuGet](https://img.shields.io/badge/NuGet-004880?style=flat&logo=nuget) | 📦 **Package Manager** | .NET paket yönetimi |

</div>

---

## 🚀 Özellikler

<div align="center">

### 👥 Kullanıcı Özellikleri

</div>

| Özellik | Durum | Açıklama |
|---------|-------|----------|
| 🔐 **Kullanıcı Kayıt/Giriş** | ✅ | E-posta doğrulamalı hesap oluşturma |
| 🛍️ **Ürün Görüntüleme** | ✅ | Kategorilere göre filtreleme ve arama |
| 🛒 **Sepet İşlemleri** | ✅ | Ürün ekleme, çıkarma, güncelleme |
| 💳 **Ödeme Sistemi** | ✅ | Iyzico entegrasyonu ile güvenli ödeme |
| 📝 **Yorum & Değerlendirme** | ✅ | Ürünlere yorum yapma ve puanlama |
| 📧 **E-posta Bildirimleri** | ✅ | Hesap doğrulama ve sipariş bildirimleri |
| 📱 **Responsive Tasarım** | ✅ | Mobil uyumlu modern arayüz |
| 🔍 **Gelişmiş Arama** | ✅ | Fiyat aralığı ve kategori filtreleri |

<div align="center">

### 👨‍💼 Admin Özellikleri

</div>

| Özellik | Durum | Açıklama |
|---------|-------|----------|
| 📊 **Dashboard** | ✅ | Satış istatistikleri ve genel bakış |
| 🏷️ **Ürün Yönetimi** | ✅ | CRUD işlemleri, resim yükleme |
| 📂 **Kategori Yönetimi** | ✅ | Kategori ekleme, düzenleme, silme |
| 👥 **Kullanıcı Yönetimi** | ✅ | Rol bazlı yetki kontrolü |
| 📋 **Sipariş Takibi** | ✅ | Sipariş durumu güncelleme |
| 💬 **Yorum Moderasyonu** | ✅ | Yorumları onaylama/reddetme |
| 📈 **Raporlama** | ✅ | Satış ve kullanıcı raporları |
| 🔧 **Sistem Ayarları** | ✅ | Genel sistem konfigürasyonu |

---

## 📁 Proje Yapısı

<div align="center">

```
📦 Prodora Solution
├── 🏛️ Prodora.Entitys                 # Entity Layer
│   ├── 📄 Product.cs                  # Ürün entity'si
│   ├── 📄 Category.cs                 # Kategori entity'si
│   ├── 📄 Order.cs                    # Sipariş entity'si
│   ├── 📄 Comment.cs                  # Yorum entity'si
│   ├── 📄 Basket.cs                   # Sepet entity'si
│   └── 📄 Image.cs                    # Resim entity'si
├── 💾 Prodora.DataAccess              # Data Access Layer
│   ├── 📁 Abstract/                   # Interface'ler
│   │   ├── 📄 IRepository.cs          # Generic repository
│   │   ├── 📄 IProductDal.cs          # Ürün data access
│   │   ├── 📄 ICategoryDal.cs         # Kategori data access
│   │   └── 📄 ...                     # Diğer DAL interface'leri
│   ├── 📁 Concrate/EfCore/            # EF Core implementasyonları
│   │   ├── 📄 DataContext.cs          # DbContext
│   │   ├── 📄 EfCoreProductDal.cs     # Ürün EF implementasyonu
│   │   ├── 📄 EfCoreCategoryDal.cs    # Kategori EF implementasyonu
│   │   └── 📄 ...                     # Diğer EF implementasyonları
│   └── 📁 Migrations/                 # EF Core migration'ları
├── ⚡ Prodora.Business                # Business Layer
│   ├── 📁 Abstract/                   # Service interface'leri
│   │   ├── 📄 IProductServices.cs     # Ürün service interface
│   │   ├── 📄 ICategoryServices.cs    # Kategori service interface
│   │   └── 📄 ...                     # Diğer service interface'leri
│   └── 📁 Concrate/                   # Service implementasyonları
│       ├── 📄 ProductManager.cs       # Ürün business logic
│       ├── 📄 CategoryManager.cs      # Kategori business logic
│       └── 📄 ...                     # Diğer manager sınıfları
└── 🌐 Prodora.WebUI                   # Presentation Layer
    ├── 📁 Controllers/                # MVC Controller'lar
    │   ├── 📄 HomeController.cs       # Ana sayfa
    │   ├── 📄 ShopController.cs       # Mağaza işlemleri
    │   ├── 📄 AccountController.cs    # Hesap işlemleri
    │   ├── 📄 AdminController.cs      # Admin panel
    │   └── 📄 BasketController.cs     # Sepet işlemleri
    ├── 📁 Views/                      # Razor View'lar
    │   ├── 📁 Home/                   # Ana sayfa view'ları
    │   ├── 📁 Shop/                   # Mağaza view'ları
    │   ├── 📁 Account/                # Hesap view'ları
    │   ├── 📁 Admin/                  # Admin view'ları
    │   └── 📁 Shared/                 # Paylaşılan view'lar
    ├── 📁 Models/                     # ViewModel'ler
    ├── 📁 wwwroot/                    # Static dosyalar
    │   ├── 📁 css/                    # CSS dosyaları
    │   ├── 📁 js/                     # JavaScript dosyaları
    │   ├── 📁 img/                    # Resim dosyaları
    │   └── 📁 Prodora-İmage/          # Proje ekran görüntüleri
    ├── 📁 Identity/                   # ASP.NET Identity
    ├── 📁 Extensions/                 # Extension method'lar
    ├── 📁 Middlewares/               # Custom middleware'ler
    └── 📄 Program.cs                  # Uygulama başlangıç noktası
```

</div>

---

## 🎯 Design Patterns & Architectures

<div align="center">

| Pattern/Architecture | Kullanım Alanı | Açıklama |
|---------------------|----------------|-----------|
| 🏗️ **N-Tier Architecture** | Genel mimari | Katmanlı mimari yapısı |
| 📦 **Repository Pattern** | Data Access | Veri erişim soyutlaması |
| 🔧 **Dependency Injection** | IoC Container | Bağımlılık enjeksiyonu |
| 🎭 **MVC Pattern** | Web Layer | Model-View-Controller |
| 🏭 **Manager Pattern** | Business Layer | İş mantığı yönetimi |
| 🔍 **Generic Repository** | Data Access | Tip güvenli veri işlemleri |
| 🛡️ **Unit of Work** | Data Access | Transaction yönetimi |

</div>

---

## 🗄️ Veritabanı Şeması

<div align="center">

```mermaid
erDiagram
    Product ||--o{ ProductCategory : has
    Category ||--o{ ProductCategory : belongs
    Product ||--o{ Image : contains
    Product ||--o{ Comment : receives
    User ||--o{ Comment : makes
    User ||--o{ Order : places
    Order ||--o{ OrderItem : contains
    Product ||--o{ OrderItem : includes
    User ||--o{ Basket : owns
    Basket ||--o{ Product : contains

    Product {
        int Id PK
        string Name
        string Description
        decimal Price
        bool Stock
        string Brand
        datetime CreatedDate
    }
    
    Category {
        int Id PK
        string Name
        string Description
        string Url
    }
    
    ProductCategory {
        int ProductId FK
        int CategoryId FK
    }
    
    Image {
        int Id PK
        string ImageUrl
        int ProductId FK
    }
    
    Comment {
        int Id PK
        string Text
        int Rating
        datetime Date
        string UserId FK
        int ProductId FK
    }
    
    User {
        string Id PK
        string UserName
        string Email
        string FirstName
        string LastName
        datetime DateOfBirth
    }
    
    Order {
        int Id PK
        string OrderNumber
        datetime OrderDate
        decimal TotalAmount
        string OrderState
        string UserId FK
        string PaymentId
    }
    
    OrderItem {
        int Id PK
        int Quantity
        decimal Price
        int OrderId FK
        int ProductId FK
    }
    
    Basket {
        int Id PK
        string UserId FK
        int ProductId FK
        int Quantity
        datetime DateCreated
    }
```

</div>

---

## � Kurulum ve Çalıştırma

### 📋 Gereksinimler

- ![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat&logo=.net) **.NET 6.0 SDK** veya üzeri
- ![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server) **SQL Server 2019+** veya **SQL Server Express**
- ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2022-5C2D91?style=flat&logo=visual-studio) **Visual Studio 2022** (önerilen)

### 🚀 Adım Adım Kurulum

#### 1️⃣ **Projeyi Klonlayın**
```bash
git clone https://github.com/egegeegege/Prodora.git
cd Prodora
```

#### 2️⃣ **Veritabanı Bağlantı Stringlerini Ayarlayın**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ProdoraDb;Trusted_Connection=true;TrustServerCertificate=true",
    "IdentityConnection": "Server=.;Database=ProdoraIdentityDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

#### 3️⃣ **NuGet Paketlerini Yükleyin**
```bash
dotnet restore
```

#### 4️⃣ **Veritabanını Oluşturun**
```bash
# Ana veritabanı migration'ı
dotnet ef database update --project Prodora.DataAccess --startup-project Prodora.WebUI

# Identity veritabanı migration'ı  
dotnet ef database update --context ApplicationIdentityDbContext --project Prodora.WebUI
```

#### 5️⃣ **Projeyi Çalıştırın**
```bash
dotnet run --project Prodora.WebUI
```

#### 6️⃣ **Tarayıcıda Açın**
```
https://localhost:5001
```

---

## 🔐 Güvenlik Özellikleri

<div align="center">

| Güvenlik Özelliği | Durum | Açıklama |
|-------------------|-------|----------|
| 🔒 **HTTPS Enforcement** | ✅ | Tüm iletişim şifreli |
| 🛡️ **CSRF Protection** | ✅ | Cross-site request forgery koruması |
| 🔑 **JWT Token** | ✅ | Güvenli authentication token'ları |
| 📧 **Email Verification** | ✅ | E-posta doğrulaması zorunlu |
| 🔐 **Password Policy** | ✅ | Güçlü şifre kuralları |
| 🚫 **XSS Protection** | ✅ | Cross-site scripting koruması |
| 🔒 **SQL Injection Protection** | ✅ | Parametrized query'ler |
| 👥 **Role-Based Access** | ✅ | Rol tabanlı yetki kontrolü |

</div>

---

## 📊 Performans Özellikleri

<div align="center">

| Özellik | Değer | Açıklama |
|---------|-------|----------|
| ⚡ **Sayfa Yükleme Süresi** | `< 2s` | Optimize edilmiş kaynak yükleme |
| 🗄️ **Veritabanı Sorgu Optimizasyonu** | ✅ | LINQ & EF Core optimizasyonları |
| 📱 **Responsive Design** | ✅ | Tüm cihazlarda uyumlu |
| 🎨 **CSS/JS Minification** | ✅ | Sıkıştırılmış static dosyalar |
| 🖼️ **Image Optimization** | ✅ | Optimize edilmiş resim boyutları |
| 💾 **Caching Strategy** | ✅ | Memory ve output caching |

</div>

---

## 🧪 Test Stratejisi

<div align="center">

| Test Türü | Durum | Araç/Framework |
|-----------|-------|----------------|
| 🧩 **Unit Tests** | 🔄 | xUnit, Moq |
| 🔗 **Integration Tests** | 🔄 | ASP.NET Core Test Host |
| 🌐 **End-to-End Tests** | 📋 | Selenium WebDriver |
| 📊 **Performance Tests** | 📋 | NBomber, BenchmarkDotNet |
| 🔒 **Security Tests** | 📋 | OWASP ZAP |

</div>

**Durum Açıklaması:**
- ✅ Tamamlandı
- 🔄 Devam ediyor
- 📋 Planlandı

---

## 📈 Gelecek Planları

<div align="center">

### 🚀 Yakın Gelecek (Q1 2025)

</div>

| Özellik | Öncelik | Açıklama |
|---------|---------|----------|
| 📱 **Mobile App** | 🔥 **Yüksek** | React Native ile mobil uygulama |
| 🌐 **Multi-Language** | 🔥 **Yüksek** | Çoklu dil desteği |
| 📊 **Advanced Analytics** | 📊 **Orta** | Gelişmiş satış analitiği |
| 🔔 **Push Notifications** | 📊 **Orta** | Gerçek zamanlı bildirimler |
| 🎁 **Loyalty Program** | 📊 **Orta** | Müşteri sadakat programı |

<div align="center">

### 🔮 Uzun Vadeli (2025-2026)

</div>

| Özellik | Açıklama |
|---------|----------|
| 🤖 **AI Recommendations** | Yapay zeka destekli ürün önerileri |
| 🛒 **Multi-Vendor Support** | Çok satıcılı marketplace |
| 📦 **Advanced Inventory** | Gelişmiş stok yönetimi |
| 💬 **Live Chat Support** | Canlı müşteri desteği |
| 🌍 **Global Shipping** | Uluslararası kargo entegrasyonu |

---

## 🤝 Katkıda Bulunma

Projeye katkıda bulunmak istiyorsanız:

1. 🍴 **Fork** edin
2. 🌿 **Feature branch** oluşturun (`git checkout -b feature/AmazingFeature`)
3. 💾 **Commit** edin (`git commit -m 'Add some AmazingFeature'`)
4. 📤 **Push** edin (`git push origin feature/AmazingFeature`)
5. 🔄 **Pull Request** açın

---

## 📄 Lisans

Bu proje **MIT Lisansı** ile lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakınız.

---

## 📞 İletişim & Destek

<div align="center">

[![GitHub](https://img.shields.io/badge/GitHub-egegeegege-181717?style=for-the-badge&logo=github)](https://github.com/egegeegege)
[![Email](https://img.shields.io/badge/Email-prodora%40example.com-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:prodora@example.com)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0077B5?style=for-the-badge&logo=linkedin)](https://linkedin.com/in/yourprofile)

**💬 Discord:** [Prodora Community Server](https://discord.gg/prodora)  
**📧 Email:** prodora@example.com  
**🐛 Issues:** [GitHub Issues](https://github.com/egegeegege/Prodora/issues)

</div>

---

<div align="center">

### 🌟 Bu projeyi beğendiyseniz star vermeyi unutmayın!

**Made with ❤️ by [Ege Developer](https://github.com/egegeegege)**

![Visitor Count](https://visitor-badge.laobi.icu/badge?page_id=egegeegege.Prodora)
![Last Updated](https://img.shields.io/github/last-commit/egegeegege/Prodora?style=flat&label=Last%20Updated)

---

*© 2025 Prodora. Tüm hakları saklıdır.*

</div>

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

> Modern ve şık arayüz, kolay yönetim paneli ve kullanıcı dostu alışveriş deneyimini Deneyinleyin!

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
