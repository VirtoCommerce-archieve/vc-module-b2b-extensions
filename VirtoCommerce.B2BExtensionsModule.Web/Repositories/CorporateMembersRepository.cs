using System.Data.Entity;
using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public class CorporateMembersRepository : CustomerRepositoryImpl
    {
        public CorporateMembersRepository()
        {
        }

        public CorporateMembersRepository(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, interceptors)
        {
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyDataEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompanyDataEntity>().ToTable("Company");

            modelBuilder.Entity<CompanyMemberDataEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompanyMemberDataEntity>().ToTable("CompanyMember");

            modelBuilder.Entity<DepartmentDataEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<DepartmentDataEntity>().ToTable("Department");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<CompanyDataEntity> Companies
        {
            get { return GetAsQueryable<CompanyDataEntity>(); }
        }

        public IQueryable<CompanyMemberDataEntity> CompanyMembers
        {
            get { return GetAsQueryable<CompanyMemberDataEntity>(); }
        }

        public IQueryable<DepartmentDataEntity> Departments
        {
            get { return GetAsQueryable<DepartmentDataEntity>(); }
        }
    }
}