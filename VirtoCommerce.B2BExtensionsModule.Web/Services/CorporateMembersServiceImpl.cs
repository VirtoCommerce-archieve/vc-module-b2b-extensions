using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateMembersServiceImpl : CommerceMembersServiceImpl
    {
        private readonly ISecurityService _securityService;

        public CorporateMembersServiceImpl(Func<ICustomerRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, ISecurityService securityService, IEventPublisher<MemberChangingEvent> eventPublisher, ICommerceService commerceService)
            : base(repositoryFactory, dynamicPropertyService, commerceService, securityService, eventPublisher)
        {
            _securityService = securityService;
        }

        public override GenericSearchResult<Member> SearchMembers(MembersSearchCriteria criteria)
        {
            var retVal = base.SearchMembers(criteria);
            retVal.Results.ProcessSecurityAccounts(_securityService);
            return retVal;
        }

        public override Member[] GetByIds(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var retVal = base.GetByIds(memberIds, responseGroup, memberTypes);
            retVal.ProcessSecurityAccounts(_securityService);
            return retVal;
        }
    }
}