using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotTask
{
    public class departure
    {
        public string flight_number { get; set; }
        public string status { get; set; }
        public string destination { get; set; }
        public string scheduled_departure_time { get; set; }
        public string estimated_departure_time { get; set; }
        public string date { get; set; }
    }
}
