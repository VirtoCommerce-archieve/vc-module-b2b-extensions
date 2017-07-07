using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions
{
    public static class MemberExtensions
    {
        public static void ProcessSecurityAccounts(this ICollection<Member> members, ISecurityService securityService)
        {
            Parallel.ForEach(members, new ParallelOptions { MaxDegreeOfParallelism = 10 }, member =>
            {
                //Fully load security accounts for members which support them 
                var hasSecurityAccounts = member as IHasSecurityAccounts;
                if (hasSecurityAccounts != null)
                {
                    //Fully load all security accounts associated with this contact
                    foreach (var account in hasSecurityAccounts.SecurityAccounts.ToArray())
                    {
                        var result = Task.Run(() => securityService.FindByIdAsync(account.Id, UserDetails.Full));
                        hasSecurityAccounts.SecurityAccounts.Remove(account);
                        hasSecurityAccounts.SecurityAccounts.Add(result.Result);
                    }
                }
            });
        }
    }
}