# 🛍️ PRODORA - Modern E-Ticaret Platformu

<div align="center">

![Prodora Logo](https://img.shields.io/badge/🛍️-PRODORA-FF6000?style=for-the-badge&labelColor=000000)

**🚀 Next Generation E-Commerce Platform with Advanced Features**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://docs.microsoft.com/en-us/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

![GitHub Stars](https://img.shields.io/github/stars/egeaydn/Prodora?style=social)
![GitHub Forks](https://img.shields.io/github/forks/egeaydn/Prodora?style=social)
![GitHub Issues](https://img.shields.io/github/issues/egeaydn/Prodora?style=social)

</div>

---

## 📸 Ekran Görüntüleri

<div align="center">

### 🛍️ Mağaza & Ürün Listesi
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/homePage.png" alt="Ürün Listesi" width="90%" style="border-radius: 10px; margin: 5px;">

### 👤 Hesabım & ❓ Yardım Merkezi
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/myAccountPage.png" alt="Hesabım" width="45%" style="border-radius: 10px; margin: 5px;">
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/howCanIHelpYouPage.png" alt="Yardım Merkezi" width="45%" style="border-radius: 10px; margin: 5px;">

### 📦 Siparişlerim
<img src="Prodora.WebUI/wwwroot/Prodora-İmage/orders.png" alt="Siparişlerim" width="90%" style="border-radius: 10px; margin: 5px;">

</div>

---

## 🎯 Proje Özeti

**Prodora**, modern e-ticaret ihtiyaçlarına yönelik geliştirilmiş, tam özellikli bir online alışveriş platformudur. Katmanlı mimari (N-Tier Architecture) kullanılarak geliştirilmiş, güvenli ve ölçeklenebilir bir yapıya sahiptir.

### 👤 Kullanıcı Rolleri

- **Admin:** Ürün ve kategori yönetimi, kullanıcı yönetimi, sistem ayarları.
- **Kullanıcı:** Ürünleri görüntüleme, sepete ekleme, sipariş verme, profil yönetimi.

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
| ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=.net) | **8.0** | 🏗️ **Core Framework** | Ana uygulama framework'ü |
| ![C#](https://img.shields.io/badge/C%23-11.0-239120?style=flat&logo=c-sharp) | **11.0** | 💻 **Programming Language** | Ana programlama dili |
| ![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=flat&logo=.net) | **8.0** | 🌐 **Web Framework** | MVC pattern, Web API |
| ![Entity Framework](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat&logo=.net) | **8.0** | 🗄️ **ORM** | Code-First, Migration, LINQ |
| ![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server) | **2019+** | 💾 **Database** | İlişkisel veritabanı |
| ![ASP.NET Identity](https://img.shields.io/badge/Identity-Core-512BD4?style=flat&logo=.net) | **8.0** | 🔐 **Authentication** | Kullanıcı yönetimi ve güvenlik |

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
| ❌ **Sipariş İptali & İade** | ✅ | Hazırlanmadan önce iptal; kartla ödenenler iyzico üzerinden iade edilir |
| 🎟️ **Kupon Kodu** | ✅ | Ödemede yüzde veya sabit tutar indirimi, kişi başı tek kullanım |

<div align="center">

### 👨‍💼 Admin Özellikleri

</div>

| Özellik | Durum | Açıklama |
|---------|-------|----------|
| 📊 **Dashboard** | ✅ | Satış istatistikleri ve genel bakış |
| 🏷️ **Ürün Yönetimi** | ✅ | CRUD işlemleri, resim yükleme |
| 📂 **Kategori Yönetimi** | ✅ | Kategori ekleme, düzenleme, silme |
| 👥 **Kullanıcı Yönetimi** | ✅ | Rol bazlı yetki kontrolü |
| 📋 **Sipariş Takibi** | ✅ | Durum ilerletme, kargo takip numarası, yönetici iptali ve iadesi |
| 🎟️ **Kupon Yönetimi** | ✅ | Kod, indirim türü, alt sepet tutarı, son gün ve kullanım sınırı |
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
| 🛡️ **Transaction Yönetimi** | Data Access | `CommerceTransaction` ile sepet ve ödeme yazmalarında kilitli transaction |

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

- ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=.net) **.NET 8.0 SDK** veya üzeri
- ![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server) **SQL Server 2019+** veya **SQL Server Express**
- ![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2022-5C2D91?style=flat&logo=visual-studio) **Visual Studio 2022** (önerilen)

### 🚀 Adım Adım Kurulum

#### 1️⃣ **Projeyi Klonlayın**
```bash
git clone https://github.com/egeaydn/Prodora.git
cd Prodora
```

#### 2️⃣ **Veritabanı ve yerel ayarlar**

Mevcut MSSQL veritabanını kullanmak için `Prodora.WebUI/appsettings.json` içindeki `ConnectionStrings:IdentityConnection` değerini kendi SQL Server adresine göre düzenle. Ticaret tabloları varsayılan olarak aynı veritabanını kullanır; ayrı veritabanı gerekiyorsa `CommerceConnection` tanımla.

SMTP ve iyzico sandbox anahtarlarını `Prodora.WebUI/appsettings.Local.json` içinde tut. Yeni kurulumda `appsettings.Local.example.json` dosyasını bu adla kopyala; mevcut yerel dosyanın üzerine yazma. Bu dosya Git ve publish çıktısına dahil edilmez, yalnızca Development ortamında okunur.

Yayında hesap e-postalarının doğru adrese gitmesi için `Site__PublicBaseUrl` ortam değişkenini HTTPS site kökünle ayarla; `AllowedHosts` değerini gerçek alan adınla sınırla. Geliştirmede yerel loopback adresi otomatik kullanılır.

#### 3️⃣ **NuGet paketleri**

```powershell
dotnet restore
```

#### 4️⃣ **Veritabanı**

Mevcut veritabanını önce yedekle. 26 Eylül güvenilirlik migration'ı için yedekli güncelleme ve durum kontrolü [düzeltme notlarında](docs/GUVENILIRLIK_DUZELTMELERI.md) anlatılıyor. Mevcut bir veritabanında yeni migration'lar (örneğin sipariş takip numarası) için yedek aldıktan sonra aynı iki `dotnet ef database update` komutunu çalıştır. Sıfırdan kurulumda:

```powershell
dotnet ef database update --context DataContext --project Prodora.DataAccess --startup-project Prodora.WebUI
dotnet ef database update --context ApplicationIdentityDbContext --project Prodora.WebUI
```

#### 5️⃣ **Çalıştırma**

```powershell
dotnet run --project Prodora.WebUI --launch-profile http
```

Tarayıcı adresi: **http://localhost:5047**. Ödeme entegrasyonu **iyzico sandbox** ortamındadır; test kartları kullanılır.

#### 🐳 **Docker ile tek komut (yerel demo)**

```powershell
docker compose up --build
```

Uygulama + SQL Server ayağa kalkar ve migration'lar otomatik uygulanır: **http://localhost:8080**. Bu yalnızca yerel demo içindir (Development ortamı, düz HTTP). SQL şifresini değiştirmek için `MSSQL_SA_PASSWORD` ortam değişkenini ayarla. Sağlık kontrolleri: `/health` (süreç ayakta) ve `/health/ready` (veritabanlarına bağlanabiliyor); ikisi de ayrıntı döndürmez. Günlükler Serilog ile konsola yazılır. Yönetici hesabı için kayıt olduktan sonra kullanıcıyı `admin` rolüne ekle.

#### ☁️ **Azure'a yayın**

`Deploy to Azure` workflow'u (GitHub → Actions) elle tetiklenir: derler, birim testlerini çalıştırır, yayınlar ve `/health/ready` ile doğrular. Çalışması için `AZURE_WEBAPP_NAME` değişkeni ve `AZURE_WEBAPP_PUBLISH_PROFILE` gizli anahtarı tanımlanmalı; ayrıntılar workflow dosyasının başındadır.

#### 6️⃣ **Kontroller**

```powershell
dotnet run --project tools/Prodora.StoreChecks
dotnet run --project tools/Prodora.EmailPreview
dotnet test tests/Prodora.Tests
```

StoreChecks yalnızca kendi oluşturduğu geçici SQL Server veritabanlarında çalışır; gerçek e-posta veya ödeme göndermez. E-posta tasarımları `artifacts/email-previews/index.html` üzerinden incelenebilir.

---

## 🔐 Güvenlik ve sınırlar

Uygulama ASP.NET Core Identity oturum çerezi, e-posta doğrulaması, parola kuralları, rol kontrolü ve form CSRF koruması kullanır. JWT kimlik doğrulaması yoktur. HTTP geliştirme profili vardır; gerçek bir dağıtımda HTTPS ve güvenli çerez ayarları ayrıca doğrulanmalıdır.

İyzico entegrasyonu yalnızca sandbox adresine gider. Gerçek ödeme için ayrı bir operasyon ve güvenlik hazırlığı gerekir. Yerel SMTP ve sandbox anahtarları Git dışındaki `appsettings.Local.json` dosyasında tutulur. Daha önce Git'e girmiş anahtarların sağlayıcı tarafında yenilenmesi gerekir. Eski `Prodora.bak` dosyası da yeni commit'lerden çıkarılmıştır; eski commitlerde bulunmaya devam eder.

## 🧪 Kontroller

- `tools/Prodora.PasswordResetChecks`: gerçek MVC ve Identity şifre yenileme, antiforgery ve oturum senaryoları; SQL/SMTP kullanmaz.
- `tools/Prodora.EmailPreview`: dört e-posta şablonu, güvenli HTML kodlama ve bağlantı kontrolleri; e-posta göndermez.
- `tests/Prodora.Tests`: xUnit birim testleri; sipariş durum geçişleri, ödeme akışı (`CheckoutService`) ve Product/Basket/Order manager kuralları. SQL gerektirmez: `dotnet test tests/Prodora.Tests`.
- `tools/Prodora.StoreChecks`: geçici SQL Server veritabanlarında migration, sepet, sipariş, ödeme ve hesap regresyonları; gerçek iyzico çağrısı yapmaz.

GitHub Actions çözümü derler, birim testlerini ve ilk iki kontrolü çalıştırır; ayrı bir job'da SQL Server 2022 servis container'ı üzerinde StoreChecks'i de çalıştırır. Gerçek posta teslimatı, iyzico sandbox uçtan uca ödeme ve tarayıcı görsel kontrolleri ayrıca doğrulanmalıdır.

---

## 📈 Gelecek Planları

<div align="center">

### 🚀 Yakın Gelecek

</div>

| Özellik | Öncelik | Açıklama |
|---------|---------|----------|
| 📱 **Mobile App** | 🔥 **Yüksek** | React Native ile mobil uygulama |
| 🌐 **Multi-Language** | 🔥 **Yüksek** | Çoklu dil desteği |
| 📊 **Advanced Analytics** | 📊 **Orta** | Gelişmiş satış analitiği |
| 🔔 **Push Notifications** | 📊 **Orta** | Gerçek zamanlı bildirimler |
| 🎁 **Loyalty Program** | 📊 **Orta** | Müşteri sadakat programı |

<div align="center">

### 🔮 Uzun Vadeli

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

[![GitHub](https://img.shields.io/badge/GitHub-egeaydn-181717?style=for-the-badge&logo=github)](https://github.com/egeaydn)
[![Email](https://img.shields.io/badge/Email-egeaydinn31%40gmail.com-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:egeaydinn31@gmail.com)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Ege%20Ayd%C4%B1n-0077B5?style=for-the-badge&logo=linkedin)](https://www.linkedin.com/in/ege-ayd%C4%B1n-156704317/)

**📧 Email:** egeaydinn31@gmail.com  
**🐛 Issues:** [GitHub Issues](https://github.com/egeaydn/Prodora/issues)

</div>

---

<div align="center">

### 🌟 Bu projeyi beğendiyseniz star vermeyi unutmayın!

**Made with ❤️ by [Ege Aydın](https://github.com/egeaydn)**

![Visitor Count](https://visitor-badge.laobi.icu/badge?page_id=egeaydn.Prodora)
![Last Updated](https://img.shields.io/github/last-commit/egeaydn/Prodora?style=flat&label=Last%20Updated)

---

*© 2025 Prodora. Tüm hakları saklıdır.*

</div>