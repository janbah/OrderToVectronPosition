using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.VPosClient.MasterData
{
    public class Tax
    {
        public int TaxNo { get; set; }
        public decimal Rate { get; set; }
        public string Name { get; set; }
    }
}
