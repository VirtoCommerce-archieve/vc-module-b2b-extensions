using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Search;
using VirtoCommerce.B2BExtensionsModule.Web.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b")]
    public class CorporateCommerceController: CorporateControllerBase
    {
        private readonly ICorporateCommerceSearchService _commerceSearchService;

        public CorporateCommerceController(ISecurityService securityService, ICorporateCommerceSearchService commerceSearchService) : base(securityService)
        {
            _commerceSearchService = commerceSearchService;
        }

        /// <summary>
        /// Search fulfillment centers registered in the system
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(FulfillmentCenter[]))]
        [Route("fulfillment/centers/search")]
        public IHttpActionResult SearchFulfillmentCenters([FromBody] FulfillmentCenterSearchCriteria searchCriteria)
        {
            var retVal = _commerceSearchService.SearchFulfillmentCenters(searchCriteria);
            return Ok(retVal);
        }
    }
}