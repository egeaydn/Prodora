using Prodora.Entitys;

namespace Prodora.Business.Abstract;

public interface IAddressServices
{
    List<SavedAddress> GetAll(string userId);
    SavedAddress? Get(string userId, int id);
    SavedAddress Add(string userId, SavedAddress address);
    void SetDefault(string userId, int id);
    void Delete(string userId, int id);
}
