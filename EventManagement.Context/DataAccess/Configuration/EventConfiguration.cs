using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Context.DataAccess.Configuration
{
    public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedNever();
            builder.Property(a => a.Title).IsRequired().HasMaxLength(50);
            builder.Property(a => a.TotalSeats).IsRequired();
            builder.HasMany(a => a.Bookings).WithOne(a => a.Event).HasForeignKey(a => a.EventId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
