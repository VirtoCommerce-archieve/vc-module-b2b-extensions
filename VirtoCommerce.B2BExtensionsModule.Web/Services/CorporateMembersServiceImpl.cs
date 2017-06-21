using System;
using System.Linq;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Repositories;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.B2BExtensionsModule.Web.Services
{
    public class CorporateMembersServiceImpl : ServiceBase, ICorporateMembersService
    {
        private readonly Func<ICorporateMembersRepository> _cortorateRepositoryFactory;
        private readonly IDynamicPropertyService _dynamicPropertyService;

        public CorporateMembersServiceImpl(Func<ICorporateMembersRepository> cortorateRepositoryFactory, IDynamicPropertyService dynamicPropertyService)
        {
            _cortorateRepositoryFactory = cortorateRepositoryFactory;
            _dynamicPropertyService = dynamicPropertyService;
        }

        public Company GetCompanyById(string id)
        {
            Company retVal = null;

            using (var repository = _cortorateRepositoryFactory())
            {
                var company = repository.GetCompanyById(id);

                retVal = AbstractTypeFactory<Company>.TryCreateInstance();
                if (company != null)
                {
                    company.ToModel(retVal);
                }
            }

            return retVal;
        }

        public GenericSearchResult<CompanyMember> GetCompanyMembers(MembersSearchCriteria criteria)
        {
            var retVal = new GenericSearchResult<CompanyMember>();

            using (var repository = _cortorateRepositoryFactory())
            {
                var query = LinqKit.Extensions.AsExpandable(repository.CompanyMembers);

                if (criteria.MemberId != null)
                {
                    query = query.Where(m => m.MemberRelations.Any(r => r.AncestorId == criteria.MemberId));
                }
                else
                {
                    if (!criteria.DeepSearch)
                    {
                        query = query.Where(m => !m.MemberRelations.Any());
                    }
                }

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<CompanyMember>(m => m.LastName), SortDirection = SortDirection.Descending } };
                }

                query = query.OrderBySortInfos(sortInfos);

                retVal.TotalCount = query.Count();

                retVal.Results = query.Skip(criteria.Skip).Take(criteria.Take).ToArray()
                                      .Select(m => m.ToModel(AbstractTypeFactory<CompanyMember>.TryCreateInstance())).Cast<CompanyMember>().ToList();
                return retVal;
            }
        }

        public CompanyMember GetCompanyMemberById(string id)
        {
            CompanyMember retVal = null;

            using (var repository = _cortorateRepositoryFactory())
            {
                var companyMember = repository.GetCompanyMemberById(id);

                retVal = AbstractTypeFactory<CompanyMember>.TryCreateInstance();
                if (companyMember != null)
                {
                    companyMember.ToModel(retVal);
                }
            }

            return retVal;
        }

        public virtual void SaveChanges(Member[] members)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = _cortorateRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var existingMemberEntities = repository.GetMembersByIds(members.Where(m => !m.IsTransient()).Select(m => m.Id).ToArray());

                foreach (var member in members)
                {
                    var memberEntityType = AbstractTypeFactory<Member>.AllTypeInfos.Where(t => t.MappedType != null && t.IsAssignableTo(member.MemberType)).Select(t => t.MappedType).FirstOrDefault();
                    if (memberEntityType != null)
                    {
                        var dataSourceMember = AbstractTypeFactory<MemberDataEntity>.TryCreateInstance(memberEntityType.Name);
                        if (dataSourceMember != null)
                        {
                            dataSourceMember.FromModel(member, pkMap);

                            var dataTargetMember = existingMemberEntities.FirstOrDefault(m => m.Id == member.Id);
                            if (dataTargetMember != null)
                            {
                                changeTracker.Attach(dataTargetMember);
                                dataSourceMember.Patch(dataTargetMember);
                            }
                            else
                            {
                                repository.Add(dataSourceMember);
                            }
                        }
                    }
                }

                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }

            //Save dynamic properties
            foreach (var member in members)
            {
                _dynamicPropertyService.SaveDynamicPropertyValues(member);
            }
        }

        public virtual void RemoveCorporateMembersByIds(string[] ids)
        {
            using (var repository = _cortorateRepositoryFactory()) {
                repository.RemoveMembersByIds(ids);
                CommitChanges(repository);
            }
        }
    }
}