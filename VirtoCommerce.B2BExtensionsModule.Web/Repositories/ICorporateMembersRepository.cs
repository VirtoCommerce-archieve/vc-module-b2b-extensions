using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public interface ICorporateMembersRepository
    {
        IQueryable<CompanyDataEntity> Companies { get; }

        IQueryable<CompanyMemberDataEntity> CompanyMembers { get; }

        IQueryable<DepartmentDataEntity> Departments { get; }
    }
}