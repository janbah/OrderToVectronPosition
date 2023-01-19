using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Orders
{
    public class RefundData
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string TotalRefundedAmount { get; set; }
        public string PaymentTransactionId { get; set; }
        public string RefundTransactionId { get; set; }
    }
}
