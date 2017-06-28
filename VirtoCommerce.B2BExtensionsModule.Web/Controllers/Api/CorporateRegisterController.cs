using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
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
        private readonly ISecurityService _securityService;

        public CorporateRegisterController(IStoreService storeService, IMemberService memberService, ISecurityService securityService, INotificationManager notificationManager)
        {
            _storeService = storeService;
            _memberService = memberService;
            _securityService = securityService;
            _notificationManager = notificationManager;
        }

        // POST: api/b2b/register
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(Register registerData)
        {
            await CreateAsync(registerData, null);
            return Ok();
        }

        [HttpPost]
        [Route("register/{invite}")]
        [AllowAnonymous]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> RegisterByInvite([FromBody] Register registerData, string invite)
        {
            var member = _memberService.GetByIds(new[] { invite }).Cast<CompanyMember>().First();
            await CreateAsync(registerData, member);
            return Ok();
        }

        private async Task CreateAsync(Register registerData, CompanyMember member)
        {
            var user = new ApplicationUserExtended
            {
                Email = member?.Emails.FirstOrDefault() ?? registerData.Email,
                Password = registerData.Password,
                UserName = registerData.UserName,
                UserType = "Customer",
                StoreId = registerData.StoreId,
            };

            //Register user in VC Platform (create security account)
            var result = await _securityService.CreateAsync(user);

            if (result.Succeeded)
            {
                //Load newly created account from API
                var storefrontUser = await _securityService.FindByNameAsync(user.UserName, UserDetails.Reduced);
                
                if (member == null)
                {
                    var company = new Company
                    {
                        Name = registerData.CompanyName
                    };
                    _memberService.SaveChanges(new[] { company });

                    member = new CompanyMember
                    {
                        Id = storefrontUser.Id,
                        Name = $"{registerData.FirstName} {registerData.LastName}",
                        FullName = $"{registerData.FirstName} {registerData.LastName}",
                        FirstName = registerData.FirstName,
                        LastName = registerData.LastName,
                        IsActive = true,
                        Organizations = new List<string> { company.Id }
                    };
                }
                else
                {
                    member.Name = $"{registerData.FirstName} {registerData.LastName}",
                    member.FullName = $"{registerData.FirstName} {registerData.LastName}";
                    member.FirstName = registerData.FirstName;
                    member.LastName = registerData.LastName;
                    member.IsActive = true;
                }

                _memberService.SaveChanges(new[] { member });
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }
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
    }
}