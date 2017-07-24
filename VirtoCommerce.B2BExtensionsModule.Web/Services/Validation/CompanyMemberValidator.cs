using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.Domain.Customer.Services;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services.Validation
{
    [CLSCompliant(false)]
    public class CompanyMemberValidator : AbstractValidator<CompanyMember>
    {
        public CompanyMemberValidator(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            // User must have only one and unique email
            RuleFor(x => x.Emails).NotNull().Must(e => e.Count == 1).WithMessage(B2BExtensionsResources.InvalidEmailCount)
            .Must(e => memberSearchService.SearchMembers(new CorporateMembersSearchCriteria { Email = e.SingleOrDefault(), MemberType = typeof(CompanyMember).Name }).TotalCount == 0)
            .WithMessage(string.Format(B2BExtensionsResources.EmailAlreadyUsed, Constants.PropertyValue));

            // User must me a member of one and only one company
            RuleFor(x => x.Organizations).NotNull().Must(o => o.Count == 1).WithMessage(B2BExtensionsResources.InvalidCompanyCount)
            .Must(o => memberService.GetByIds(new[] { o.SingleOrDefault() }).Length == 1)
            .WithMessage(string.Format(B2BExtensionsResources.CompanyDoesNotExist, string.Format(B2BExtensionsResources.WithId, Constants.PropertyValue)));
        }
    }
}