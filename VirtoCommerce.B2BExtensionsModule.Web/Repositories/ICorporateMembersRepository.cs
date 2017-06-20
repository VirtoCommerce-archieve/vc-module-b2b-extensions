using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;

namespace VirtoCommerce.B2BExtensionsModule.Web.Repositories
{
    public interface ICorporateMembersRepository: ICustomerRepository
    {
        IQueryable<CompanyDataEntity> Companies { get; }
        IQueryable<CompanyMemberDataEntity> CompanyMembers { get; }
        IQueryable<DepartmentDataEntity> Departments { get; }

        CompanyDataEntity GetCompanyById(string id);
        CompanyMemberDataEntity GetCompanyMemberById(string id);
    }
}
