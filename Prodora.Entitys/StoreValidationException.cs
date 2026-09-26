namespace Prodora.Entitys;

// Only these deliberately authored messages may be shown directly to a shopper.
public sealed class StoreValidationException(string message) : InvalidOperationException(message);
