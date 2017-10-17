using VirtoCommerce.CoreModule.Data.Model;
using VirtoCommerce.CoreModule.Data.Repositories;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public interface ICorporateCommerceRepository : IСommerceRepository
    {
        FulfillmentCenter[] GetFulfillmentCentersByIds(string[] ids);
    }
}