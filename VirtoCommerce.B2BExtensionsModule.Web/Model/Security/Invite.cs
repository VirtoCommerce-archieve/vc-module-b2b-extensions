using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    public class Invite
    {
        public string StoreId { get; set; }

        public string CompanyId { get; set; }

        public string[] Emails { get; set; }

        public string AdminName { get; set; }

        public string AdminEmail { get; set; }

        public string Message { get; set; }

        public string Language { get; set; }

        public string CallbackUrl { get; set; }
    }
}