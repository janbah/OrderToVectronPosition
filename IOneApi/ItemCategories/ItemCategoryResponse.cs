using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.ItemCategories
{
    public class ItemCategoryResponse : ApiResponse
    {
        public ItemCategory Data { get; set; }
    }
}
