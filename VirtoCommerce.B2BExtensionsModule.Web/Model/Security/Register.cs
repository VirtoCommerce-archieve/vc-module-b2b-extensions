using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model.Security
{
    public class Register
    {
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string StoreId { get; set; }
    }
}