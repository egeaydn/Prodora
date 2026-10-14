namespace Prodora.Entitys;

public sealed record StoreOrderPage(List<Order> Orders, int TotalItems, int Page, int Pages);
