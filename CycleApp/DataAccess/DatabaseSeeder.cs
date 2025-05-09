using CycleApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleApp.DataAccess
{
    public static class DatabaseSeeder
    {
        public static async Task SeedTestData(CycleDbContext dbContext)
        {
            if (await dbContext.Users.AnyAsync())
                return;

            // Create test users
            var users = new List<User>
            {
                new User
                {
                    Email = "test1@example.com",
                    CycleLength = 28,
                    PeriodLength = 5,
                    RemindPeriod = true,
                    RemindOvulation = true,
                    TimeZoneId = "UTC"
                },
                new User
                {
                    Email = "test2@example.com",
                    CycleLength = 30,
                    PeriodLength = 4,
                    RemindPeriod = true,
                    RemindOvulation = true,
                    TimeZoneId = "America/New_York"
                }
            };

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            // Add periods for each user
            foreach (var user in users)
            {
                var periods = new List<Period>();
                var startDate = DateTime.UtcNow.AddDays(-60); // Start 60 days ago

                for (int i = 0; i < 3; i++) // Add 3 past periods
                {
                    var periodStart = startDate.AddDays(i * user.CycleLength);
                    periods.Add(new Period
                    {
                        UserId = user.UserId,
                        StartDate = periodStart,
                        EndDate = periodStart.AddDays(user.PeriodLength),
                        IsActive = false,
                        IsPredicted = false,
                        DayOfCycle = 1
                    });
                }

                await dbContext.Periods.AddRangeAsync(periods);
            }

            await dbContext.SaveChangesAsync(); // Save periods before querying them

            // Add some entries
            var entries = new List<Entry>();
            foreach (var user in users)
            {
                var lastPeriod = await dbContext.Periods
                    .Where(p => p.UserId == user.UserId)
                    .OrderByDescending(p => p.StartDate)
                    .FirstAsync();

                entries.Add(new Entry
                {
                    UserId = user.UserId,
                    Date = lastPeriod.StartDate.AddDays(2),
                    PeriodStarted = true,
                    Heaviness = "Medium",
                    Symptoms = new List<EntrySymptom>
                    {
                        new EntrySymptom
                        {
                            Name = "None",
                            Intensity = "None",
                            Notes = "No symptoms"
                        }
                    },
                    Mood = "Tired"
                });

                entries.Add(new Entry
                {
                    UserId = user.UserId,
                    Date = lastPeriod.StartDate.AddDays(5),
                    PeriodEnded = true,
                    Heaviness = "Light",
                    Symptoms = new List<EntrySymptom>
                    {
                        new EntrySymptom
                        {
                            Name = "None",
                            Intensity = "None",
                            Notes = "No symptoms"
                        }
                    },
                    Mood = "Good"
                });
            }

            await dbContext.Entries.AddRangeAsync(entries);
            await dbContext.SaveChangesAsync();
        }
    }
} 