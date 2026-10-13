using Prodora.Business.Abstract;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.Business.Concrate;

public class AddressManager(IAddressDal addressDal) : IAddressServices
{
    public List<SavedAddress> GetAll(string userId) => addressDal.GetAll(userId);
    public SavedAddress? Get(string userId, int id) => addressDal.Get(userId, id);
    public SavedAddress Add(string userId, SavedAddress address) => addressDal.Add(userId, address);
    public void SetDefault(string userId, int id) => addressDal.SetDefault(userId, id);
    public void Delete(string userId, int id) => addressDal.Delete(userId, id);
}
