using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.IoneApi
{
    public class BranchAddress
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }
        public string FloorName { get; set; }
        public string PositionName { get; set; }
        public string HouseCode { get; set; }
        public string PostalCode { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateId { get; set; }
        public string Country { get; set; }
        public int CountryId { get; set; }
        public string FullAddress { get; set; }
    }
}
