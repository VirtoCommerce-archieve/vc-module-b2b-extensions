using FluentValidation.Attributes;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    [Validator(typeof(InviteDataValidator))]
    public class InviteData
    {
        public string StoreId { get; set; }

        public string CompanyId { get; set; }

        public string[] Emails { get; set; }

        public string AdminName { get; set; }

        public string AdminEmail { get; set; }

        public string Message { get; set; }

        public string Language { get; set; }

        public string CallbackUrl { get; set; }
    }
}