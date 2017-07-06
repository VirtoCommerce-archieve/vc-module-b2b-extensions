using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateMembersController : CorporateControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly ISecurityService _securityService;
        private readonly ISecurityOptions _securityOptions;
        private readonly IRoleManagementService _roleService;

        public CorporateMembersController(IMemberService memberService, IMemberSearchService memberSearchService, ISecurityService securityService, ISecurityOptions securityOptions, IRoleManagementService roleService)
            : base(securityService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _securityService = securityService;
            _securityOptions = securityOptions;
            _roleService = roleService;
        }

        // GET: api/b2b/company/{id}
        [HttpGet]
        [Route("company/{id}")]
        [ResponseType(typeof(Company))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyInfo)]
        public IHttpActionResult GetCompanyById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        // GET: api/b2b/company/{id}
        [HttpGet]
        [Route("organizationsByCustomerId/{id}")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetOrganizationsByCustomerId(string id)
        {
            var member = _memberService.GetByIds(new[] { id }).Cast<CompanyMember>().FirstOrDefault();
            CheckCurrentUserHasPermissionForUser(B2BPredefinedPermissions.CompanyMembers, member?.SecurityAccounts.Select(x => x.UserName));
            if (member != null && member.Organizations.Any()) {
                return Ok(member.Organizations);
            }
            return NotFound();
        }

        // POST: api/b2b/company
        [HttpPost]
        [Route("company")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyInfo)]
        public IHttpActionResult UpdateCompany(Company company)
        {
            _memberService.SaveChanges(new[] { company });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/b2b/companyMembers
        [HttpPost]
        [Route("companyMembers")]
        [ResponseType(typeof(GenericSearchResult<CompanyMember>))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult GetCompanyMembers(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            GenericSearchResult<Member> members = _memberSearchService.SearchMembers(criteria);
            var ids = members.Results.Select(m => m.Id).ToArray();
            members.Results = _memberService.GetByIds(ids);
            return Ok(members);
        }

        // GET: api/b2b/companyMember/{id}
        [HttpGet]
        [Route("companyMember/{id}")]
        [ResponseType(typeof(CompanyMember))]
        public IHttpActionResult GetCompanyMemberById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).Cast<CompanyMember>().FirstOrDefault();
            CheckCurrentUserHasPermissionForUser(B2BPredefinedPermissions.CompanyMembers, retVal?.SecurityAccounts.Select(x => x.UserName));
            if (retVal != null)
            {
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        // GET: api/b2b/users/{userName}
        [HttpGet]
        [Route("users/{userName}")]
        [ResponseType(typeof(ApplicationUserExtended))]
        public async Task<IHttpActionResult> GetUserByNameAsync(string userName)
        {
            CheckCurrentUserHasPermissionForUser(B2BPredefinedPermissions.CompanyMembers, new [] { userName });
            var retVal = await _securityService.FindByNameAsync(userName, UserDetails.Full);
            return Ok(retVal);
        }

        // POST: api/b2b/companyMember
        [HttpPost]
        [Route("companyMember")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateMember(CompanyMember companyMember)
        {
            CheckCurrentUserHasPermissionForUser(B2BPredefinedPermissions.CompanyMembers, companyMember?.SecurityAccounts.Select(x => x.UserName));
            _memberService.SaveChanges(new[] { companyMember });
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [Route("users")]
        [ResponseType(typeof(SecurityResult))]
        public async Task<IHttpActionResult> UpdateUserAsync(ApplicationUserExtended user)
        {
            if (_securityOptions != null && _securityOptions.NonEditableUsers != null)
            {
                if (_securityOptions.NonEditableUsers.Contains(user.UserName))
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, "It is forbidden to edit this user.");
                }
            }
            if (user != null)
            {
                user.PasswordHash = null;
                user.SecurityStamp = null;
            }
            var result = await _securityService.UpdateAsync(user);
            return ProcessSecurityResult(result);
        }

        // DELETE: api/b2b/companyMembers
        [HttpDelete]
        [Route("companyMembers")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _memberService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/b2b/roles
        [HttpGet]
        [Route("roles")]
        [ResponseType(typeof(Role[]))]
        public async Task<IHttpActionResult> GetRoles()
        {
            var currentUser = await _securityService.FindByNameAsync(User.Identity.Name, UserDetails.Reduced);
            var currentMember = _memberService.GetByIds(new[] {currentUser.MemberId}).Cast<CompanyMember>().FirstOrDefault();
            if (currentMember != null)
            {
                var result = _roleService.SearchRoles(new RoleSearchRequest()).Roles
                    .Where(x => x.Name == Constants.ModuleAdminRole || x.Name == Constants.ModuleManagerRole ||
                                x.Name == Constants.ModuleEmployeeRole);
                return Ok(result);
            }
            return Unauthorized();
        }

        private IHttpActionResult ProcessSecurityResult(SecurityResult securityResult)
        {
            IHttpActionResult result;

            if (securityResult == null)
            {
                result = BadRequest();
            }
            else
            {
                if (!securityResult.Succeeded)
                    result = BadRequest(securityResult.Errors != null ? string.Join(" ", securityResult.Errors) : "Unknown error.");
                else
                    result = Ok(securityResult);
            }

            return result;
        }
    }
}
