using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Order2VPos.Core.Common;

namespace Order2VPos.Core.VPosClient.MasterData
{
    public class PLU
    {
        public int PLUno { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public bool SaleAllowed { get; set; }
        public int TaxNo { get; set; }
        public PriceData[] Prices { get; set; }
        public int[] SelectWin { get; set; }
        public int DepartmentNo { get; set; }
        public string Attributes { get; set; }
        public int MainGroupA { get; set; }
        public int MainGroupB { get; set; }

        public bool IsForWebShop => Attributes?.Length >= AppSettings.Default.AttributeNoForWebShop && Attributes.Substring(AppSettings.Default.AttributeNoForWebShop - 1,1) == "1";
    }
}
