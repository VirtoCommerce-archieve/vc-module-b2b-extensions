using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications
{
    public class CorporateProductEmailNotification : EmailNotification
    {
        public CorporateProductEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        [NotificationParameter("Product")]
        public CatalogProduct Product { get; set; }

        [NotificationParameter("ProductUrl")]
        public string ProductUrl { get; set; }

        [NotificationParameter("Store name")]
        public string StoreName { get; set; }
    }
}