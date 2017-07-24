using System;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Extensions;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Repositories;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateMembersServiceImpl : CommerceMembersServiceImpl
    {
        private readonly ISecurityService _securityService;
        private readonly Lazy<IValidator<CompanyMember>> _companyMemberValidator;

        [CLSCompliant(false)]
        public CorporateMembersServiceImpl(Func<ICorporateMembersRepository> repositoryFactory, ICommerceService commerceService,
            IDynamicPropertyService dynamicPropertyService, ISecurityService securityService,
            IEventPublisher<MemberChangingEvent> eventPublisher, IValidatorFactory validatorFactory)
            : base((Func<ICustomerRepository>)repositoryFactory, dynamicPropertyService, commerceService, securityService, eventPublisher)
        {
            _securityService = securityService;
            _companyMemberValidator = new Lazy<IValidator<CompanyMember>>(validatorFactory.GetValidator<CompanyMember>);
        }

        public override GenericSearchResult<Member> SearchMembers(MembersSearchCriteria criteria)
        {
            var retVal = base.SearchMembers(criteria);
            retVal.Results.ProcessSecurityAccounts(_securityService);
            return retVal;
        }

        public override Member[] GetByIds(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var retVal = base.GetByIds(memberIds, responseGroup, memberTypes);
            retVal.ProcessSecurityAccounts(_securityService);
            return retVal;
        }

        #region Overrides of MemberServiceBase

        public override void SaveChanges(Member[] members)
        {
            var companyMembers = members.OfType<CompanyMember>().ToArray();
            foreach (var companyMember in companyMembers)
            {
                _companyMemberValidator.Value.ValidateAndThrow(companyMember);
            }
            base.SaveChanges(members);
        }

        #endregion

        protected override Expression<Func<MemberDataEntity, bool>> GetQueryPredicate(MembersSearchCriteria criteria)
        {
            var retVal = base.GetQueryPredicate(criteria);

            var corporateCriteria = criteria as CorporateMembersSearchCriteria;
            if (corporateCriteria != null)
            {
                var predicate = PredicateBuilder.True<MemberDataEntity>();

                if (!string.IsNullOrEmpty(corporateCriteria.Name))
                {
                    predicate = predicate.And(x => x.Name == corporateCriteria.Name);
                }

                if (!string.IsNullOrEmpty(corporateCriteria.Email))
                {
                    predicate = predicate.And(x => x.Emails.Any(e => e.Address == corporateCriteria.Email));
                }

                //Should use Expand() to all predicates to prevent EF error
                //http://stackoverflow.com/questions/2947820/c-sharp-predicatebuilder-entities-the-parameter-f-was-not-bound-in-the-specif?rq=1
                retVal = LinqKit.Extensions.Expand(retVal.And(LinqKit.Extensions.Expand(predicate)));
            }

            return retVal;
        }
    }
}