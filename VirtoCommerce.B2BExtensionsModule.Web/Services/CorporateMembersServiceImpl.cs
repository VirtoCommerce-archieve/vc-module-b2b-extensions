using System;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateMembersServiceImpl : CommerceMembersServiceImpl
    {
        public CorporateMembersServiceImpl(Func<ICustomerRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, ISecurityService securityService, IEventPublisher<MemberChangingEvent> eventPublisher, ICommerceService commerceService)
            : base(repositoryFactory, dynamicPropertyService, commerceService, securityService, eventPublisher)
        {
        }
    }
}