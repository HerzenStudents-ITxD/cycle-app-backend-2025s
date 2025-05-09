using Microsoft.EntityFrameworkCore;
using CycleApp.Models;

namespace CycleApp.DataAccess
{
    public class CycleDbContext : DbContext
    {
        public CycleDbContext(DbContextOptions<CycleDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<Ovulation> Ovulations { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<EntrySymptom> EntrySymptoms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(u => u.CycleLength)
                    .IsRequired();

                entity.Property(u => u.PeriodLength)
                    .IsRequired();

                entity.Property(u => u.TimeZoneId)
                    .HasMaxLength(50);

                entity.HasMany(u => u.Periods)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Ovulations)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Entries)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Period>(entity =>
            {
                entity.Property(p => p.StartDate)
                    .IsRequired();

                entity.Property(p => p.IsActive)
                    .IsRequired();

                entity.Property(p => p.IsPredicted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(p => p.DayOfCycle)
                    .IsRequired();

                entity.HasMany(p => p.Entries)
                    .WithOne(e => e.Period)
                    .HasForeignKey(e => e.PeriodId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Ovulation>(entity =>
            {
                entity.Property(o => o.StartDate)
                    .IsRequired();

                entity.Property(o => o.EndDate)
                    .IsRequired();

                entity.Property(o => o.IsPredicted)
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Entry>(entity =>
            {
                entity.Property(e => e.Date)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.PeriodStarted)
                    .IsRequired(false);

                entity.Property(e => e.PeriodEnded)
                    .IsRequired(false);

                entity.Property(e => e.Note)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.Heaviness)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(e => e.Sex)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(e => e.Mood)
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.Discharges)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.HasMany(e => e.Symptoms)
                    .WithOne(s => s.Entry)
                    .HasForeignKey(s => s.EntryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EntrySymptom>(entity =>
            {
                entity.Property(s => s.EntrySymptomId)
                    .UseIdentityColumn();

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.Intensity)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(s => s.Notes)
                    .HasMaxLength(200)
                    .IsRequired(false);
            });
        }
    }
}