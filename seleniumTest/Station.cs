using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScraperBase
{
    class Station
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string Predictions { get; set; }
        public string Href { get; set; }

        public Station() { }
    }
}
