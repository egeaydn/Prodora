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
using Prodora.WebUI.Orders;
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
Check(AccountLinkBuilder.TryParseOrigin("https://shop.example.test", out _) &&
    !AccountLinkBuilder.TryParseOrigin("javascript:alert(1)", out _) &&
    !AccountLinkBuilder.TryParseOrigin("https://shop.example.test/path", out _) &&
    !AccountLinkBuilder.TryParseOrigin("https://shop.example.test/?next=evil", out _) &&
    !AccountLinkBuilder.TryParseOrigin("https://user:pass@shop.example.test", out _),
    "account-link origin accepts only a plain HTTP/HTTPS origin");
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
    var orderDal = new EfCoreOrderDal(factory);
    var cancelService = new OrderCancellationService(new OrderManager(orderDal), gateway, NullLogger<OrderCancellationService>.Instance);
    Check((await cancelService.CancelAsOwnerAsync(eft.Id, "someone-else", "127.0.0.1")).Result == CancelResult.NotFound, "customer cannot cancel another user's order");
    Check(orderDal.GetStoreOrders("open").Any(o => o.Id == eft.Id) && !orderDal.GetStoreOrders("closed").Any(o => o.Id == eft.Id), "unpaid EFT order is listed as open");
    Check((await cancelService.CancelAsOwnerAsync(eft.Id, "eft-test", "127.0.0.1")).Result == CancelResult.Cancelled
        && orderDal.GetOrder(eft.Id)!.OrderEnums == OrderStatus.Cancelled, "customer cancels an unpaid EFT order");
    Check((await cancelService.CancelAsOwnerAsync(eft.Id, "eft-test", "127.0.0.1")).Result == CancelResult.NotAllowed, "cancelled order cannot be cancelled twice");
    Check(orderDal.GetStoreOrders("closed").Any(o => o.Id == eft.Id), "cancelled order is listed as closed");
    Rejected(() => orderDal.SetStatus(eft.Id, OrderStatus.Processing), "cancelled order cannot be reopened");
    var refundGateway = new FakeGateway();
    var refundService = new CheckoutService(checkoutDal, refundGateway, NullLogger<CheckoutService>.Instance);
    baskets.AddItem("refund-test", product.Id, 1);
    var paid = await refundService.SubmitAsync(Form(), "refund-test", "credit", "127.0.0.1");
    var refundCancel = new OrderCancellationService(new OrderManager(orderDal), refundGateway, NullLogger<OrderCancellationService>.Instance);
    Check((await refundCancel.CancelAsOwnerAsync(paid.Id, "refund-test", "127.0.0.1")).Result == CancelResult.Cancelled
        && refundGateway.Cancels == 1 && orderDal.GetOrder(paid.Id)!.OrderEnums == OrderStatus.Cancelled, "paid card order is refunded, then cancelled");
    var rejectGateway = new FakeGateway { CancelSucceeds = false };
    var rejectService = new CheckoutService(checkoutDal, rejectGateway, NullLogger<CheckoutService>.Instance);
    baskets.AddItem("norefund-test", product.Id, 1);
    var kept = await rejectService.SubmitAsync(Form(), "norefund-test", "credit", "127.0.0.1");
    var rejectCancel = new OrderCancellationService(new OrderManager(orderDal), rejectGateway, NullLogger<OrderCancellationService>.Instance);
    Check((await rejectCancel.CancelAsOwnerAsync(kept.Id, "norefund-test", "127.0.0.1")).Result == CancelResult.RefundFailed
        && orderDal.GetOrder(kept.Id)!.OrderEnums == OrderStatus.Completed, "rejected refund leaves the paid order untouched");
    Rejected(() => orderDal.SetStatus(paid.Id, OrderStatus.Delivered), "order cannot skip from paid to delivered");
    Check(orderDal.SetStatus(kept.Id, OrderStatus.Processing).Order.OrderEnums == OrderStatus.Processing, "admin moves a paid order to processing");
    Rejected(() => orderDal.SetStatus(kept.Id, OrderStatus.Shipped), "shipping needs a tracking number");
    Rejected(() => orderDal.SetStatus(kept.Id, OrderStatus.Shipped, "<b>x</b>"), "unsafe tracking number rejected");
    Check(orderDal.GetOrder(kept.Id)!.OrderEnums == OrderStatus.Processing && orderDal.GetOrder(kept.Id)!.TrackingNumber == null, "failed shipping attempt changes nothing");
    var shipped = orderDal.SetStatus(kept.Id, OrderStatus.Shipped, " TR-12345 ");
    Check(shipped.Previous == OrderStatus.Processing && orderDal.GetOrder(kept.Id)!.TrackingNumber == "TR-12345", "shipping stores the trimmed tracking number");
    Check((await rejectCancel.CancelAsAdminAsync(kept.Id, "127.0.0.1")).Result == CancelResult.NotAllowed, "shipped order cannot be cancelled by admin");
    Check((await refundCancel.CancelAsAdminAsync(orderDal.GetStoreOrders("all").First(o => o.UserId == "refund-test").Id, "127.0.0.1")).Result == CancelResult.NotAllowed, "already cancelled order cannot be cancelled again by admin");
    baskets.AddItem("admin-cancel-test", product.Id, 1);
    var prepared = await refundService.SubmitAsync(Form(), "admin-cancel-test", "credit", "127.0.0.1");
    orderDal.SetStatus(prepared.Id, OrderStatus.Processing);
    var cancelsBefore = refundGateway.Cancels;
    Check((await refundCancel.CancelAsAdminAsync(prepared.Id, "127.0.0.1")).Result == CancelResult.Cancelled
        && refundGateway.Cancels == cancelsBefore + 1 && orderDal.GetOrder(prepared.Id)!.OrderEnums == OrderStatus.Cancelled, "admin cancels and refunds an order being prepared");
    var couponDal = new EfCoreCouponDal(factory);
    couponDal.Create(new Coupon { Code = "save10", Kind = CouponKind.Percent, Value = 10 });
    couponDal.Create(new Coupon { Code = "ONEUSE", Kind = CouponKind.FixedAmount, Value = 5m, MaxRedemptions = 1 });
    couponDal.Create(new Coupon { Code = "OLD", Kind = CouponKind.Percent, Value = 10, ExpiresAt = DateTime.Now.AddDays(-1) });
    couponDal.Create(new Coupon { Code = "BIGSPEND", Kind = CouponKind.Percent, Value = 10, MinSubtotal = 1000m });
    Rejected(() => couponDal.Create(new Coupon { Code = "SAVE10", Kind = CouponKind.Percent, Value = 5 }), "duplicate coupon code rejected");
    Rejected(() => couponDal.Create(new Coupon { Code = "BAD", Kind = CouponKind.Percent, Value = 95 }), "out-of-range coupon rejected");
    Check(couponDal.GetByCode(" Save10 ")?.Code == "SAVE10", "coupon codes are stored upper-cased and found regardless of case");
    int Uses(string code) => couponDal.GetAll().Single(c => c.Code == code).RedemptionCount;
    var couponGateway = new FakeGateway();
    var couponService = new CheckoutService(checkoutDal, couponGateway, NullLogger<CheckoutService>.Instance);
    var couponCancel = new OrderCancellationService(new OrderManager(orderDal), couponGateway, NullLogger<OrderCancellationService>.Instance);
    OrderModels CouponForm(string? code) { var f = Form(); f.CouponCode = code; return f; }
    async Task<Order> BuyWith(CheckoutService service, string user, string? code)
    {
        baskets.AddItem(user, product.Id, 1);
        return await service.SubmitAsync(CouponForm(code), user, "credit", "127.0.0.1");
    }
    async Task<bool> Refused(string user, string? code)
    {
        baskets.AddItem(user, product.Id, 1);
        try { await couponService.SubmitAsync(CouponForm(code), user, "credit", "127.0.0.1"); return false; }
        catch (StoreValidationException) { return true; }
    }
    var discounted = await BuyWith(couponService, "coupon-a", "save10");
    var unitPrice = discounted.OrderItems.Single().Price;
    Check(discounted.CouponCode == "SAVE10" && discounted.DiscountAmount == decimal.Round(unitPrice * 0.10m, 2, MidpointRounding.AwayFromZero)
        && discounted.Total == unitPrice - discounted.DiscountAmount, "percent coupon discounts the order on the server");
    Check(Uses("SAVE10") == 1, "coupon use is counted");
    Check(await Refused("coupon-a", "SAVE10"), "same customer cannot reuse a coupon");
    Iyzipay.Model.Payment ProviderReply(Order o, decimal paid) => new()
    {
        Status = "success", PaymentId = "p", ConversationId = o.RequestId, BasketId = o.OrderNumber, Currency = "TRY", FraudStatus = 1,
        PaidPrice = paid.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
    };
    Check(IyzicoPaymentGateway.Verify(ProviderReply(discounted, discounted.Total), discounted).Status == OrderStatus.Completed, "provider paid amount is checked against the discounted total");
    Check(IyzicoPaymentGateway.Verify(ProviderReply(discounted, discounted.Subtotal), discounted).Status == OrderStatus.PaymentUncertain, "full price paid for a discounted order is not accepted");
    Check(await Refused("coupon-b", "OLD"), "expired coupon refused");
    Check(await Refused("coupon-b", "NOSUCH"), "unknown coupon refused");
    Check(await Refused("coupon-b", "BIGSPEND"), "coupon below its minimum basket is refused");
    Check(await Refused("coupon-b", "x'; DROP TABLE Coupons;--"), "malformed coupon code refused");
    Check(baskets.CartByUserId("coupon-b").BasketItems.Count == 1 && !orderDal.GetOrders("coupon-b").Any(), "refused coupon leaves the basket and creates no order");
    var plain = await couponService.SubmitAsync(CouponForm(null), "coupon-b", "credit", "127.0.0.1");
    Check(plain.DiscountAmount == 0 && plain.CouponCode == null, "order without a coupon is not discounted");
    var last = await BuyWith(couponService, "coupon-c", "ONEUSE");
    Check(last.DiscountAmount == 5m, "fixed coupon discounts the order");
    Check(await Refused("coupon-d", "ONEUSE"), "coupon with no uses left is refused for the next customer");
    orderDal.SetStatus(last.Id, OrderStatus.Processing);
    Check((await couponCancel.CancelAsAdminAsync(last.Id, "127.0.0.1")).Result == CancelResult.Cancelled && Uses("ONEUSE") == 0, "cancelling an order gives the coupon use back");
    Check((await BuyWith(couponService, "coupon-d", "ONEUSE")).DiscountAmount == 5m, "released coupon use can be taken by someone else");
    var failedCouponService = new CheckoutService(checkoutDal, new FakeGateway { Outcome = OrderStatus.PaymentFailed }, NullLogger<CheckoutService>.Instance);
    var failedWithCoupon = await BuyWith(failedCouponService, "coupon-e", "SAVE10");
    Check(failedWithCoupon.OrderEnums == OrderStatus.PaymentFailed && Uses("SAVE10") == 1, "failed payment gives the coupon use back");
    Check((await couponService.SubmitAsync(CouponForm("SAVE10"), "coupon-e", "credit", "127.0.0.1")).DiscountAmount > 0 && Uses("SAVE10") == 2, "customer can retry with the same coupon after a failed payment");
    couponDal.SetActive(couponDal.GetByCode("SAVE10")!.Id, false);
    Check(await Refused("coupon-f", "SAVE10"), "deactivated coupon refused");
    var racing = new[] { "race-1", "race-2", "race-3", "race-4" };
    couponDal.Create(new Coupon { Code = "RACE", Kind = CouponKind.FixedAmount, Value = 1m, MaxRedemptions = 2 });
    foreach (var racer in racing) baskets.AddItem(racer, product.Id, 1);
    var results = await Task.WhenAll(racing.Select(async racer =>
    {
        try { await couponService.SubmitAsync(CouponForm("RACE"), racer, "credit", "127.0.0.1"); return true; }
        catch (StoreValidationException) { return false; }
    }));
    Check(results.Count(won => won) == 2 && Uses("RACE") == 2, "concurrent buyers cannot exceed the coupon's use limit");
    var wishDal = new EfCoreWishlistDal(factory);
    Check(wishDal.Toggle("wish-a", product.Id) && wishDal.GetProductIds("wish-a").Contains(product.Id), "product is added to the wishlist");
    Check(!wishDal.GetProductIds("wish-b").Contains(product.Id) && wishDal.GetProducts("wish-b").Count == 0, "wishlists are private to each customer");
    Check(wishDal.GetProducts("wish-a").Single().Id == product.Id && wishDal.GetProducts("wish-a").Single().Images != null, "wishlist page data includes the product with its image list loaded");
    Check(!wishDal.Toggle("wish-a", product.Id) && wishDal.GetProductIds("wish-a").Count == 0, "toggling again removes it");
    Rejected(() => wishDal.Toggle("wish-a", 987654), "unknown product cannot be saved");
    Rejected(() => wishDal.Toggle("", product.Id), "anonymous customer cannot save products");
    await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() => { try { wishDal.Toggle("wish-race", product.Id); } catch (Exception) { } })));
    using (var context = factory.CreateDbContext())
        Check(context.WishlistItems.Count(w => w.UserId == "wish-race") <= 1, "concurrent toggles never create duplicate rows");
    wishDal.Toggle("wish-c", product.Id);
    using (var context = factory.CreateDbContext()) { context.Products.Find(product.Id)!.IsArchived = true; context.SaveChanges(); }
    Check(wishDal.GetProducts("wish-c").Count == 0, "archived products disappear from the wishlist");
    Rejected(() => wishDal.Toggle("wish-d", product.Id), "archived product cannot be saved");
    using (var context = factory.CreateDbContext()) { context.Products.Find(product.Id)!.IsArchived = false; context.SaveChanges(); }
    Check(wishDal.GetProducts("wish-c").Count == 1, "restored product returns to the wishlist");
    var addressDal = new EfCoreAddressDal(factory);
    SavedAddress Sample(string title, string street) => new() { Title = title, FirstName = "Ege", LastName = "Aydin", Phone = "0555 123 45 67", Address = street, City = "Istanbul" };
    var home = addressDal.Add("addr-a", Sample("Ev", "Street 1"));
    Check(home.IsDefault && home.Phone == "05551234567", "first address becomes the default and its phone is normalised");
    var office = addressDal.Add("addr-a", Sample("Is", "Street 2"));
    Check(!office.IsDefault && addressDal.GetAll("addr-a").Select(a => a.Title).SequenceEqual(new[] { "Ev", "Is" }), "second address is not the default and the default is listed first");
    Check(addressDal.Add("addr-a", Sample("Baska ad", "Street 2")).Id == office.Id && addressDal.GetAll("addr-a").Count == 2, "an identical address is not saved twice");
    Check(addressDal.GetAll("addr-b").Count == 0 && addressDal.Get("addr-b", home.Id) == null, "address books are private to each customer");
    Rejected(() => addressDal.SetDefault("addr-b", home.Id), "another customer cannot change someone else's default");
    Rejected(() => addressDal.Delete("addr-b", home.Id), "another customer cannot delete someone else's address");
    addressDal.SetDefault("addr-a", office.Id);
    Check(addressDal.GetAll("addr-a").Single(a => a.IsDefault).Id == office.Id && addressDal.GetAll("addr-a").Count(a => a.IsDefault) == 1, "changing the default leaves exactly one default");
    addressDal.Delete("addr-a", office.Id);
    Check(addressDal.GetAll("addr-a").Single().IsDefault, "deleting the default promotes the remaining address");
    Rejected(() => addressDal.Add("addr-a", Sample("", "Street 3")), "address without a title is rejected");
    Rejected(() => addressDal.Add("", Sample("Ev", "Street 3")), "anonymous customer cannot save an address");
    for (var i = 0; i < AddressRules.MaxPerUser; i++) addressDal.Add("addr-full", Sample("A" + i, "Street " + i));
    Rejected(() => addressDal.Add("addr-full", Sample("Extra", "Street extra")), "address book size is limited");
    Check(addressDal.GetAll("addr-full").Count == AddressRules.MaxPerUser && addressDal.GetAll("addr-full").Count(a => a.IsDefault) == 1, "a full address book still has one default");
    await Task.WhenAll(Enumerable.Range(0, 5).Select(i => Task.Run(() => addressDal.Add("addr-race", Sample("R" + i, "Race street " + i)))));
    Check(addressDal.GetAll("addr-race").Count == 5 && addressDal.GetAll("addr-race").Count(a => a.IsDefault) == 1, "concurrent first saves still leave exactly one default");
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
        PaidPrice = saved.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
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
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IWishlistDal, EfCoreWishlistDal>();
    builder.Services.AddScoped<IWishlistServices, WishlistManager>();
    builder.Services.AddScoped<Prodora.WebUI.Wishlist.WishlistState>();
    builder.Services.AddScoped<IAddressDal, EfCoreAddressDal>();
    builder.Services.AddScoped<IAddressServices, AddressManager>();
    builder.Services.AddScoped<ICouponDal, EfCoreCouponDal>();
    builder.Services.AddScoped<ICouponServices, CouponManager>();
    builder.Services.AddSingleton<IPaymentGateway>(gateway);
    builder.Services.AddScoped<CheckoutService>();
    var mail = new CapturedMail();
    builder.Services.AddSingleton<IAccountEmailSender>(mail);
    builder.Services.AddScoped<AccountLinkBuilder>();
    builder.Services.AddRateLimiter(o =>
    {
        o.AddFixedWindowLimiter("account-email", limit => { limit.PermitLimit = 100; limit.Window = TimeSpan.FromMinutes(1); });
        o.AddFixedWindowLimiter("account-login", limit => { limit.PermitLimit = 100; limit.Window = TimeSpan.FromMinutes(1); });
        o.AddFixedWindowLimiter("account-reset", limit => { limit.PermitLimit = 100; limit.Window = TimeSpan.FromMinutes(1); });
    });
    builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())).AddApplicationPart(typeof(AccountController).Assembly);
    app = builder.Build();
    app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
    app.MapControllerRoute("shopDetails", "shop/details/{id:int}/{slug?}", new { controller = "Shop", action = "Details" });
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    await app.StartAsync();
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new() }) { BaseAddress = new Uri(address) };
    async Task<HttpResponseMessage> Post(string path, Dictionary<string,string> values, string? formPath = null, string? requestHost = null)
    {
        var formHtml = await client.GetStringAsync(formPath ?? path);
        var match = Regex.Match(formHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if (!match.Success) throw new Exception("Missing antiforgery form at " + path);
        values["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(values)
        };
        if (requestHost != null) request.Headers.Host = requestHost;
        return await client.SendAsync(request);
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
    Check(new Uri(Regex.Match(mail.Last!.TextBody, @"https?://[^\s]+").Value).Authority == client.BaseAddress!.Authority,
        "confirmation link uses the trusted local site origin");
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
    var emailCount = mail.Count;
    var forgedHost = await Post("/Account/ForgotPassword", new() { ["Email"] = "changed@example.test" }, requestHost: "attacker.example");
    Check((int)forgedHost.StatusCode >= 400 && mail.Count == emailCount, "forged Host cannot enter a password-reset email");
    Check((await client.GetAsync("/Comment/GetComments")).StatusCode == HttpStatusCode.BadRequest, "missing comment product ID returns 400, not 500");
    Check((await client.GetAsync("/Comment/GetComments?productId=" + product.Id)).StatusCode == HttpStatusCode.OK, "legacy comment endpoint renders existing partial");
    Check((await client.GetAsync("/Comment/GetCommentsByUserName?userName=checkuser")).StatusCode == HttpStatusCode.OK, "username comment lookup resolves Identity user without unimplemented DAL");
    Check((await client.GetAsync("/Comment/GetCommentsByRating?rating=6")).StatusCode == HttpStatusCode.BadRequest, "invalid comment rating rejected");
    var comment = await Post("/Comment/Create", new() { ["ProductId"] = product.Id.ToString(), ["Text"] = "Synthetic review", ["Raiting"] = "5" }, "/Comment/GetComments?productId=" + product.Id);
    Check(comment.StatusCode == HttpStatusCode.OK && (await comment.Content.ReadAsStringAsync()).Contains("true"), "comment form posts with antiforgery token");
    Check((await client.GetStringAsync("/Comment/GetCommentsByUserName?userName=checkuser")).Contains("Synthetic review"), "username query returns stored comments");
    Check((await client.GetAsync("/PaymentReview")).StatusCode == HttpStatusCode.Redirect, "ordinary customer cannot view payment review queue");
    using var anonymous = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(address) };
    Check((await anonymous.GetAsync("/Wishlist")).StatusCode == HttpStatusCode.Redirect, "anonymous visitor is sent to sign in for the wishlist");
    var wisher = await UserRecord();
    var emptyWishlist = await client.GetStringAsync("/Wishlist");
    Check(emptyWishlist.Contains("Henüz favorin yok"), "empty wishlist explains how to add products");
    var saveIt = await Post("/Wishlist/Toggle", new() { ["productId"] = product.Id.ToString(), ["returnUrl"] = "/products" }, "/products");
    Check(saveIt.StatusCode == HttpStatusCode.Redirect && saveIt.Headers.Location!.OriginalString == "/products" && wishDal.GetProductIds(wisher.Id).Contains(product.Id), "heart saves the product and returns to the list");
    Check((await client.GetStringAsync("/products")).Contains("is-saved"), "saved product shows a filled heart");
    Check((await client.GetStringAsync("/Wishlist")).Contains(new EfCoreProductDal(factory).GetProductDetails(product.Id).Name), "wishlist page shows the saved product");
    var foreign = await Post("/Wishlist/Toggle", new() { ["productId"] = product.Id.ToString(), ["returnUrl"] = "https://evil.example/phish" }, "/products");
    Check(foreign.StatusCode == HttpStatusCode.Redirect && foreign.Headers.Location!.OriginalString.EndsWith("/Wishlist") && !wishDal.GetProductIds(wisher.Id).Contains(product.Id), "external return URL is ignored and the second tap removes the product");
    Check((await anonymous.GetAsync("/Addresses")).StatusCode == HttpStatusCode.Redirect, "anonymous visitor is sent to sign in for the address book");
    Check((await client.GetStringAsync("/Addresses")).Contains("Kayıtlı adresin yok"), "empty address book explains itself");
    var addAddress = await Post("/Addresses/Create", new() { ["title"] = "Ev", ["firstName"] = "Check", ["lastName"] = "User", ["phone"] = "0555 000 11 22", ["address"] = "Saved Street 5", ["city"] = "Izmir" }, "/Addresses");
    Check(addAddress.StatusCode == HttpStatusCode.Redirect && addressDal.GetAll(wisher.Id).Single().IsDefault, "address form saves the first address as default");
    var badAddress = await Post("/Addresses/Create", new() { ["title"] = "", ["firstName"] = "x", ["lastName"] = "y", ["phone"] = "1", ["address"] = "z", ["city"] = "w" }, "/Addresses");
    Check(badAddress.StatusCode == HttpStatusCode.Redirect && addressDal.GetAll(wisher.Id).Count == 1, "invalid address form saves nothing");
    baskets.AddItem(wisher.Id, product.Id, 1);
    var checkoutPage = await client.GetStringAsync("/Basket/Checkout");
    Check(checkoutPage.Contains("Saved Street 5") && checkoutPage.Contains("Kayıtlı adreslerim"), "checkout is prefilled with the default saved address");
    Check((await client.GetStringAsync("/Basket/Checkout?address=999999")).Contains("Saved Street 5") == false, "unknown address id fills nothing");
    var anotherId = addressDal.Add("addr-other", Sample("Baska", "Other Customer Street")).Id;
    Check((await client.GetStringAsync("/Basket/Checkout?address=" + anotherId)).Contains("Other Customer Street") == false, "checkout never prefills another customer's address");
    var deleteAddress = await Post("/Addresses/Delete", new() { ["id"] = addressDal.GetAll(wisher.Id).Single().Id.ToString() }, "/Addresses");
    Check(deleteAddress.StatusCode == HttpStatusCode.Redirect && addressDal.GetAll(wisher.Id).Count == 0, "address can be deleted from the address book");
    var noToken = await client.PostAsync("/Wishlist/Toggle", new FormUrlEncodedContent(new Dictionary<string, string> { ["productId"] = product.Id.ToString() }));
    Check(noToken.StatusCode == HttpStatusCode.BadRequest, "wishlist change without antiforgery token is rejected");
    var listed = new EfCoreProductDal(factory).GetProductDetails(product.Id);
    var slug = Prodora.WebUI.Seo.Slugger.Slugify(listed.Name);
    var sitemap = await client.GetAsync("/sitemap.xml");
    var sitemapText = await sitemap.Content.ReadAsStringAsync();
    Check(sitemap.StatusCode == HttpStatusCode.OK && sitemap.Content.Headers.ContentType?.MediaType == "application/xml"
        && sitemapText.Contains($"/shop/details/{product.Id}/{slug}") && sitemapText.Contains("<urlset"), "sitemap.xml lists products with slugged URLs");
    var robots = await (await client.GetAsync("/robots.txt")).Content.ReadAsStringAsync();
    Check(robots.Contains("Disallow: /Admin") && robots.Contains("Disallow: /Basket") && robots.Contains("Sitemap: ") && robots.Contains("/sitemap.xml"), "robots.txt hides private areas and points to the sitemap");
    var bare = await client.GetAsync($"/shop/details/{product.Id}");
    Check(bare.StatusCode == HttpStatusCode.MovedPermanently && bare.Headers.Location!.ToString().EndsWith($"/shop/details/{product.Id}/{slug}"), "slug-less product link redirects permanently to the canonical URL");
    Check((await client.GetAsync($"/shop/details/{product.Id}/old-name")).StatusCode == HttpStatusCode.MovedPermanently, "outdated slug redirects to the canonical URL");
    var detailsPage = await client.GetAsync($"/shop/details/{product.Id}/{slug}");
    Check(detailsPage.StatusCode == HttpStatusCode.OK && (await detailsPage.Content.ReadAsStringAsync()).Contains("rel=\"canonical\""), "canonical product page renders with a canonical link");
    Check((await client.GetAsync("/shop/details/999999/anything")).StatusCode == HttpStatusCode.NotFound, "unknown product is still 404");
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
    public int Cancels;
    public bool CancelSucceeds { get; init; } = true;
    public Task<RefundOutcome> CancelAsync(Order order, string ip)
    {
        Interlocked.Increment(ref Cancels);
        return Task.FromResult(new RefundOutcome(CancelSucceeds));
    }
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
