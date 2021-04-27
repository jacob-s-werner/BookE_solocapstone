using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookEWebsite.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>()
            .HasData(
            new IdentityRole
            {
                Id = "8539f5e1-71f4-4b4f-87d9-4a2992fee49d",
                ConcurrencyStamp = "648007b5-8ed0-4f6b-ad39-0619783b8a46",
                Name = "Artist",
                NormalizedName = "ARTIST"
            },
            new IdentityRole
            {
                Id = "975bbdf1-9b96-4b7e-99e2-8b397a941a56",
                ConcurrencyStamp = "d702cc45-74b5-49c9-95e5-af9595ddfe36",
                Name = "Business",
                NormalizedName = "BUSINESS"
            }
            );
        }
    }
}
