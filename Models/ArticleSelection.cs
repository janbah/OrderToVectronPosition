using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.Models
{
    public class ArticleSelection
    {
        public int Id { get; set; }

        public int VectronPluNo { get; set; }

        public string Name { get; set; }

        public int IoneRefId { get; set; }
    }
}
