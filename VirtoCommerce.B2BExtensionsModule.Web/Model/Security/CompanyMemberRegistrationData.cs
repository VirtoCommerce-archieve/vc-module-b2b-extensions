using FluentValidation.Attributes;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    [Validator(typeof(CompanyMemberRegistrationDataValidator))]
    public class CompanyMemberRegistrationData : CompanyMemberRegistrationDataBase
    {
        public string Role { get; set; }

        public override CompanyMember ToCompanyMember(CompanyMember companyMember, string memberId)
        {
            var retVal = base.ToCompanyMember(companyMember, memberId);
            retVal.IsActive = false;
            return retVal;
        }
    }
}