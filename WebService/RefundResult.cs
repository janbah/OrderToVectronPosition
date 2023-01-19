using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.WebService
{
    public class RefundResult
    {
        public RefundStatus RefundStatus { get; set; }
        public string Message { get; set; }
    }
}
