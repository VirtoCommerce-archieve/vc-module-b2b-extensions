﻿using VirtoCommerce.Domain.Customer.Model;

namespace VirtoCommerce.B2BExtensionsModule.Web.Model
{
    public class Department : Member
    {
        public string ParentId { get; set; }
    }
}