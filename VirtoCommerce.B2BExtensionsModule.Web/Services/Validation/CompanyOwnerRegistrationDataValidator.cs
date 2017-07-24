using System;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services.Validation
{
    [CLSCompliant(false)]
    public class CompanyOwnerRegistrationDataValidator : RegistrationDataBaseValidator<CompanyOwnerRegistrationData>
    {
        public CompanyOwnerRegistrationDataValidator(ISecurityService securityService, IStoreService storeService, IMemberSearchService memberSearchService)
            : base(securityService, storeService)
        {
            RuleFor(x => x.CompanyName)
                .Must(x =>
                {
                    var r = memberSearchService.SearchMembers(new CorporateMembersSearchCriteria { Name = x, MemberType = typeof(Company).Name });
                    return r.TotalCount == 0;
                })
                .WithMessage(string.Format(B2BExtensionsResources.CompanyAlreadyExist, Constants.PropertyValue));
        }
    }
}