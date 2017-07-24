using VirtoCommerce.Domain.Customer.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Search
{
    public class CorporateMembersSearchCriteria : MembersSearchCriteria
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}