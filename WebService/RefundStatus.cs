using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.WebService
{
    public enum RefundStatus
    {
        Unknown,
        Success,
        Fail,
        Canceled,
        Pending,
        NoCorrespondingOrder,
        Error
    }
}
