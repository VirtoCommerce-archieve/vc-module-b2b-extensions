using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
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

        public CorporateRegisterController(ISecurityService securityService, IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _securityService = securityService;
            _memberService = memberService;
            _memberSearchService = memberSearchService;
        }

        // POST: api/b2b/register
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(Register registerData)
        {
            await Task.Delay(5000);
            return Ok();
            var user = new ApplicationUserExtended
            {
                Email = registerData.Email,
                Password = registerData.Password,
                UserName = registerData.UserName,
                UserType = "Administrator",
                StoreId = registerData.StoreId,
            };

            //Register user in VC Platform (create security account)
            var result = await _securityService.CreateAsync(user);

            if (result.Succeeded == true)
            {
                //Load newly created account from API
                var storefrontUser = await _securityService.FindByNameAsync(user.UserName, UserDetails.Reduced);

                var company = new Company
                {
                    Name = registerData.CompanyName
                };
                _memberService.SaveChanges(new[] { company });
                //var retVal = _memberService.GetByIds(new[] { company.Id }).FirstOrDefault();

                var member = new CompanyMember
                {
                    Id = storefrontUser.Id,

                    //UserId = storefrontUser.Id,
                    //UserName = storefrontUser.UserName,
                    //IsRegisteredUser = true,

                    Name = registerData.FirstName + registerData.LastName,
                    FullName = registerData.FirstName + registerData.LastName,
                    FirstName = registerData.FirstName,
                    LastName = registerData.LastName,
                    IsActive = true
                };

                member.Organizations = new List<string>();
                member.Organizations.Add(company.Id);

                _memberService.SaveChanges(new[] { member });
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }

            return Ok();
        }
    }
}