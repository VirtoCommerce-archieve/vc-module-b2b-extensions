using System.Linq;
using VirtoCommerce.CoreModule.Data.Model;
using VirtoCommerce.CoreModule.Data.Repositories;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public class CorporateCommerceRepository: CommerceRepositoryImpl, ICorporateCommerceRepository
    {
        public CorporateCommerceRepository()
        {
        }

        public CorporateCommerceRepository(string nameOrConnectionString, params IInterceptor[] interceptors) : base(nameOrConnectionString, interceptors)
        {
        }

        public FulfillmentCenter[] GetFulfillmentCentersByIds(string[] ids)
        {
            return FulfillmentCenters.Where(x => ids.Contains(x.Id)).ToArray();
        }
    }
}