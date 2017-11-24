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

        public override Member ToModel(Member member)
        {
            var retVal = (CompanyMember)base.ToModel(member);
            retVal.Title = Title;
            retVal.IsActive = IsActive;

            return retVal;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            base.FromModel(member, pkMap);

            var from = (CompanyMember)member;
            Title = from.Title;
            IsActive = from.IsActive;

            return this;
        }

        public override void Patch(MemberDataEntity entity)
        {
            base.Patch(entity);

            var target = (CompanyMemberDataEntity)entity;
            target.Title = Title;
            target.IsActive = IsActive;
        }
    }
}