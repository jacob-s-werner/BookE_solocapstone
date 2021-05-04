using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Services.ScheduleOptionsSvc
{
    public class ScheduleOptionsService
    {
        public string[] Hours { get;}
        public string[] Minutes { get; }

        public string[] TimeOfDay { get; }
        public string[] DaysOfTheWeek { get; }

        public ScheduleOptionsService()
        {
            Hours = new string[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
            Minutes = new string[] { "00", "15", "30", "45" };
            TimeOfDay = new string[] { "AM", "PM" };
            DaysOfTheWeek = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        }
    }
}
