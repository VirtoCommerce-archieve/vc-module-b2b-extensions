using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateRegisterController : ApiController
    {
        private readonly INotificationManager _notificationManager;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IRoleManagementService _roleService;
        private readonly ISecurityService _securityService;

        public CorporateRegisterController(INotificationManager notificationManager,
            IStoreService storeService,
            IMemberService memberService,
            IMemberSearchService memberSearchService,
            IRoleManagementService roleService,
            ISecurityService securityService)
        {
            _notificationManager = notificationManager;
            _storeService = storeService;
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _roleService = roleService;
            _securityService = securityService;
        }

        // POST: api/b2b/register
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(Register registerData)
        {
            return await CreateAsync(registerData, null);
        }

        [HttpPost]
        [Route("register/{invite}")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> RegisterByInvite([FromBody] Register registerData, string invite)
        {
            var member = _memberService.GetByIds(new[] { invite }).Cast<CompanyMember>().First();
            return await CreateAsync(registerData, member);
        }

        [HttpPost]
        [Route("invite")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Invite(Invite invite)
        {
            if (invite == null || string.IsNullOrEmpty(invite.StoreId) || string.IsNullOrEmpty(invite.CompanyId) || invite.Emails.IsNullOrEmpty() ||
                string.IsNullOrEmpty(invite.AdminName) || string.IsNullOrEmpty(invite.AdminEmail) || string.IsNullOrEmpty(invite.CallbackUrl))
            {
                return BadRequest();
            }

            var store = _storeService.GetById(invite.StoreId);
            var company = _memberService.GetByIds(new[] { invite.CompanyId }).FirstOrDefault();
            if (store == null || company == null)
            {
                return BadRequest();
            }

            invite.Emails.ProcessWithPaging(50, (currentEmails, currentCount, totalCount) =>
            {
                var companyMembers = currentEmails.Select(email => new CompanyMember
                {
                    FullName = email,
                    Emails = new[] { email },
                    Organizations = new[] { invite.CompanyId },
                    IsActive = false
                }).ToArray();
                _memberService.SaveChanges(companyMembers.ToArray());

                foreach (var companyMember in companyMembers)
                {
                    var token = companyMember.Id;

                    var uriBuilder = new UriBuilder(invite.CallbackUrl);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["code"] = token;
                    uriBuilder.Query = query.ToString();

                    var notification = _notificationManager.GetNewNotification<CorporateInviteEmailNotification>(invite.StoreId, "Store", invite.Language);
                    notification.Url = uriBuilder.ToString();
                    notification.CompanyName = company.Name;
                    notification.Message = invite.Message;

                    notification.StoreName = store.Name;
                    notification.Sender = store.Email;
                    notification.IsActive = true;

                    notification.AdminName = invite.AdminName;
                    notification.AdminEmail = invite.AdminEmail;

                    notification.Recipient = companyMember.Emails.First();

                    _notificationManager.ScheduleSendNotification(notification);
                }
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

        private async Task<IHttpActionResult> CreateAsync(Register registerData, CompanyMember member)
        {
            if (!registerData.IsValid())
            {
                return BadRequest();
            }

            var user = new ApplicationUserExtended
            {
                Email = member?.Emails.FirstOrDefault() ?? registerData.Email,
                Password = registerData.Password,
                UserName = registerData.UserName,
                UserType = AccountType.Customer.ToString(),
                UserState = AccountState.Approved,
                StoreId = registerData.StoreId
            };

            if (member == null)
            {
                //Check same company exist
                var searchRequest = new MembersSearchCriteria
                {
                    Keyword = registerData.CompanyName,
                    MemberType = typeof(Company).Name
                };
                var companySearchResult = _memberSearchService.SearchMembers(searchRequest);
                if (companySearchResult.TotalCount > 0)
                {
                    return BadRequest("Company with same name already exist");
                }

                var corporateAdminRole = _roleService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleAdminRole }).Roles.First();
                user.Roles = new[] { corporateAdminRole };
            }

            //Register user in VC Platform (create security account)
            var result = await _securityService.CreateAsync(user);

            if (result.Succeeded)
            {
                if (member == null)
                {
                    var company = new Company
                    {
                        Name = registerData.CompanyName
                    };
                    _memberService.SaveChanges(new[] { company });

                    member = new CompanyMember
                    {
                        Id = user.Id,
                        Name = $"{registerData.FirstName} {registerData.LastName}",
                        FullName = $"{registerData.FirstName} {registerData.LastName}",
                        FirstName = registerData.FirstName,
                        LastName = registerData.LastName,
                        Emails = new[] { registerData.Email },
                        IsActive = true,
                        Organizations = new List<string> { company.Id }
                    };
                }
                else
                {
                    member.Name = $"{registerData.FirstName} {registerData.LastName}";
                    member.FullName = $"{registerData.FirstName} {registerData.LastName}";
                    member.FirstName = registerData.FirstName;
                    member.LastName = registerData.LastName;
                    member.IsActive = true;
                }

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