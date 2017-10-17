using System;
using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Repositories;
using VirtoCommerce.CoreModule.Data.Converters;
using VirtoCommerce.CoreModule.Data.Services;
using VirtoCommerce.CustomerModule.Data.Extensions;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using domainModel = VirtoCommerce.Domain.Commerce.Model;
using dataModel = VirtoCommerce.CoreModule.Data.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateCommerceServiceImpl: CommerceServiceImpl, ICorporateCommerceService, ICorporateCommerceSearchService
    {
        private readonly Func<ICorporateCommerceRepository> _repositoryFactory;

        public CorporateCommerceServiceImpl(Func<ICorporateCommerceRepository> repositoryFactory) : base(repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public virtual domainModel.FulfillmentCenter[] GetFulfillmentCentersById(string[] ids)
        {
            domainModel.FulfillmentCenter[] result = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    result = repository.GetFulfillmentCentersByIds(ids).Select(x => x.ToCoreModel()).ToArray();
                }
            }
            return result;
        }

        // Mock. TODO: write implementation
        public virtual GenericSearchResult<domainModel.FulfillmentCenter> SearchFulfillmentCenters(FulfillmentCenterSearchCriteria criteria)
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = repository.FulfillmentCenters;

                // TODO

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<dataModel.FulfillmentCenter>(x => x.Name), SortDirection = SortDirection.Descending } };
                }
                query = query.OrderBySortInfos(sortInfos);

                var result = new GenericSearchResult<domainModel.FulfillmentCenter> { TotalCount = query.Count() };
                var fulfillmentIds = query.Select(x => x.Id).Skip(criteria.Skip).Take(criteria.Take).ToArray();
                result.Results = GetFulfillmentCentersById(fulfillmentIds);
                return result;
            }
        }
    }
}