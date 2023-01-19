using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.VPosClient
{
    public class VPosResponse
    {
        public bool IsError { get; set; }
        public bool IsCanceled { get; set; }
        public int VPosErrorNumber { get; set; }
        public string Message { get; set; }
        public int ReceiptMainNo { get; set; }
        public decimal SubTotal { get; set; }
        public string UUId { get; set; }
    }
}
