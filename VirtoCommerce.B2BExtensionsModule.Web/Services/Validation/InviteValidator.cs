using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.Domain.Customer.Services;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services.Validation
{
    [CLSCompliant(false)]
    public class InviteValidator : AbstractValidator<Invite>
    {
        public InviteValidator(IMemberService memberService)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            CompanyMember member = null;
            RuleFor(x => x.Value).NotEmpty()
                .Must(i => (member = memberService.GetByIds(new[] { i }).OfType<CompanyMember>().SingleOrDefault()) != null)
                .WithMessage(B2BCustomerResources.InvalidInvite)
                .Must(i => !member.SecurityAccounts.Any())
                .WithMessage(B2BCustomerResources.InviteAlreadyUsed);
        }
    }
}