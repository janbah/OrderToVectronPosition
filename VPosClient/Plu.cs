using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.VPosClient
{
    public class Plu
    {
        public Plu()
        {
            Modifier = new List<int>();
            AdditionalPlus = new List<Plu>();
        }

        public int Number { get; set; }
        public decimal Quantity { get; set; }

        public decimal ModifyPriceAbsoluteValue { get; set; }
        public decimal ModifyPriceAbsoluteFactor { get; set; }

        public decimal ModifyPricePercentValue { get; set; }
        public decimal ModifyPricePercentFactor { get; set; }

        public decimal? OverridePriceValue { get; set; }
        public decimal OverridePriceFactor { get; set; }

        public List<int> Modifier { get; set; }

        public List<Plu> AdditionalPlus { get; set; }
    }
}
