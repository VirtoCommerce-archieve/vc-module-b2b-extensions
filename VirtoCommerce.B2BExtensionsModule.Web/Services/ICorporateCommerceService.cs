using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Services;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public interface ICorporateCommerceService: ICommerceService
    {
        FulfillmentCenter[] GetFulfillmentCentersById(string[] ids);
    }
}