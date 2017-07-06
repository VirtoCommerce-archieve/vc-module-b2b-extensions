using System;
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
            _container.RegisterType<IMemberSearchService, CorporateMembersServiceImpl>();
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

            // Corporate administrator
            var role = roleManagementService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleAdminRole }).Roles.FirstOrDefault();
            if (role == null)
            {
                role = new Role
                {
                    Name = Constants.ModuleAdminRole,
                    Description = Constants.ModuleAdminRoleDescription
                };
            }

            // All available permissions
            var allPermissions = securityService.GetAllPermissions();

            // Add security:call_api permissions, because B2B users is customers (and have no access to admin site), but must have access to platform api
            // Add platform:security:read and platform:security:update permissions to read and assign specified role to B2B user
            var callApiPermission = allPermissions.Where(p => p.Id == PredefinedPermissions.SecurityCallApi).ToArray();
            var rolePermissions = allPermissions.Where(p => new[] {PredefinedPermissions.SecurityQuery, PredefinedPermissions.SecurityUpdate}.Contains(p.Id)).ToArray();

            // Corporate administrator: security:call_api + all available B2B permissions + platform:security:read and platform:security:update
            role.Permissions = callApiPermission.Concat(allPermissions.Where(p => p.ModuleId == ModuleInfo.Id).Concat(rolePermissions)).ToArray();

            roleManagementService.AddOrUpdateRole(role);

            // Corporate manager
            role = roleManagementService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleManagerRole }).Roles.FirstOrDefault();
            if (role == null)
            {
                role = new Role
                {
                    Name = Constants.ModuleManagerRole,
                    Description = Constants.ModuleManagerRoleDescription
                };
            }

            // Corporate anager: security:call_api + B2B company & company members edit permissions + platform:security:read and platform:security:update
            var managerPermissions = new[] { B2BPredefinedPermissions.CompanyInfo, B2BPredefinedPermissions.CompanyMembers };
            role.Permissions = callApiPermission.Concat(allPermissions.Where(p => managerPermissions.Contains(p.Id)).Concat(rolePermissions)).ToArray();

            roleManagementService.AddOrUpdateRole(role);

            // Employee
            role = roleManagementService.SearchRoles(new RoleSearchRequest { Keyword = Constants.ModuleEmployeeRole }).Roles.FirstOrDefault();
            if (role == null)
            {
                role = new Role
                {
                    Name = Constants.ModuleEmployeeRole,
                    Description = Constants.ModuleEmployeeRoleDescription
                };
            }

            // Employee: security:call_api permission only
            role.Permissions = callApiPermission.ToArray();

            roleManagementService.AddOrUpdateRole(role);
        }
    }
}