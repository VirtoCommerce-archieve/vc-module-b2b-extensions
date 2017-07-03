using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model
{
    public class CompanyMemberDataEntity : EmployeeDataEntity
    {
        public string Title { get; set; }

        public override Member ToModel(Member member)
        {
            return base.ToModel(member);
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberDataEntity memberEntity)
        {
            var target = memberEntity as CompanyMemberDataEntity;
            if (target != null)
            {
                target.Title = this.Title;
            }
            base.Patch(memberEntity);
        }
    }
}