﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi.Items
{
    public class ItemResponse : ApiResponse
    {
        public Item Data { get; set; }
    }
}
