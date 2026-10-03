using Prodora.WebUI.EmailServices;

internal sealed class NoopEmailSender : IAccountEmailSender
{
    public BrandedEmail? Last { get; private set; }
    public int Count { get; private set; }
    public Task<bool> SendAsync(BrandedEmail email, string recipient)
    {
        Last = email;
        Count++;
        return Task.FromResult(true);
    }
}
