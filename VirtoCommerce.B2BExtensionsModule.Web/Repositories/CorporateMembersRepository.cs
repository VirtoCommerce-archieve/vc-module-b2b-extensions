using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public class CorporateMembersRepository : CustomerRepositoryImpl, ICorporateMembersRepository
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
            modelBuilder.Entity<CompanyDataEntity>().Property(x => x.Name).HasColumnAnnotation("Name", new IndexAnnotation(new IndexAttribute { IsUnique = true }));
            modelBuilder.Entity<CompanyDataEntity>().ToTable("Company");

            modelBuilder.Entity<DepartmentDataEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompanyMemberDataEntity>().Property(x => x.Name).HasColumnAnnotation("Name", new IndexAnnotation(new IndexAttribute { IsUnique = true }));
            modelBuilder.Entity<DepartmentDataEntity>().ToTable("Department");

            modelBuilder.Entity<CompanyMemberDataEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<CompanyMemberDataEntity>().ToTable("CompanyMember");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<CompanyDataEntity> Companies => GetAsQueryable<CompanyDataEntity>();

        public IQueryable<CompanyMemberDataEntity> CompanyMembers => GetAsQueryable<CompanyMemberDataEntity>();

        public IQueryable<DepartmentDataEntity> Departments => GetAsQueryable<DepartmentDataEntity>();
    }
}