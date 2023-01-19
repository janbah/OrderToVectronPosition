using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Orders
{
    public class OrderItemData
    {
        public int Id { get; set; }
        public string IoneId { get; set; }
        public int StatusId { get; set; }
        public string CustomerRefNumber { get; set; }
        public string OrderDate { get; set; }
        public string Total { get; set; }
        public string SubTotal { get; set; }
        public string TaxTotal { get; set; }
        public List<OrderItem> OrderItemList { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedDate { get; set; }

    }
}
