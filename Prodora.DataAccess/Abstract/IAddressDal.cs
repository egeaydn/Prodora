using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract;

public interface IAddressDal
{
    /// <summary>The customer's addresses, default first, then newest.</summary>
    List<SavedAddress> GetAll(string userId);
    SavedAddress? Get(string userId, int id);
    /// <summary>Saves the address. The first one becomes the default; an identical address is not saved twice.</summary>
    SavedAddress Add(string userId, SavedAddress address);
    void SetDefault(string userId, int id);
    /// <summary>Removes an address. When it was the default, the newest remaining address takes over.</summary>
    void Delete(string userId, int id);
}
