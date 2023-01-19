using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.Common
{
    public struct GcRange
    {
        public int From { get; set; }
        public int To { get; set; }

        public override string ToString()
        {
            return $"{From} - {To}";
        }
    }
}
