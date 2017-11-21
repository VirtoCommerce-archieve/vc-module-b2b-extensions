using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model
{
    public class CompanyMemberDataEntity : ContactDataEntity
    {
        [StringLength(128)]
        public string Title { get; set; } 
        
        public bool IsActive { get; set; }

        public override void Patch(MemberDataEntity memberEntity)
        {
            var target = memberEntity as CompanyMemberDataEntity;
            
            target.IsActive = this.IsActive;
            target.Title = this.Title;

            base.Patch(target);
        }

        public override Member ToModel(Member member)
        {
            return base.ToModel(member);
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            return base.FromModel(member, pkMap);
        }        
    }
}