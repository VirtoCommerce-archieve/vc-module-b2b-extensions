using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications;
using VirtoCommerce.B2BExtensionsModule.Web.Repositories;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.B2BExtensionsModule.Web
{
    public class Module : ModuleBase
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var db = new CorporateMembersRepository(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CorporateMembersRepository, Migrations.Configuration>();
                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            Func<CorporateMembersRepository> customerRepositoryFactory = () => new CorporateMembersRepository(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>());

            _container.RegisterInstance<Func<ICustomerRepository>>(customerRepositoryFactory);
            _container.RegisterInstance<Func<IMemberRepository>>(customerRepositoryFactory);

            _container.RegisterType<IMemberService, CorporateMembersServiceImpl>();
            _container.RegisterType<MemberSearchServiceDecorator, CorporateMemberSearchServiceDecorator>();
        }

        public override void PostInitialize()
        {
            AbstractTypeFactory<Member>.RegisterType<Company>().MapToType<CompanyDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<CompanyDataEntity>();

            AbstractTypeFactory<Member>.RegisterType<CompanyMember>().MapToType<CompanyMemberDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<CompanyMemberDataEntity>();

            AbstractTypeFactory<Member>.RegisterType<Department>().MapToType<DepartmentDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<DepartmentDataEntity>();

            base.PostInitialize();

            var notificationManager = _container.Resolve<INotificationManager>();
            notificationManager.RegisterNotificationType(() => new CorporateInviteEmailNotification(_container.Resolve<IEmailNotificationSendingGateway>())
            {
                DisplayName = "Company member invite notification",
                Description = "This notification sends to specified email when this email invited to register as company member.",
                NotificationTemplate = new NotificationTemplate
                {
                    Subject = B2BExtensionsResources.InviteEmailNotificationSubject,
                    Body = B2BExtensionsResources.InviteEmailNotificationBody,
                    Language = "en-US"
                }
            });

            InitializeSecurity();
        }

        #endregion

        private void InitializeSecurity()
        {
            var roleManagementService = _container.Resolve<IRoleManagementService>();
            var securityService = _container.Resolve<ISecurityService>();
            
            var allPermissions = securityService.GetAllPermissions();
            var managerPermissions = new[] { B2BPredefinedPermissions.CompanyInfo, B2BPredefinedPermissions.CompanyMembers };

            InitializeRole(roleManagementService, Constants.ModuleAdminRole, Constants.ModuleAdminRoleDescription, allPermissions.Where(p => p.ModuleId == ModuleInfo.Id));
            InitializeRole(roleManagementService, Constants.ModuleManagerRole, Constants.ModuleManagerRoleDescription, allPermissions.Where(p => managerPermissions.Contains(p.Id)));
            InitializeRole(roleManagementService, Constants.ModuleEmployeeRole, Constants.ModuleEmployeeRoleDescription, null);
        }

        private void InitializeRole(IRoleManagementService roleManagementService, string name, string description, IEnumerable<Permission> permissions)
        {
            // Corporate administrator
            var role = roleManagementService.SearchRoles(new RoleSearchRequest {Keyword = name}).Roles.FirstOrDefault() ?? new Role {Name = name, Description = description};

            // Add security:call_api permissions, because B2B users is customers (and have no access to admin site), but must have access to platform api
            var callApiPermission = PredefinedPermissions.Permissions.Where(p => p.Id == PredefinedPermissions.SecurityCallApi).ToArray();

            // Corporate administrator: security:call_api + permissions
            role.Permissions = callApiPermission.Concat(permissions ?? Enumerable.Empty<Permission>()).ToArray();

            roleManagementService.AddOrUpdateRole(role);
        }
    }
}