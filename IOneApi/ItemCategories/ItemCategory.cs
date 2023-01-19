using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.ItemCategories
{
    public class ItemCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string APIObjectId { get; set; }
        public int[] BranchAddressIdList { get; set; }

        public int LevelId { get; set; } = 2;
        public int StatusId { get; set; } = 1;

        public int ItemCategoryWebshopLink { get; set; } = 1;

        public int? ParentId { get; set; }


    }
}
