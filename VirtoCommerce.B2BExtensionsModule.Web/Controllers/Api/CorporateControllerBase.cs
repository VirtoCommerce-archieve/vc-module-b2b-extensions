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

        protected void CheckCurrentUserHasPermissionForUser(string permission, IEnumerable<string> userNames)
        {
            //Scope bound security check
            if (!_securityService.UserHasAnyPermission(User.Identity.Name, null, permission) &&
                !(userNames != null && userNames.Contains(User.Identity.Name) && permission == B2BPredefinedPermissions.CompanyMembers))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
        }
    }
}