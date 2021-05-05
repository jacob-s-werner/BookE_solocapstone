using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Models
{
    public class TimeViewModel
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string TimeOfDay { get; set; }
        public string Time { get; set; }
        public string Day { get; set; }

    }
}
