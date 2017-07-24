using FluentValidation.Attributes;
using VirtoCommerce.B2BExtensionsModule.Web.Services.Validation;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    [Validator(typeof(InviteValidator))]
    public class Invite
    {
        public string Value { get; set; }
    }
}