using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.Common
{
    public class PriceListAssignment
    {
        [Category("Vectron POS"), DisplayName("Preisebene")]
        public int VectronPriceLevel { get; set; }
        [Category("IONE API"), DisplayName("Preisliste (PriceListId)")]
        public int PriceListId { get; set; }

        public override string ToString()
        {
            return $"Ebene {VectronPriceLevel} = Preisliste {PriceListId}";
        }
    }
}
