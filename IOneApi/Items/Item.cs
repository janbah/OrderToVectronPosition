using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Items
{
    public class Item
    {
        public int Id { get; set; }
        public string APIObjectId { get; set; }
        public string Name { get; set; }
        public string BasePriceWithTax { get; set; }
        public string TaxPercentage { get; set; }
        public int[] BranchAddressIdList { get; set; }
        public int? ItemCategoryId { get; set; }
        public List<ItemPrice> ItemPriceList { get; set; }
        public bool ItemWebshopLink { get; set; }
    }
}
