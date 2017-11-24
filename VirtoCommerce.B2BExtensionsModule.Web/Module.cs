using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Practices.Unity;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Repositories;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.B2BExtensionsModule.Web.Services;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;
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
        private const string ConnectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var db = new CorporateMembersRepository(ConnectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CorporateMembersRepository, Migrations.Configuration>();
                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Func<CorporateMembersRepository> customerRepositoryFactory = () => new CorporateMembersRepository(ConnectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>());

            _container.RegisterInstance<Func<ICorporateMembersRepository>>(customerRepositoryFactory);
            _container.RegisterInstance<Func<ICustomerRepository>>(customerRepositoryFactory);
            _container.RegisterInstance<Func<IMemberRepository>>(customerRepositoryFactory);

            _container.RegisterType<IMemberService, CorporateMembersServiceImpl>();
            _container.RegisterType<IMemberSearchService, MemberSearchServiceDecorator>();

            // https://www.codeproject.com/Articles/326647/FluentValidation-and-Unity
            // https://stackoverflow.com/questions/25185272/how-can-i-automatically-register-all-my-fluent-validators-with-unity
            _container.RegisterType<IValidatorFactory, ValidatorFactory>(new ContainerControlledLifetimeManager());
            var validators = AssemblyScanner.FindValidatorsInAssemblyContaining<InviteValidator>();
            validators.ForEach(validator => _container.RegisterType(validator.InterfaceType, validator.ValidatorType, new ContainerControlledLifetimeManager()));
        }

        public override void PostInitialize()
        {
            AbstractTypeFactory<Member>.OverrideType<Organization, Company>().MapToType<CompanyDataEntity>();
            AbstractTypeFactory<Member>.OverrideType<Contact, CompanyMember>().MapToType<CompanyMemberDataEntity>();

            AbstractTypeFactory<Member>.RegisterType<Department>().MapToType<DepartmentDataEntity>();
            AbstractTypeFactory<MemberDataEntity>.RegisterType<DepartmentDataEntity>();

            AbstractTypeFactory<MembersSearchCriteria>.RegisterType<CorporateMembersSearchCriteria>();

            base.PostInitialize();

            InitializeSecurity();
            InitializeNotifications();
        }

        #endregion

        private void InitializeNotifications()
        {

            var notificationManager = _container.Resolve<INotificationManager>();
            notificationManager.RegisterNotificationType(() => new CorporateInviteEmailNotification(_container.Resolve<IEmailNotificationSendingGateway>())
            {
                DisplayName = "Company member invite notification",
                Description = "This notification sends to specified email when this email invited to register as company member.",
                NotificationTemplate = new NotificationTemplate
                {
                    Subject = B2BCustomerResources.InviteEmailNotificationSubject,
                    Body = B2BCustomerResources.InviteEmailNotificationBody,
                    Language = "en-US"
                }
            });
            notificationManager.RegisterNotificationType(() => new CorporateProductEmailNotification(_container.Resolve<IEmailNotificationSendingGateway>())
            {
                DisplayName = "Product sending notification",
                Description = "This notification sends to specified email when user want to send specified product to this email.",
                NotificationTemplate = new NotificationTemplate
                {
                    Subject = B2BCustomerResources.ProductEmailNotificationSubject,
                    Body = B2BCustomerResources.ProductEmailNotificationBody,
                    Language = "en-US"
                }
            });
        }

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