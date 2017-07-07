using VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions;
using VirtoCommerce.CustomerModule.Data.Search;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateMemberSearchServiceDecorator : MemberSearchServiceDecorator
    {
        private readonly ISecurityService _securityService;

        public CorporateMemberSearchServiceDecorator(CommerceMembersServiceImpl memberSearchService, MemberIndexedSearchService memberIndexedSearchService, ISecurityService securityService) :
            base(memberSearchService, memberIndexedSearchService)
        {
            _securityService = securityService;
        }

        public override GenericSearchResult<Member> SearchMembers(MembersSearchCriteria criteria)
        {
            var retVal = base.SearchMembers(criteria);
            retVal.Results.ProcessSecurityAccounts(_securityService);
            return retVal;
        }
    }
}