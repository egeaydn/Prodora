using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;
using Prodora.Business.Abstract;
using Prodora.Business.Concrate;
using Prodora.DataAccess.Abstract;
using Prodora.DataAccess.Concrate.EfCore;
using Prodora.Entitys;
using Prodora.WebUI.Controllers;
using Prodora.WebUI.EmailServices;
using Prodora.WebUI.Identity;
using Prodora.WebUI.Models;
using Prodora.WebUI.Payments;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var webRoot = Path.Combine(root, "Prodora.WebUI");
var config = new ConfigurationBuilder().SetBasePath(webRoot).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables().Build();
var configuredConnection = config.GetConnectionString("CommerceConnection") ?? config.GetConnectionString("IdentityConnection")!;
if (args.Contains("--inspect") || args.Contains("--migrate"))
{
    await DatabaseMaintenance.Run(configuredConnection, args.Contains("--migrate"));
    return;
}
var databaseName = "Prodora_Checks_" + Guid.NewGuid().ToString("N");
var connectionBuilder = new SqlConnectionStringBuilder(configuredConnection) { InitialCatalog = databaseName };
var commerceConnection = connectionBuilder.ConnectionString;
connectionBuilder.InitialCatalog = databaseName + "_Identity";
var identityConnection = connectionBuilder.ConnectionString;
var factory = new TestContextFactory(commerceConnection);
var identityOptions = new DbContextOptionsBuilder<ApplicationIdentityDbContext>().UseSqlServer(identityConnection).Options;
await using var data = factory.CreateDbContext();
await using var identity = new ApplicationIdentityDbContext(identityOptions);
WebApplication? app = null;
var checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    Console.WriteLine("PASS: " + label);
    checks++;
}
void Rejected(Action action, string label)
{
    try { action(); } catch (InvalidOperationException) { Check(true, label); return; }
    throw new Exception("FAIL: expected rejection: " + label);
}
OrderModels Form(string? key = null) => new()
{
    RequestId = key ?? Guid.NewGuid().ToString("N"), Firstname = "Test", Lastname = "User", Address = "Test address",
    City = "Istanbul", Phone = "5551234567", Email = "synthetic@example.test",
    CardName = "Test User", CardNumber = "test-only", CVV = "000", ExpirationMonth = "12", ExpirationYear = "2030"
};
try
{
    // Exercise the real migration on legacy schema/data, not EnsureCreated's latest schema.
    await data.GetService<IMigrator>().MigrateAsync("20250614165332_GaratieMigrate");
    await data.Database.ExecuteSqlRawAsync("""
        INSERT INTO ProdoraProducts (Name,Description,Price,Stock,Brand) VALUES ('Legacy product','Test',12.50,1,'Test');
        DECLARE @product int = SCOPE_IDENTITY();
        INSERT INTO Orders (UserId,OrderNumber,OrderDate,FirstName,LastName,Adress,City,Phone,Email,OrderNote,PaymentId,PaymentToken,ConversionId,OrderEnums,PaymentEnum)
        VALUES ('legacy','legacy',GETDATE(),'Test','Test','Test','Test','000','test@example.test','','','','',5,10);
        DECLARE @order int = SCOPE_IDENTITY();
        INSERT INTO OrderItem (OrderId,ProductId,Price,Quantity) VALUES (@order,@product,12.50,2);
        """);
    await data.Database.MigrateAsync();
    var legacy = await data.Orders.Include(o => o.OrderItems).SingleAsync();
    Check(legacy.OrderItems.Single().ProductName == "Legacy product" && legacy.OrderItems.Single().Price == 12.50m, "migration preserves legacy order and fills product snapshot");
    await identity.Database.EnsureCreatedAsync();
    var baskets = new EfCoreBasketDal(factory);
    var checkoutDal = new EfCoreCheckoutDal(factory);
    var products = new EfCoreProductDal(factory);
    var product = new Product { Name = "Original product", Description = "Test", Brand = "Test", Stock = true, Price = 19.95m };
    products.Create(product);
    await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Task.Run(() => baskets.EnsureBasket("basket-test"))));
    Check(await data.Baskets.CountAsync(b => b.UserId == "basket-test") == 1, "repeated/concurrent confirmation creates one basket");
    await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Task.Run(() => baskets.AddItem("basket-test", product.Id, 1))));
    Check(baskets.CartByUserId("basket-test").BasketItems.Single().Quantity == 5, "concurrent additions preserve every increment");
    baskets.AddItem("basket-test", product.Id, 94);
    Rejected(() => baskets.AddItem("basket-test", product.Id, 1), "total per-product quantity cannot exceed 99");
    Rejected(() => baskets.AddItem("basket-test", product.Id, 0), "zero quantity rejected");
    Rejected(() => baskets.AddItem("basket-test", product.Id, int.MaxValue), "overflow quantity rejected");
    using (var context = factory.CreateDbContext()) { var p = context.Products.Find(product.Id)!; p.Stock = false; context.SaveChanges(); }
    Rejected(() => baskets.AddItem("basket-test", product.Id, 1), "out-of-stock add rejected");
    var gateway = new FakeGateway();
    var checkout = new CheckoutService(checkoutDal, gateway, NullLogger<CheckoutService>.Instance);
    try { await checkout.SubmitAsync(Form(), "basket-test", "credit", "127.0.0.1"); throw new Exception("unavailable checkout accepted"); }
    catch (InvalidOperationException) { Check(gateway.Calls == 0, "checkout rechecks stock before payment"); }
    using (var context = factory.CreateDbContext()) { var p = context.Products.Find(product.Id)!; p.Stock = true; context.SaveChanges(); }
    var sameForm = Form();
    var submissions = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Task.Run(() => checkout.SubmitAsync(sameForm, "basket-test", "credit", "127.0.0.1"))));
    Check(submissions.Select(o => o.Id).Distinct().Count() == 1 && gateway.Calls == 1, "concurrent duplicate checkout charges once and creates one order");
    var saved = new EfCoreOrderDal(factory).GetOrders("basket-test").Single();
    Check(saved.PaymentId == "synthetic-payment" && saved.OrderEnums == OrderStatus.Completed, "provider payment reference saved");
    Check(baskets.CartByUserId("basket-test").BasketItems.Count == 0, "successful order and basket clear committed");
    Check(new EfCoreOrderDal(factory).GetOrders("").Count == 0, "missing user ID never returns other orders");
    products.Delete(product);
    Check(!products.GetAll().Any(p => p.Id == product.Id) && products.GetAllIncludingArchived().Any(p => p.Id == product.Id), "archived product hidden from store, retained in admin");
    Check(new EfCoreOrderDal(factory).GetOrders("basket-test").Single().OrderItems.Single().ProductName == "Original product", "archiving preserves order details");
    Rejected(() => baskets.AddItem("archived-test", product.Id, 1), "archived product add rejected");
    using (var context = factory.CreateDbContext())
    {
        context.Products.Remove(context.Products.Find(product.Id)!);
        try { context.SaveChanges(); throw new Exception("physical deletion accepted"); }
        catch (DbUpdateException) { Check(true, "database restricts deletion of purchased product"); }
    }
    products.Restore(product.Id);
    using (var context = factory.CreateDbContext()) { var p = context.Products.Find(product.Id)!; p.Name = "Changed name"; p.Price = 88m; context.SaveChanges(); }
    saved = new EfCoreOrderDal(factory).GetOrders("basket-test").Single();
    Check(saved.OrderItems.Single().ProductName == "Original product" && saved.OrderItems.Single().Price == 19.95m, "catalog edits leave historic name and price unchanged");
    baskets.AddItem("eft-test", product.Id, 1);
    var eft = await checkout.SubmitAsync(Form(), "eft-test", "eft", "127.0.0.1");
    Check(eft.OrderEnums == OrderStatus.Pending && eft.PaymentId == "" && gateway.Calls == 1, "EFT stays unpaid and never calls payment provider");
    Check(baskets.CartByUserId("eft-test").BasketItems.Count == 0, "EFT order and basket clear committed together");
    var failedGateway = new FakeGateway { Outcome = OrderStatus.PaymentFailed };
    var failedService = new CheckoutService(checkoutDal, failedGateway, NullLogger<CheckoutService>.Instance);
    baskets.AddItem("failed-test", product.Id, 1);
    var failedForm = Form();
    await failedService.SubmitAsync(failedForm, "failed-test", "credit", "127.0.0.1");
    await failedService.SubmitAsync(failedForm, "failed-test", "credit", "127.0.0.1");
    Check(failedGateway.Calls == 1 && baskets.CartByUserId("failed-test").BasketItems.Count == 1, "failed payment keeps basket and same request cannot charge again");
    var unknownGateway = new FakeGateway { Throw = true };
    var unknownService = new CheckoutService(checkoutDal, unknownGateway, NullLogger<CheckoutService>.Instance);
    baskets.AddItem("unknown-test", product.Id, 1);
    var unknown = await unknownService.SubmitAsync(Form(), "unknown-test", "credit", "127.0.0.1");
    Check(unknown.OrderEnums == OrderStatus.PaymentUncertain, "transport failure stored as uncertain instead of safe-to-retry");
    Rejected(() => baskets.AddItem("unknown-test", product.Id, 1), "uncertain payment freezes basket");
    try { await unknownService.SubmitAsync(Form(), "unknown-test", "credit", "127.0.0.1"); throw new Exception("second uncertain payment accepted"); }
    catch (InvalidOperationException) { Check(unknownGateway.Calls == 1, "new request cannot charge while payment is uncertain"); }

    Iyzipay.Model.Payment Reply() => new()
    {
        Status = "success", PaymentId = saved.PaymentId, ConversationId = saved.RequestId, BasketId = saved.OrderNumber, Currency = "TRY", FraudStatus = 1,
        PaidPrice = saved.OrderItems.Sum(i => i.Price * i.Quantity).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
    };
    Check(IyzicoPaymentGateway.Verify(Reply(), saved).Status == OrderStatus.Completed, "matching provider reply is accepted");
    var mismatched = Reply(); mismatched.PaidPrice = "0.01";
    Check(IyzicoPaymentGateway.Verify(mismatched, saved).Status == OrderStatus.PaymentUncertain, "wrong paid amount cannot complete order");
    mismatched = Reply(); mismatched.ConversationId = "different-request";
    Check(IyzicoPaymentGateway.Verify(mismatched, saved).Status == OrderStatus.PaymentUncertain, "wrong provider conversation cannot complete order");
    mismatched = Reply(); mismatched.FraudStatus = 0;
    Check(IyzicoPaymentGateway.Verify(mismatched, saved).Status == OrderStatus.PaymentUncertain, "pending provider risk review cannot complete order");
    mismatched = Reply(); mismatched.PaymentStatus = "INIT_THREEDS";
    Check(IyzicoPaymentGateway.Verify(mismatched, saved, true).Status == OrderStatus.PaymentUncertain, "successful lookup alone does not mean successful payment");
    mismatched = Reply(); mismatched.PaymentStatus = "FAILURE";
    Check(IyzicoPaymentGateway.Verify(mismatched, saved, true).Status == OrderStatus.PaymentFailed, "positive provider failure confirmation permits safe retry");
    mismatched = Reply(); mismatched.BasketId = "another-order";
    Check(IyzicoPaymentGateway.Verify(mismatched, saved).Status == OrderStatus.PaymentUncertain, "provider reply for another basket cannot complete order");

    // Real MVC + Identity + antiforgery, with captured mail and no network payment calls.
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = webRoot, EnvironmentName = "Development", ApplicationName = typeof(AccountController).Assembly.GetName().Name });
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddSingleton<IDbContextFactory<DataContext>>(factory);
    builder.Services.AddDbContext<ApplicationIdentityDbContext>(o => o.UseSqlServer(identityConnection));
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => { o.SignIn.RequireConfirmedEmail = true; o.User.RequireUniqueEmail = true; })
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>().AddDefaultTokenProviders();
    builder.Services.ConfigureApplicationCookie(o => o.Cookie.SameSite = SameSiteMode.Lax);
    builder.Services.AddScoped<IBasketDal, EfCoreBasketDal>();
    builder.Services.AddScoped<IBasketServices, BasketManager>();
    builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
    builder.Services.AddScoped<IProductServices, ProductManager>();
    builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
    builder.Services.AddScoped<ICommentServices, CommentManager>();
    builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
    builder.Services.AddScoped<ICategoryServices, CategoryManager>();
    builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
    builder.Services.AddScoped<IOrderServices, OrderManager>();
    builder.Services.AddScoped<ICheckoutDal, EfCoreCheckoutDal>();
    builder.Services.AddSingleton<IPaymentGateway>(gateway);
    builder.Services.AddScoped<CheckoutService>();
    var mail = new CapturedMail();
    builder.Services.AddSingleton<IAccountEmailSender>(mail);
    builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("account-email", limit => { limit.PermitLimit = 100; limit.Window = TimeSpan.FromMinutes(1); }));
    builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())).AddApplicationPart(typeof(AccountController).Assembly);
    app = builder.Build();
    app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    await app.StartAsync();
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new() }) { BaseAddress = new Uri(address) };
    async Task<HttpResponseMessage> Post(string path, Dictionary<string,string> values, string? formPath = null)
    {
        var formHtml = await client.GetStringAsync(formPath ?? path);
        var match = Regex.Match(formHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if (!match.Success) throw new Exception("Missing antiforgery form at " + path);
        values["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }
    async Task<ApplicationUser> UserRecord()
    {
        await using var scope = app.Services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByNameAsync("checkuser"))!;
    }
    string MailPath()
    {
        var url = Regex.Match(mail.Last!.TextBody, @"https?://[^\s]+").Value;
        return new Uri(url).PathAndQuery;
    }
    var registered = await Post("/Account/Register", new() { ["FullName"]="Check User", ["UserName"]="checkuser", ["Email"]="check@example.test", ["Password"]="Strong1!", ["RePassword"]="Strong1!" });
    Check(registered.StatusCode == HttpStatusCode.Redirect && mail.Count == 1, "registration sends branded confirmation through injected sender");
    var confirmation = MailPath();
    await client.GetAsync(confirmation); await client.GetAsync(confirmation);
    var account = await UserRecord();
    Check(account.EmailConfirmed && await data.Baskets.CountAsync(b => b.UserId == account.Id) == 1, "confirmation link works twice without duplicate basket");
    var duplicate = await Post("/Account/Register", new() { ["FullName"]="Check User", ["UserName"]="checkuser", ["Email"]="check@example.test", ["Password"]="Strong1!", ["RePassword"]="Strong1!" });
    Check(duplicate.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await duplicate.Content.ReadAsStringAsync()).Contains("kullanılıyor"), "duplicate registration displays Identity errors");
    var login = await Post("/Account/Login", new() { ["Email"]="check@example.test", ["Password"]="Strong1!" });
    Check(login.StatusCode == HttpStatusCode.Redirect, "confirmed account can sign in");
    await Post("/Account/Manage", new() { ["FullName"]="Check User", ["UserName"]="checkuser", ["Email"]="changed@example.test", ["CurrentPassword"]="wrong" });
    Check((await UserRecord()).Email == "check@example.test", "email change requires correct current password");
    await Post("/Account/Manage", new() { ["FullName"]="Check User", ["UserName"]="checkuser", ["Email"]="changed@example.test", ["CurrentPassword"]="Strong1!" });
    Check((await UserRecord()).Email == "check@example.test" && mail.Recipient == "changed@example.test", "old email remains active until new address is confirmed");
    await client.GetAsync(MailPath());
    Check((await UserRecord()).Email == "changed@example.test" && (await UserRecord()).EmailConfirmed, "new-address confirmation updates email");
    await Post("/Account/ForgotPassword", new() { ["Email"]="changed@example.test" });
    var resetPath = MailPath();
    var resetToken = QueryHelpers.ParseQuery(new Uri(client.BaseAddress!, resetPath).Query)["token"].ToString();
    var reset = await Post(resetPath, new() { ["Token"]=resetToken, ["Email"]="changed@example.test", ["Password"]="NewStrong2!" });
    Check(reset.StatusCode == HttpStatusCode.Redirect, "signed-in reset form accepts valid antiforgery token and redirects");
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Check(await manager.CheckPasswordAsync((await manager.FindByNameAsync("checkuser"))!, "NewStrong2!"), "reset actually updates the password");
    }
    var reused = await Post(resetPath, new() { ["Token"]=resetToken, ["Email"]="changed@example.test", ["Password"]="OtherStrong3!" });
    Check(reused.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await reused.Content.ReadAsStringAsync()).Contains("geçersiz"), "used reset token is rejected with useful message");
    var csrf = await client.PostAsync("/Account/ResetPassword", new FormUrlEncodedContent(new Dictionary<string,string> { ["Token"]=resetToken, ["Email"]="changed@example.test", ["Password"]="OtherStrong3!" }));
    Check(csrf.StatusCode == HttpStatusCode.BadRequest, "missing antiforgery token is still rejected");
    Check((await client.GetAsync("/Comment/GetComments")).StatusCode == HttpStatusCode.BadRequest, "missing comment product ID returns 400, not 500");
    Check((await client.GetAsync("/Comment/GetComments?productId=" + product.Id)).StatusCode == HttpStatusCode.OK, "legacy comment endpoint renders existing partial");
    Check((await client.GetAsync("/Comment/GetCommentsByUserName?userName=checkuser")).StatusCode == HttpStatusCode.OK, "username comment lookup resolves Identity user without unimplemented DAL");
    Check((await client.GetAsync("/Comment/GetCommentsByRating?rating=6")).StatusCode == HttpStatusCode.BadRequest, "invalid comment rating rejected");
    var comment = await Post("/Comment/Create", new() { ["ProductId"] = product.Id.ToString(), ["Text"] = "Synthetic review", ["Raiting"] = "5" }, "/Comment/GetComments?productId=" + product.Id);
    Check(comment.StatusCode == HttpStatusCode.OK && (await comment.Content.ReadAsStringAsync()).Contains("true"), "comment form posts with antiforgery token");
    Check((await client.GetStringAsync("/Comment/GetCommentsByUserName?userName=checkuser")).Contains("Synthetic review"), "username query returns stored comments");
    Check((await client.GetAsync("/PaymentReview")).StatusCode == HttpStatusCode.Redirect, "ordinary customer cannot view payment review queue");
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole("admin"));
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await manager.AddToRoleAsync((await manager.FindByNameAsync("checkuser"))!, "admin");
    }
    await Post("/Account/Login", new() { ["Email"] = "changed@example.test", ["Password"] = "NewStrong2!" });
    Check((await client.GetAsync("/PaymentReview")).StatusCode == HttpStatusCode.OK, "admin payment queue renders");
    var callsBeforeReview = gateway.Calls;
    var reviewed = await Post("/PaymentReview/Verify", new() { ["id"] = unknown.Id.ToString() }, "/PaymentReview");
    Check(reviewed.StatusCode == HttpStatusCode.Redirect && new EfCoreOrderDal(factory).GetOrders("unknown-test").Single().OrderEnums == OrderStatus.Completed,
        "admin can settle uncertain order after provider verification");
    Check(gateway.Calls == callsBeforeReview && baskets.CartByUserId("unknown-test").BasketItems.Count == 0,
        "reconciliation never charges again and clears basket atomically");
    baskets.AddItem("unknown-test", product.Id, 1);
    Check(baskets.CartByUserId("unknown-test").BasketItems.Count == 1, "verified payment releases basket lock");
    Console.WriteLine($"SUCCESS: {checks} checks; no real email or provider payment sent.");
}
finally
{
    if (app != null) { await app.StopAsync(); await app.DisposeAsync(); }
    // Only databases generated by this process may be removed.
    if (!Regex.IsMatch(databaseName, "^Prodora_Checks_[a-f0-9]{32}$")) throw new Exception("Unsafe test database name");
    await identity.Database.EnsureDeletedAsync();
    await data.Database.EnsureDeletedAsync();
    Console.WriteLine("Isolated test databases removed.");
}
sealed class TestContextFactory(string connection) : IDbContextFactory<DataContext>
{
    public DataContext CreateDbContext() => new(new DbContextOptionsBuilder<DataContext>().UseSqlServer(connection).Options);
}
sealed class FakeGateway : IPaymentGateway
{
    public int Calls;
    public bool IsConfigured => true;
    public bool Throw { get; init; }
    public OrderStatus Outcome { get; init; } = OrderStatus.Completed;
    public Task<PaymentOutcome> RetrieveAsync(Order order) => Task.FromResult(new PaymentOutcome(Outcome, "synthetic-payment", order.RequestId!));
    public async Task<PaymentOutcome> PayAsync(Order order, OrderModels model, string ip)
    {
        Interlocked.Increment(ref Calls);
        await Task.Delay(100);
        if (Throw) throw new HttpRequestException("Synthetic timeout");
        return new(Outcome, Outcome == OrderStatus.Completed ? "synthetic-payment" : "", order.RequestId!);
    }
}
sealed class CapturedMail : IAccountEmailSender
{
    public BrandedEmail? Last { get; private set; }
    public string? Recipient { get; private set; }
    public int Count { get; private set; }
    public Task<bool> SendAsync(BrandedEmail email, string recipient)
    {
        Last = email; Recipient = recipient; Count++;
        return Task.FromResult(true);
    }
}
