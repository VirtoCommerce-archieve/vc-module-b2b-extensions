using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Mailing;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications;
using VirtoCommerce.B2BExtensionsModule.Web.Model.Security;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.B2BExtensionsModule.Web.Controllers.Api
{
    [RoutePrefix("api/b2b/send")]
    public class CorporateMailingController: CorporateControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly IItemService _productService;
        private readonly INotificationManager _notificationManager;

        public CorporateMailingController(IStoreService storeService, IItemService productService, ISecurityService securityService, INotificationManager notificationManager) : base(securityService)
        {
            _storeService = storeService;
            _productService = productService;
            _notificationManager = notificationManager;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("product/{id}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult SendProduct(string id, [FromBody] CorporateProductSendingData sendingData)
        {
            var store = _storeService.GetById(sendingData.StoreId);
            var product = _productService.GetById(id, ItemResponseGroup.ItemLarge);

            var notification = _notificationManager.GetNewNotification<CorporateProductEmailNotification>(sendingData.StoreId, "Store", sendingData.Language);

            notification.Product = product;
            notification.ProductUrl = sendingData.ProductUrl;
            notification.StoreName = store.Name;
            notification.Sender = store.Email;
            notification.IsActive = true;

            notification.Recipient = sendingData.Email;

            _notificationManager.ScheduleSendNotification(notification);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}