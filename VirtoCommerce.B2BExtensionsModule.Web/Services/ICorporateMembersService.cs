using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public interface ICorporateMembersService
    {
        Company GetCompanyById(string id);

        GenericSearchResult<CompanyMember> GetCompanyMembers(MembersSearchCriteria criteria);
        CompanyMember GetCompanyMemberById(string id);

        void SaveChanges(Member[] members);
        void RemoveCorporateMembersByIds(string[] ids);
    }
}
