using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
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

        public override Member[] GetByIds(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var retVal = base.GetByIds(memberIds, responseGroup, memberTypes);
            Parallel.ForEach(retVal, new ParallelOptions { MaxDegreeOfParallelism = 10 }, member =>
            {
                //Fully load security accounts for members which support them 
                var hasSecurityAccounts = member as IHasSecurityAccounts;
                if (hasSecurityAccounts != null)
                {
                    //Fully load all security accounts associated with this contact
                    foreach (var account in hasSecurityAccounts.SecurityAccounts.ToArray())
                    {
                        var result = Task.Run(() => _securityService.FindByIdAsync(account.Id, UserDetails.Full));
                        hasSecurityAccounts.SecurityAccounts.Remove(account);
                        hasSecurityAccounts.SecurityAccounts.Add(result.Result);
                    }
                }
            });
            return retVal;
        }
    }
}