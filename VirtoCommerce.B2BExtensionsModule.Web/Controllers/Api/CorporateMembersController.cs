using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model;
using VirtoCommerce.B2BExtensionsModule.Web.Services;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateMembersController : ApiController
    {
        private readonly ICorporateMembersService _corporateMembersService;

        public CorporateMembersController(ICorporateMembersService corporateMembersService)
        {
            _corporateMembersService = corporateMembersService;
        }

        // GET: api/b2b/company/{id}
        [HttpGet]
        [Route("company/{id}")]
        [ResponseType(typeof(Company))]
        public IHttpActionResult GetCompanyById(string id)
        {
            var retVal = _corporateMembersService.GetCompanyById(id);
            return Ok(retVal);
        }

        // POST: api/b2b/company
        [HttpPost]
        [Route("company")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateCompany(Company company)
        {
            _corporateMembersService.SaveChanges(new[] { company });
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

            var result = _corporateMembersService.GetCompanyMembers(criteria);
            return Ok(result);
        }

        // GET: api/b2b/companyMember/{id}
        [HttpGet]
        [Route("companyMember/{id}")]
        [ResponseType(typeof(CompanyMember))]
        public IHttpActionResult GetCompanyMemberById(string id)
        {
            var retVal = _corporateMembersService.GetCompanyMemberById(id);
            return Ok(retVal);
        }

        // POST: api/b2b/companyMember
        [HttpPost]
        [Route("companyMember")]
        [ResponseType(typeof(void))]
        public IHttpActionResult UpdateMember(CompanyMember companyMember)
        {
            _corporateMembersService.SaveChanges(new[] { companyMember });
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/b2b/companyMembers
        [HttpDelete]
        [Route("companyMembers")]
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _corporateMembersService.RemoveCorporateMembersByIds(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
