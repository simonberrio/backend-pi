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

            // EventParticipant → Event
            builder.Entity<EventParticipant>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // ❗ Evitar duplicados (usuario no se puede registrar 2 veces)
            builder.Entity<EventParticipant>()
                .HasIndex(ep => new { ep.UserId, ep.EventId })
                .IsUnique();

            // Enum como string (MUY recomendado 🔥)
            builder.Entity<EventParticipant>()
                .Property(ep => ep.Status)
                .HasConversion<string>();

            builder.Entity<Event>()
                .Property(e => e.Category)
                .HasConversion<string>();
        }
    }
}
