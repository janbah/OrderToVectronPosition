using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.WebService
{
    public class ResponseMessage
    {
        public string Message { get; set; }
        public bool RefundSuccess { get; set; }
        public bool ReceiptIgnored { get; set; }
    }
}
