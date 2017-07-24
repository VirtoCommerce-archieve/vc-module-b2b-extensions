using System;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services.Validation
{
    [CLSCompliant(false)]
    public class InviteDataValidator : AbstractValidator<InviteData>
    {
        public InviteDataValidator(IStoreService storeService, IMemberService memberService)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            RuleFor(x => x.StoreId).NotEmpty().StoreExist(storeService);
            RuleFor(x => x.CompanyId).NotEmpty().CompanyExist(memberService);
            RuleFor(x => x.Emails).NotEmpty().WithMessage(B2BExtensionsResources.EmailsIsNullOrEmpty);
            RuleForEach(x => x.Emails).EmailAddress().WithMessage(B2BExtensionsResources.EmailsIsNotValid);
            RuleFor(x => x.AdminName).NotEmpty();
            RuleFor(x => x.AdminEmail).EmailAddress().WithMessage(B2BExtensionsResources.AdminEmailIsNotValid);
            RuleFor(x => x.CallbackUrl).NotEmpty();
        }
    }
}