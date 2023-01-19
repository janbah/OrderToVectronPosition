using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Orders
{
    public class OrderItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string GrossPrice { get; set; }
        public string Total { get; set; }
        public string Quantity { get; set; }
        public string TaxPercentage { get; set; }
        public string Discount { get; set; }
        public string DiscountUnit { get; set; }
        public string DiscountQuantity { get; set; }
    }
}
