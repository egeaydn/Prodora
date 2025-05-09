﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.DataAccess.Concrate.EfCore;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
				.AddDefaultTokenProviders();

var userManager = builder.Services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
var roleManager = builder.Services.BuildServiceProvider().GetService<RoleManager<IdentityRole>>();

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

builder.Services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}


app.UseStaticFiles();
app.CustomStaticFiles(); // node_modules => modules 
app.UseHttpsRedirection();
app.UseAuthentication(); // kimlik doðrulama
app.UseAuthorization(); // yetkilendirme
app.UseRouting();


app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");

	endpoints.MapControllerRoute(
		name: "products",
		pattern: "products/{category}",
		defaults: new { controller = "Shop", action = "List" }
	);
	endpoints.MapControllerRoute(
		name: "adminProducts",
		pattern: "admin/products",
		defaults: new { controller = "Admin", action = "ProductList" }
	);
	endpoints.MapControllerRoute(
		name: "adminProducts",
		pattern: "admin/products/{id}",
		defaults: new { controller = "Admin", action = "EditProduct" }
	);
	endpoints.MapControllerRoute(
		 name: "adminProducts",
		 pattern: "admin/category",
		 defaults: new { controller = "Admin", action = "CategoryList" }
	);
	endpoints.MapControllerRoute(
		name: "adminProducts",
		pattern: "admin/categories/{id}",
		defaults: new { controller = "Admin", action = "EditCategory" }
	);
}
);


app.Run();
