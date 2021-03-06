using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Models
{
    public class BusinessEvent
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime StartTime { get; set; } // Will convert // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?redirectedfrom=MSDN&view=net-5.0#System_DateTime_ToString_System_String_
        public DateTime EndTime { get; set; }
        public DateTime TimeBooked { get; set; }
        public double Cost { get; set; }
        public double? Tip { get; set; }
        
        [ForeignKey("Business")]
        public int? BusinessId { get; set; }
        public Business Business { get; set; }
        
        [ForeignKey("Artist")]
        public int? ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
