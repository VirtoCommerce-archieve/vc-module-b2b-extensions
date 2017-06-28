using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateRegisterController : ApiController
    {
        private readonly ISecurityService _securityService;
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IRoleManagementService _roleService;

        public CorporateRegisterController(ISecurityService securityService, IMemberService memberService, IMemberSearchService memberSearchService, IRoleManagementService roleService)
        {
            _securityService = securityService;
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _roleService = roleService;
        }

        // POST: api/b2b/register
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(Register registerData)
        {
            if (!registerData.IsValid())
            {
                return BadRequest();
            }

            //Check same company exist
            var searchRequest = new MembersSearchCriteria {
                Keyword = registerData.CompanyName,
                MemberType = typeof(Company).Name
            };
            var companySearchResult = _memberSearchService.SearchMembers(searchRequest);

            if (companySearchResult.TotalCount > 0)
            {
                return Ok(new { Message = "Company with same name already exist" });
            }

            var corporateAdminRole = _roleService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleAdminRole }).Roles.First();
            var user = new ApplicationUserExtended
            {
                Email = registerData.Email,
                Password = registerData.Password,
                UserName = registerData.UserName,
                UserType = AccountType.Administrator.ToString(),
                UserState = AccountState.Approved,
                StoreId = registerData.StoreId,
                Roles = new[] { corporateAdminRole }
            };

            //Register user in VC Platform (create security account)
            var result = await _securityService.CreateAsync(user);

            if (result.Succeeded == true)
            {
                //Load newly created account from API
                var storefrontUser = await _securityService.FindByNameAsync(user.UserName, UserDetails.Reduced);

                //Create new company
                var company = new Company
                {
                    Name = registerData.CompanyName
                };
                _memberService.SaveChanges(new[] { company });

                string fullName = string.Format("{0} {1}", registerData.FirstName, registerData.LastName);
                var member = new CompanyMember
                {
                    Id = storefrontUser.Id,
                    Name = fullName,
                    FullName = fullName,
                    FirstName = registerData.FirstName,
                    LastName = registerData.LastName,
                    Emails = new[] { registerData.Email },
                    IsActive = true,
                    Organizations = new List<string>() { company.Id }
                };
                _memberService.SaveChanges(new[] { member });
            }
            else
            {
                return BadRequest(result.Errors.First());
            }

            return Ok();
        }
    }
}