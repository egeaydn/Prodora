using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.DataAccess.Concrate.EfCore;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Middlewares;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/account/login";
	options.LogoutPath = "/account/logout";
	options.AccessDeniedPath = "/account/accessdenied";
	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
	options.Cookie = new CookieBuilder
	{
		HttpOnly = true,
		Name = "PRODORA.Security.Cookie",
		SameSite = SameSiteMode.Strict
	};
});



// Business and DataAccess
builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
builder.Services.AddScoped<IProductServices, ProductManager>();
builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
builder.Services.AddScoped<ICategoryServices, CategoryManager>();
builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
builder.Services.AddScoped<ICommentServices, CommentManager>();
builder.Services.AddScoped<IBasketDal, EfCoreBasketDal>();
builder.Services.AddScoped<IBasketServices, BasketManager>();
builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
builder.Services.AddScoped<IOrderServices, OrderManager>();

builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}

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
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("adminProducts", "admin/products", new { controller = "Admin", action = "ProductList" });
app.MapControllerRoute("adminEditProduct", "admin/products/{id:int}", new { controller = "Admin", action = "EditProduct" });
app.MapControllerRoute("adminCategories", "admin/category", new { controller = "Admin", action = "CategoryList" });
app.MapControllerRoute("adminEditCategory", "admin/categories/{id:int}", new { controller = "Admin", action = "EditCategory" });
app.MapControllerRoute("shopDetails", "shop/details/{id:int}", new { controller = "Shop", action = "Details" });
app.MapControllerRoute("checkout", "checkout", new { controller = "Basket", action = "Checkout" });
app.MapControllerRoute("orders", "orders", new { controller = "Basket", action = "GetOrders" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
