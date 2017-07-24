using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateRegisterController : CorporateControllerBase
    {
        private readonly INotificationManager _notificationManager;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly IRoleManagementService _roleService;
        private readonly ISecurityService _securityService;

        private readonly IValidator<CompanyOwnerRegistrationData> _companyOwnerRegistrationDataValidator;
        private readonly IValidator<CompanyMemberRegistrationData> _companyMemberRegistrationDataValidator;
        private readonly IValidator<CompanyMemberRegistrationByInviteData> _companyMemberRegistrationByInviteDataValidator;

        private readonly IValidator<Invite> _inviteValidator;
        private readonly IValidator<InviteData> _inviteDataValidator;

        [CLSCompliant(false)]
        public CorporateRegisterController(INotificationManager notificationManager, IStoreService storeService,
            IMemberService memberService, IRoleManagementService roleService, ISecurityService securityService,
            IValidatorFactory validatorFactory) : base(securityService)
        {
            _notificationManager = notificationManager;
            _storeService = storeService;
            _memberService = memberService;
            _roleService = roleService;
            _securityService = securityService;

            _companyOwnerRegistrationDataValidator = validatorFactory.GetValidator<CompanyOwnerRegistrationData>();
            _companyMemberRegistrationDataValidator = validatorFactory.GetValidator<CompanyMemberRegistrationData>();
            _companyMemberRegistrationByInviteDataValidator = validatorFactory.GetValidator<CompanyMemberRegistrationByInviteData>();

            _inviteValidator = validatorFactory.GetValidator<Invite>();
            _inviteDataValidator = validatorFactory.GetValidator<InviteData>();
        }

        // POST: api/b2b/register
        /// <summary>
        /// Register company owner
        /// </summary>
        /// <param name="registrationData">Company owner data</param>
        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> RegisterCompanOwner(CompanyOwnerRegistrationData registrationData)
        {
            Company company = null;
            return await RegisterAsync(_companyOwnerRegistrationDataValidator, registrationData, Constants.ModuleAdminRole, null, () =>
                {
                    company = new Company
                    {
                        Name = registrationData.CompanyName
                    };
                    _memberService.SaveChanges(new[] { company });
                },
                user => registrationData.ToCompanyMember(new CompanyMember(), user.Id),
                member => member.Organizations = new List<string> { company.Id });
        }

        // POST: api/b2b/registerMember
        /// <summary>
        /// Register company member manually
        /// </summary>
        /// <param name="registrationData">Company member data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("registerMember")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public async Task<IHttpActionResult> RegisterCompanyMember(CompanyMemberRegistrationData registrationData)
        {
            return await RegisterAsync(_companyMemberRegistrationDataValidator, registrationData, registrationData.Role, null,
                null, user => registrationData.ToCompanyMember(new CompanyMember(), user.Id), null);
        }

        // GET: api/b2b/registerMember/:invite
        /// <summary>
        /// Get company member data from invite
        /// </summary>
        /// <param name="invite">Unique invite identifier from invitation url</param>
        [HttpGet]
        [AllowAnonymous]
        [Route("registerMember/{invite}")]
        [ResponseType(typeof(CompanyMemberRegistrationByInviteData))]
        public IHttpActionResult GetRegistrationDataByInvite(string invite)
        {
            var validationResult = _inviteValidator.Validate(new Invite { Value = invite });
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.BuildModelState());

            var member = _memberService.GetByIds(new[] { invite }).Cast<CompanyMember>().First();
            var company = _memberService.GetByIds(new[] { member.Organizations.First() }).Cast<Company>().First();
            return Ok(new CompanyMemberRegistrationByInviteData
            {
                CompanyName = company?.Name,
                Email = member.Emails.First()
            });
        }

        // POST: /api/b2b/registerMember/:invite
        /// <summary>
        /// Register company member by invite
        /// </summary>
        /// <param name="registrationData">Company member data</param
        /// <param name="invite">Unique invite identifier from invitation url</param>
        [HttpPost]
        [AllowAnonymous]
        [Route("registerMember/{invite}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> RegisterCompanyMemberByInvite([FromBody] CompanyMemberRegistrationByInviteData registrationData, string invite)
        {
            var validationResult = _inviteValidator.Validate(new Invite { Value = invite });
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.BuildModelState());

            var member = _memberService.GetByIds(new[] { invite }).Cast<CompanyMember>().First();
            return await RegisterAsync(_companyMemberRegistrationByInviteDataValidator, registrationData, Constants.ModuleEmployeeRole,
                user => user.MemberId = member.Id,
                null, user => registrationData.ToCompanyMember(member, member.Id), null);
        }

        // POST: /api/b2b/invite
        /// <summary>
        /// Create invite for specified emails
        /// </summary>
        /// <param name="inviteData">Data used to send invitation link to invited user</param>
        [HttpPost]
        [Route("invite")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult Invite(InviteData inviteData)
        {
            var validationResult = _inviteDataValidator.Validate(inviteData);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.BuildModelState());

            var store = _storeService.GetById(inviteData.StoreId);
            var company = _memberService.GetByIds(new[] { inviteData.CompanyId }).First();

            inviteData.Emails.ProcessWithPaging(50, (currentEmails, currentCount, totalCount) =>
            {
                var companyMembers = currentEmails.Select(email => new CompanyMember
                {
                    FullName = email,
                    Emails = new[] { email },
                    Organizations = new[] { inviteData.CompanyId },
                    IsActive = false
                }).ToArray();
                _memberService.SaveChanges(companyMembers.Cast<Member>().ToArray());

                foreach (var companyMember in companyMembers)
                {
                    var token = companyMember.Id;

                    var uriBuilder = new UriBuilder(inviteData.CallbackUrl);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["invite"] = token;
                    uriBuilder.Query = query.ToString();

                    var notification = _notificationManager.GetNewNotification<CorporateInviteEmailNotification>(inviteData.StoreId, "Store", inviteData.Language);
                    notification.Url = uriBuilder.ToString();
                    notification.CompanyName = company.Name;
                    notification.Message = inviteData.Message;

                    notification.StoreName = store.Name;
                    notification.Sender = store.Email;
                    notification.IsActive = true;

                    notification.AdminName = inviteData.AdminName;
                    notification.AdminEmail = inviteData.AdminEmail;

                    notification.Recipient = companyMember.Emails.Single();

                    _notificationManager.ScheduleSendNotification(notification);
                }
            });

            return StatusCode(HttpStatusCode.NoContent);
        }

        private async Task<IHttpActionResult> RegisterAsync<T>(IValidator<T> validator, T registrationData, string roleName, Action<ApplicationUserExtended> prepareSecurity, Action prepare,
            Func<ApplicationUserExtended, CompanyMember> build, Action<CompanyMember> complete)
            where T : RegistrationDataBase
        {
            var validationResult = validator.Validate(registrationData);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.BuildModelState());

            var user = registrationData.ToApplicationUserExtended();

            var role = _roleService.SearchRoles(new RoleSearchRequest { Keyword = roleName }).Roles.FirstOrDefault() ??
                       _roleService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleEmployeeRole }).Roles.First();
            user.Roles = new[] { role };

            prepareSecurity?.Invoke(user);

            var result = await _securityService.CreateAsync(user);

            if (result.Succeeded)
            {
                prepare?.Invoke();
                var member = build.Invoke(user);
                complete?.Invoke(member);

                try
                {
                    _memberService.SaveChanges(new Member[] { member });
                }
                catch (ValidationException exception)
                {
                    return BadRequest(exception.Errors.BuildModelState());
                }
            }
            else
            {
                return BadRequest(result.Errors.BuildErrorMessage());
            }

            return Ok();
        }
    }
}