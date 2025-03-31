using Microsoft.EntityFrameworkCore;
using CycleApp.Models;

namespace CycleApp.DataAccess;

public class CycleDbContext : DbContext
{
    public CycleDbContext(DbContextOptions<CycleDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Period> Periods { get; set; }
    public DbSet<Ovulation> Ovulations { get; set; }
    public DbSet<Entry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.user_id).HasName("user_id");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.CycleLength).HasColumnName("cycle_length");
            entity.Property(u => u.CreateDate).HasColumnName("create_data");
            entity.Property(u => u.RemindPeriod).HasColumnName("remind_period");
            entity.Property(u => u.RemindOvulation).HasColumnName("remind_ovulation");
            entity.Property(u => u.PeriodLength).HasColumnName("period_length");
            entity.Property(u => u.Theme).HasColumnName("theme");
        });

        
        modelBuilder.Entity<Period>(entity =>
        {
            entity.ToTable("periods");
            entity.HasKey(p => p.period_id).HasName("period_id");
            entity.Property(p => p.user_id).HasColumnName("user_id");
            entity.Property(p => p.StartDate).HasColumnName("start_date");
            entity.Property(p => p.EndDate).HasColumnName("end_date");
            entity.Property(p => p.IsActive).HasColumnName("is_active");

            entity.HasOne(p => p.User)
                  .WithMany(u => u.Periods)
                  .HasForeignKey(p => p.user_id);
        });

        
        modelBuilder.Entity<Ovulation>(entity =>
        {
            entity.ToTable("ovulations");
            entity.HasKey(o => o.ovulation_id).HasName("ovulation_id");
            entity.Property(o => o.user_id).HasColumnName("user_id");
            entity.Property(o => o.StartDate).HasColumnName("start_date");
            entity.Property(o => o.EndDate).HasColumnName("end_date");

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Ovulations)
                  .HasForeignKey(o => o.user_id);
        });

     
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.ToTable("entries");
            entity.HasKey(e => e.entry_id).HasName("entry_id");
            entity.Property(e => e.user_id).HasColumnName("user_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.PeriodStarted).HasColumnName("period_started");
            entity.Property(e => e.PeriodEnded).HasColumnName("period_ended");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Heaviness).HasColumnName("heaviness");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Discharges).HasColumnName("discharges");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Entries)
                  .HasForeignKey(e => e.user_id);
        });
    }
}