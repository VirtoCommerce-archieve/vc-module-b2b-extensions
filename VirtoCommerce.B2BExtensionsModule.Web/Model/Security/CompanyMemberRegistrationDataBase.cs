using System.Collections.Generic;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    public abstract class CompanyMemberRegistrationDataBase : RegistrationDataBase
    {
        public string CompanyId { get; set; }

        public override CompanyMember ToCompanyMember(CompanyMember companyMember, string memberId)
        {
            var retVal = base.ToCompanyMember(companyMember, memberId);
            retVal.Organizations = new List<string> { CompanyId };
            return retVal;
        }
    }
}