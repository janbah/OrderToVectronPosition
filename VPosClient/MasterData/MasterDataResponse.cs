using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.VPosClient.MasterData
{
    public class MasterDataResponse
    {
        public Tax[] Taxes { get; set; }
        public PLU[] PLUs { get; set; }
        public SelWin[] SelWins { get; set; }
        public Department[] Departments { get; set; }
    }
}
