using FluentValidation.Attributes;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    [Validator(typeof(CompanyMemberRegistrationByInviteDataValidator))]
    public class CompanyMemberRegistrationByInviteData : CompanyMemberRegistrationDataBase
    {
        public override CompanyMember ToCompanyMember(CompanyMember companyMember, string memberId)
        {
            var retVal = base.ToCompanyMember(companyMember, memberId);
            retVal.IsActive = true;
            return retVal;
        }
    }
}