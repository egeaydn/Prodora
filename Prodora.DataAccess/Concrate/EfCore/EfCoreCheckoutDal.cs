using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;
namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreCheckoutDal(IDbContextFactory<DataContext> factory) : ICheckoutDal
{
    public List<Order> UnresolvedOrders()
    {
        using var context = factory.CreateDbContext();
        return context.Orders.AsNoTracking().Include(o => o.OrderItems)
            .Where(o => o.OrderEnums == OrderStatus.PaymentPending || o.OrderEnums == OrderStatus.PaymentUncertain)
            .OrderBy(o => o.OrderDate).ToList();
    }

    public PreparedOrder Prepare(Order draft)
    {
        if (!Guid.TryParseExact(draft.RequestId, "N", out _)) throw new StoreValidationException("Ödeme sayfasını yenileyip tekrar dene.");
        using var context = factory.CreateDbContext();
        using var transaction = CommerceTransaction.Begin(context, draft.UserId);
        var existing = context.Orders.Include(o => o.OrderItems).SingleOrDefault(o => o.RequestId == draft.RequestId);
        if (existing != null)
        {
            if (existing.UserId != draft.UserId) throw new StoreValidationException("Ödeme sayfasını yenileyip tekrar dene.");
            transaction.Commit();
            return new(existing, false);
        }
        CommerceTransaction.EnsureEditable(context, draft.UserId);
        var basket = context.Baskets.Include(b => b.BasketItems).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .OrderBy(b => b.Id).FirstOrDefault(b => b.UserId == draft.UserId);
        if (basket == null || basket.BasketItems.Count == 0) throw new StoreValidationException("Sepetinde ürün yok.");
        if (basket.BasketItems.Any(i => i.Product == null || i.Product.IsArchived || !i.Product.Stock || i.Product.Price <= 0 || i.Quantity < 1 || i.Quantity > 99))
            throw new StoreValidationException("Sepetindeki bazı ürünler artık satışta değil veya adetleri geçersiz. Sepetini düzenle.");
        if (basket.BasketItems.GroupBy(i => i.ProductId).Any(g => g.Sum(i => (long)i.Quantity) > 99))
            throw new StoreValidationException("Bir üründen en fazla 99 adet sipariş verebilirsin.");
        if (draft.PaymentEnum is not (OrderPayments.CreditCard or OrderPayments.Eft))
            throw new StoreValidationException("Geçerli bir ödeme yöntemi seç.");
        draft.OrderItems = basket.BasketItems.Select(i => new OrderItem
        {
            ProductId = i.ProductId, ProductName = i.Product.Name,
            ProductImage = i.Product.Images.FirstOrDefault()?.ImageUrl ?? "product-placeholder.svg",
            Price = i.Product.Price, Quantity = i.Quantity
        }).ToList();
        draft.OrderEnums = draft.PaymentEnum == OrderPayments.Eft ? OrderStatus.Pending : OrderStatus.PaymentPending;
        context.Orders.Add(draft);
        if (draft.PaymentEnum == OrderPayments.Eft) context.RemoveRange(basket.BasketItems);
        context.SaveChanges();
        transaction.Commit();
        return new(draft, true);
    }

    public void Complete(int orderId, string userId, OrderStatus status, string paymentId, string conversationId)
    {
        if (status is not (OrderStatus.Completed or OrderStatus.PaymentFailed or OrderStatus.PaymentUncertain))
            throw new ArgumentOutOfRangeException(nameof(status));
        if (status == OrderStatus.Completed && string.IsNullOrWhiteSpace(paymentId))
            throw new ArgumentException("Successful payments require a provider payment ID.", nameof(paymentId));
        using var context = factory.CreateDbContext();
        using var transaction = CommerceTransaction.Begin(context, userId);
        var order = context.Orders.Include(o => o.OrderItems).Single(o => o.Id == orderId && o.UserId == userId);
        if (order.OrderEnums is not (OrderStatus.PaymentPending or OrderStatus.PaymentUncertain)) { transaction.Commit(); return; }
        order.OrderEnums = status;
        order.PaymentId = paymentId;
        order.ConversionId = conversationId;
        if (status == OrderStatus.Completed)
        {
            var basket = context.Baskets.Include(b => b.BasketItems).OrderBy(b => b.Id).FirstOrDefault(b => b.UserId == userId);
            if (basket != null) context.RemoveRange(basket.BasketItems);
        }
        context.SaveChanges();
        transaction.Commit();
    }
}
