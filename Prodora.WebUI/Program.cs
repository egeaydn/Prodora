using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.DataAccess.Concrate.EfCore;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables().AddCommandLine(args);
builder.Services.Configure<Prodora.WebUI.EmailServices.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<Prodora.WebUI.EmailServices.IAccountEmailSender, Prodora.WebUI.EmailServices.SmtpEmailSender>();
builder.Services.AddScoped<Prodora.WebUI.EmailServices.AccountLinkBuilder>();
if (!builder.Environment.IsDevelopment() &&
    (!Prodora.WebUI.EmailServices.AccountLinkBuilder.TryParseOrigin(builder.Configuration["Site:PublicBaseUrl"], out var publicOrigin)
        || publicOrigin.Scheme != Uri.UriSchemeHttps))
    throw new InvalidOperationException("Site:PublicBaseUrl must be configured with an HTTPS origin outside Development.");
builder.Services.AddRazorPages();

// Commerce and Identity use the existing database unless explicitly configured separately.
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CommerceConnection")
        ?? builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
				.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequiredLength = 6;

	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.AllowedForNewUsers = true;

	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedEmail = true;
	options.SignIn.RequireConfirmedPhoneNumber = false;
});

// Cookie Options
builder.Services.ConfigureApplicationCookie(ApplicationCookieSettings.Configure);

// The development profile serves plain HTTP, so cookies are Secure-only outside Development.
var cookieSecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
builder.Services.ConfigureApplicationCookie(options => options.Cookie.SecurePolicy = cookieSecurePolicy);
builder.Services.AddAntiforgery(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.SecurePolicy = cookieSecurePolicy;
});



// Business and DataAccess
builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
builder.Services.AddScoped<IProductServices, ProductManager>();
builder.Services.AddScoped<Prodora.WebUI.Images.ProductImageStorage>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
builder.Services.AddScoped<ICategoryServices>(services => new CachedCategoryServices(
    new CategoryManager(services.GetRequiredService<ICategoryDal>()), services.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));
builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
builder.Services.AddScoped<ICommentServices, CommentManager>();
builder.Services.AddScoped<IBasketDal, EfCoreBasketDal>();
builder.Services.AddScoped<IBasketServices, BasketManager>();
builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
builder.Services.AddScoped<IOrderServices, OrderManager>();
builder.Services.AddScoped<ICheckoutDal, EfCoreCheckoutDal>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IWishlistDal, EfCoreWishlistDal>();
builder.Services.AddScoped<IWishlistServices, WishlistManager>();
builder.Services.AddScoped<Prodora.WebUI.Wishlist.WishlistState>();
builder.Services.AddScoped<IAddressDal, EfCoreAddressDal>();
builder.Services.AddScoped<IAddressServices, AddressManager>();
builder.Services.AddScoped<IAuditDal, EfCoreAuditDal>();
builder.Services.AddScoped<Prodora.WebUI.Audit.IAuditLog, Prodora.WebUI.Audit.AuditLog>();
builder.Services.AddScoped<IVariantDal, EfCoreVariantDal>();
builder.Services.AddScoped<IVariantServices, VariantManager>();
builder.Services.AddScoped<ICouponDal, EfCoreCouponDal>();
builder.Services.AddScoped<ICouponServices, CouponManager>();
builder.Services.Configure<Prodora.WebUI.Payments.IyzicoSettings>(builder.Configuration.GetSection("Iyzico"));
builder.Services.AddScoped<Prodora.WebUI.Payments.IPaymentGateway, Prodora.WebUI.Payments.IyzicoPaymentGateway>();
builder.Services.AddScoped<Prodora.WebUI.Payments.CheckoutService>();
builder.Services.AddScoped<Prodora.WebUI.Orders.OrderCancellationService>();
builder.Services.AddScoped<Prodora.WebUI.Orders.OrderNotificationService>();

builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHealthChecks().AddCheck<Prodora.WebUI.Health.DatabaseHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
        await context.HttpContext.Response.WriteAsync("Çok fazla istek gönderdin. Birkaç dakika sonra tekrar dene.", cancellationToken);
    options.AddPolicy("account-email", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        { PermitLimit = 5, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
    options.AddPolicy("account-login", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
    options.AddPolicy("account-reset", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
});

var app = builder.Build();

// Opt-in (the Docker compose file turns it on) so a fresh database is created without running dotnet ef by hand.
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
	for (var attempt = 1; ; attempt++)
	{
		try
		{
			using var migrationScope = app.Services.CreateScope();
			await migrationScope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext().Database.MigrateAsync();
			await migrationScope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().Database.MigrateAsync();
			break;
		}
		catch (Exception exception) when (attempt < 10)
		{
			app.Logger.LogWarning("Database is not ready yet (attempt {Attempt}); error type {ErrorType}", attempt, exception.GetType().Name);
			await Task.Delay(TimeSpan.FromSeconds(3));
		}
	}
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseSecurityHeaders();

// Kullanıcı ve rol oluşturmak istiyorsan
using (var scope = app.Services.CreateScope())
{
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

	// örnek: admin rolü varsa yoksa oluştur
	if (!await roleManager.RoleExistsAsync("admin"))
	{
		await roleManager.CreateAsync(new IdentityRole("admin"));
	}
}

app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// /health: the process is up. /health/ready: it can also reach its databases. Neither returns details.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapControllerRoute("adminProducts", "admin/products", new { controller = "Admin", action = "ProductList" });
app.MapControllerRoute("adminEditProduct", "admin/products/{id:int}", new { controller = "Admin", action = "EditProduct" });
app.MapControllerRoute("adminCategories", "admin/category", new { controller = "Admin", action = "CategoryList" });
app.MapControllerRoute("adminEditCategory", "admin/categories/{id:int}", new { controller = "Admin", action = "EditCategory" });
app.MapControllerRoute("shopDetails", "shop/details/{id:int}/{slug?}", new { controller = "Shop", action = "Details" });
app.MapControllerRoute("checkout", "checkout", new { controller = "Basket", action = "Checkout" });
app.MapControllerRoute("orders", "orders", new { controller = "Basket", action = "GetOrders" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
