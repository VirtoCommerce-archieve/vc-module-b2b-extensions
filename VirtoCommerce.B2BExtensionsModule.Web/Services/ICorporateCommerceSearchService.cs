using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public interface ICorporateCommerceSearchService
    {
        GenericSearchResult<FulfillmentCenter> SearchFulfillmentCenters(FulfillmentCenterSearchCriteria criteria);
    }
}