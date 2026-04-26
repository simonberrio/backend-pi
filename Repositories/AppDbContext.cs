using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventParticipant>().HasOne(ep => ep.Event).WithMany(e => e.Participants).HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventParticipant>().HasIndex(ep => new { ep.UserId, ep.EventId }).IsUnique();

            builder.Entity<EventParticipant>().Property(ep => ep.Status).HasConversion<string>();

            builder.Entity<Event>().HasOne(e => e.CreatedByUser).WithMany(e => e.Events).HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>().Property(e => e.Category).HasConversion<string>();
        }
    }
}
