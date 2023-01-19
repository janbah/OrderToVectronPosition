using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Items
{
    public class ItemPrice
    {
        public int PriceListId { get; set; }
        public int PriceListType { get; set; }
        public string PriceListTypeText { get; set; }
        public string BasePriceWithTax { get; set; }
        public string TaxPercentage { get; set; }
    }
}
