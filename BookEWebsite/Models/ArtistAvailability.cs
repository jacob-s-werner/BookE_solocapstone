using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Models
{
    public class ArtistAvailability
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartTime { get; set; } // Will convert // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.tostring?redirectedfrom=MSDN&view=net-5.0#System_DateTime_ToString_System_String_
        public DateTime EndTime { get; set; }
        public string DayOfWeek { get; set; }
        public bool Recurring { get; set; }

        [ForeignKey("Artist")]
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        [NotMapped]
        public List<ArtistAvailability> aAvailabilitiesList {get; set;} 
    }
}
