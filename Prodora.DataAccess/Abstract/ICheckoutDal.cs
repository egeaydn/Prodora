using Prodora.Entitys;
namespace Prodora.DataAccess.Abstract;
public record PreparedOrder(Order Order, bool Created);
public interface ICheckoutDal
{
    PreparedOrder Prepare(Order draft);
    List<Order> UnresolvedOrders();
    void Complete(int orderId, string userId, OrderStatus status, string paymentId, string conversationId);
}
