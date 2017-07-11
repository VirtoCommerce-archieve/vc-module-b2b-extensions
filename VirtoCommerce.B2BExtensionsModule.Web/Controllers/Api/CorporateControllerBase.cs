using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    public class CorporateControllerBase : ApiController
    {
        private readonly ISecurityService _securityService;

        public CorporateControllerBase(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        protected void CheckCurrentUserHasPermissionForCompanyMember(IEnumerable<string> companyMemberUserNames)
        {
            // Check if user has permission
            if (_securityService.UserHasAnyPermission(User.Identity.Name, null, B2BPredefinedPermissions.CompanyMembers) ||
            // Allow user to read & update himself
                companyMemberUserNames != null && companyMemberUserNames.Contains(User.Identity.Name)) return;
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }
    }
}