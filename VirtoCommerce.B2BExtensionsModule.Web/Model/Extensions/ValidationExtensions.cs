using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Resources;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions
{
    [CLSCompliant(false)]
    public static class ValidationExtensions
    {
        public static void StoreExist<T>(this IRuleBuilderOptions<T, string> rule, IStoreService storeService)
        {
            rule.Must(x => storeService.GetById(x) != null)
                .WithMessage(string.Format(B2BCustomerResources.StoreDoesNotExist, string.Format(B2BCustomerResources.WithId, Constants.PropertyValue)));
        }

        public static void CompanyExist<T>(this IRuleBuilderOptions<T, string> rule, IMemberService memberService)
        {
            rule.Must(x => memberService.GetByIds(new [] { x }).Any())
                .WithMessage(string.Format(B2BCustomerResources.CompanyDoesNotExist, Constants.PropertyValue));
        }

        public static void UserDoesNotExist<T>(this IRuleBuilderOptions<T, string> rule, ISecurityService securityService)
        {
            rule.MustAsync((x, ct) => securityService.FindByNameAsync(x, UserDetails.Reduced).ContinueWith(result => result.Result == null, ct))
                .WithMessage(string.Format(B2BCustomerResources.UserAlreadyExist, Constants.PropertyValue));
        }

        public static ModelStateDictionary BuildModelState(this IEnumerable<ValidationFailure> errors)
        {
            var retVal = new ModelStateDictionary();
            foreach (var error in errors)
            {
                retVal.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return retVal;
        }

        public static string BuildErrorMessage(this IEnumerable<string> errors)
        {
            return string.Join("", errors.Select(x => Environment.NewLine + x).ToArray());
        }
    }
}