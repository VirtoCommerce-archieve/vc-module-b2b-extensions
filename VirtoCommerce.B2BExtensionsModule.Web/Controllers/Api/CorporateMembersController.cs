using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;

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
        public IHttpActionResult GetCompanyById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        // POST: api/b2b/company
        [HttpPost]
        [Route("company")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateCompany(Company company)
        {
            _memberService.SaveChanges(new[] { company });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/b2b/companyMembers
        [HttpPost]
        [Route("companyMembers")]
        [ResponseType(typeof(GenericSearchResult<CompanyMember>))]
        public IHttpActionResult GetCompanyMembers(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            var result = _memberSearchService.SearchMembers(criteria);
            return Ok(result);
        }

        // GET: api/b2b/companyMember/{id}
        [HttpGet]
        [Route("companyMember/{id}")]
        [ResponseType(typeof(CompanyMember))]
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
        public IHttpActionResult UpdateMember(CompanyMember companyMember)
        {
            _memberService.SaveChanges(new[] { companyMember });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/b2b/companyMembers
        [HttpDelete]
        [Route("companyMembers")]
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _memberService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
