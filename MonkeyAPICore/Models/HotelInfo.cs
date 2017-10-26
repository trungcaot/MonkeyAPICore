using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyAPICore.Models
{
    public class HotelInfo : Resource
    {
        public string Title { get; set; }
        public string TagLine { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public Address Location { get; set; }
    }

    public class Address
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }

    }
}
