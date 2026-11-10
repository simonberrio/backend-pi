using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<EventStaff> EventStaff { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OrganizerPaymentSettings> OrganizerPaymentSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventParticipant>().HasOne(ep => ep.Event).WithMany(e => e.Participants).HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventParticipant>().HasIndex(ep => new { ep.UserId, ep.EventId }).IsUnique();

            builder.Entity<EventParticipant>().Property(ep => ep.Status).HasConversion<string>();

            builder.Entity<EventParticipant>().HasIndex(ep => ep.CheckInCode).IsUnique();

            builder.Entity<Event>().HasOne(e => e.CreatedByUser).WithMany(e => e.Events).HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>().Property(e => e.Category).HasConversion<string>();

            builder.Entity<Reaction>().HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

            builder.Entity<Review>().HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

            builder.Entity<Review>().HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventStaff>().HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

            builder.Entity<EventStaff>().HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventStaff>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>().HasIndex(x => x.Reference).IsUnique();

            builder.Entity<Payment>().Property(x => x.Status).HasConversion<string>();

            builder.Entity<Payment>().HasOne(x => x.EventParticipant).WithMany(x => x.Payments).HasForeignKey(x => x.EventParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>().HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>().HasOne<User>().WithMany().HasForeignKey(x => x.OrganizerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrganizerPaymentSettings>().HasIndex(x => x.UserId).IsUnique();

            builder.Entity<OrganizerPaymentSettings>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
