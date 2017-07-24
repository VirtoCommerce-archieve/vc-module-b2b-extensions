using FluentValidation.Attributes;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;
using VirtoCommerce.Domain.Customer.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model
{
    [Validator(typeof(CompanyMemberValidator))]
    public class CompanyMember : Employee
    {
        public string Title { get; set; }
    }
}