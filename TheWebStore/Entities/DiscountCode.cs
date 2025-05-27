using System;
using System.Collections.Generic;
using WebStore.Entities;

namespace WebStore.Entities
{
    public class DiscountCode
    {
        public int DiscountCodeId { get; set; }

        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        public DiscountType DiscountType { get; set; }

        public decimal DiscountValue { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public int? MaxUsage { get; set; }

        public int TimesUsed { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
}