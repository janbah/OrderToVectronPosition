using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi
{
    public class MappingArticle
    {
        public int IoneRefIdMain { get; set; }
        public int IoneRefIdCondiment { get; set; }
        public int VectronPluNo { get; set; }
        public int? ItemCategoryIdMain { get; set; }
    }
}
