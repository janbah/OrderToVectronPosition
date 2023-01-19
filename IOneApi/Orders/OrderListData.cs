using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Orders
{
    public class OrderListData
    {
        public int StatusId { get; set; }
        public string StatusText { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public int CustomerType { get; set; }
        public string Total { get; set; }
        public string TaxTotal { get; set; }
        public string CustomerNotes { get; set; }
        public OrderItemListItem[] OrderItemList { get; set; }
        public string TableId { get; set; }
        public string CustomerRefNumber { get; set; }
        public string PreNotes { get; set; }
        public string PostNotes { get; set; }
        public string BranchAddressId { get; set; }
        public BranchAddress BranchAddress { get; set; }
        public int Id { get; set; }
        public string APIObjectId { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string IoneId { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedDate { get; set; }
    }
}
