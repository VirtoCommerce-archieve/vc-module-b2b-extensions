using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Security;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateMembersController : ApiController
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;

        public CorporateMembersController(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
        }

        // GET: api/b2b/company/{id}
        [HttpGet]
        [Route("company/{id}")]
        [ResponseType(typeof(Company))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyInfo)]
        public IHttpActionResult GetCompanyById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        // GET: api/b2b/company/{id}
        [HttpGet]
        [Route("companyByCustomerId/{id}")]
        [ResponseType(typeof(Company))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyInfo)]
        public IHttpActionResult GetCompanyByCustomerId(string id)
        {
            var member = _memberService.GetByIds(new[] { id }).Cast<CompanyMember>().FirstOrDefault();
            if (member != null && member.Organizations.Any()) {
                var companyId = member.Organizations.FirstOrDefault();
                return GetCompanyById(companyId);
            }
            return NotFound();
        }

        // POST: api/b2b/company
        [HttpPost]
        [Route("company")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyInfo)]
        public IHttpActionResult UpdateCompany(Company company)
        {
            _memberService.SaveChanges(new[] { company });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/b2b/companyMembers
        [HttpPost]
        [Route("companyMembers")]
        [ResponseType(typeof(GenericSearchResult<CompanyMember>))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult GetCompanyMembers(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            GenericSearchResult<Member> members = _memberSearchService.SearchMembers(criteria);
            var ids = members.Results.Select(m => m.Id).ToArray();
            members.Results = _memberService.GetByIds(ids);
            return Ok(members);
        }

        // GET: api/b2b/companyMember/{id}
        [HttpGet]
        [Route("companyMember/{id}")]
        [ResponseType(typeof(CompanyMember))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult GetCompanyMemberById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        // POST: api/b2b/companyMember
        [HttpPost]
        [Route("companyMember")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult UpdateMember(CompanyMember companyMember)
        {
            _memberService.SaveChanges(new[] { companyMember });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/b2b/companyMembers
        [HttpDelete]
        [Route("companyMembers")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = B2BPredefinedPermissions.CompanyMembers)]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _memberService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
