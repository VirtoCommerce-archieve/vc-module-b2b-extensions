using VirtoCommerce.Platform.Core.Notifications;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Notifications
{
    public class CorporateInviteEmailNotification : EmailNotification
    {
        public CorporateInviteEmailNotification(IEmailNotificationSendingGateway gateway)
            : base(gateway)
        {
        }

        [NotificationParameter("Invite URL")]
        public string Url { get; set; }

        [NotificationParameter("Store name")]
        public string StoreName { get; set; }

        [NotificationParameter("Company name")]
        public string CompanyName { get; set; }

        [NotificationParameter("Company's administrator name")]
        public string AdminName { get; set; }

        [NotificationParameter("Company's administrator email")]
        public string AdminEmail { get; set; }

        [NotificationParameter("Optional. Custom message appended to notification.")]
        public string Message { get; set; }
    }
}