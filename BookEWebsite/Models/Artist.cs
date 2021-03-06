using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Models
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }
        public bool CompletedRegistration { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Specialization { get; set; }
        public string Description { get; set; }
        public bool LookingForGigs { get; set; }
        public string GroupName { get; set; }
        public int? SizeOfGroup { get; set; }
        public double? HourlyCost { get; set; }
        public double? WeekendHourlyCost { get; set; }

        [ForeignKey("IdentityUser")]
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }

        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public Address Address { get; set; }

        [NotMapped]
        public double? CenterLatitude { get; set; }
        [NotMapped]
        public double? CenterLongitude { get; set; }
        [NotMapped]
        public DateTime? DayToCheck { get; set; }
        [NotMapped]
        public DateTime? StartTime { get; set; }
        [NotMapped]
        public DateTime? EndTime { get; set; }
    }
}
