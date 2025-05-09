using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace CycleApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EntriesController : BaseController
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<EntriesController> _logger;

        public EntriesController(CycleDbContext dbContext, ILogger<EntriesController> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // POST: Создание новой записи
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEntryRequest request)
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var entry = new Entry
            {
                UserId = user.UserId,
                Date = request.Date ?? DateTime.UtcNow,
                PeriodStarted = request.PeriodStarted,
                PeriodEnded = request.PeriodEnded,
                Note = request.Note,
                Heaviness = request.Heaviness,
                Sex = request.Sex,
                Mood = request.Mood,
                Discharges = request.Discharges
            };

            // If period started, create a new period
            if (request.PeriodStarted)
            {
                var period = new Period
                {
                    UserId = user.UserId,
                    StartDate = entry.Date,
                    IsActive = true,
                    DayOfCycle = 1
                };
                DbContext.Periods.Add(period);
                await DbContext.SaveChangesAsync();
                entry.PeriodId = period.PeriodId;
            }
            // If period ended, find the active period and update it
            else if (request.PeriodEnded)
            {
                var activePeriod = await DbContext.Periods
                    .FirstOrDefaultAsync(p => p.UserId == user.UserId && p.IsActive);
                
                if (activePeriod != null)
                {
                    activePeriod.EndDate = entry.Date;
                    activePeriod.IsActive = false;
                    entry.PeriodId = activePeriod.PeriodId;
                }
            }

            // Add symptoms if provided
            if (request.Symptoms != null && request.Symptoms.Any())
            {
                entry.Symptoms = request.Symptoms.Select(s => new EntrySymptom
                {
                    Name = s.Name,
                    Intensity = s.Intensity,
                    Notes = s.Notes
                }).ToList();
            }

            DbContext.Entries.Add(entry);
            await DbContext.SaveChangesAsync();

            return Ok(entry);
        }

        // GET: Получить конкретную запись по ID
        [HttpGet("{entryId}")]
        public async Task<IActionResult> GetEntryById(int entryId, CancellationToken ct)
        {
            try
            {
                var entry = await _dbContext.Entries
                    .Include(e => e.Symptoms)
                    .FirstOrDefaultAsync(e => e.EntryId == entryId, ct);

                if (entry == null)
                {
                    return NotFound(new { error = "Entry not found" });
                }

                var entryDto = new EntryDto(
                    entry.EntryId,
                    entry.UserId,
                    entry.PeriodId,
                    entry.Date,
                    entry.PeriodStarted ?? false,
                    entry.PeriodEnded ?? false,
                    entry.Note,
                    entry.Heaviness,
                    entry.Symptoms.Select(s => new SymptomDto(
                        s.EntrySymptomId.GetHashCode(),
                        entry.EntryId,
                        s.Name,
                        s.Intensity,
                        s.Notes
                    )).ToList(),
                    entry.Sex,
                    entry.Mood,
                    entry.Discharges
                );

                return Ok(entryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entry {EntryId}", entryId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // PUT: Обновить конкретную запись по ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEntryRequest request)
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var entry = await DbContext.Entries
                .Include(e => e.Symptoms)
                .FirstOrDefaultAsync(e => e.EntryId == id && e.UserId == user.UserId);

            if (entry == null)
                return NotFound("Entry not found");

            entry.Note = request.Note;
            entry.Heaviness = request.Heaviness;
            entry.Sex = request.Sex;
            entry.Mood = request.Mood;
            entry.Discharges = request.Discharges;

            // Update symptoms
            if (request.Symptoms != null)
            {
                // Remove existing symptoms
                DbContext.EntrySymptoms.RemoveRange(entry.Symptoms);

                // Add new symptoms
                entry.Symptoms = request.Symptoms.Select(s => new EntrySymptom
                {
                    Name = s.Name,
                    Intensity = s.Intensity,
                    Notes = s.Notes
                }).ToList();
            }

            await DbContext.SaveChangesAsync();
            return Ok(entry);
        }

        // DELETE: Удалить конкретную запись по ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var entry = await DbContext.Entries
                .FirstOrDefaultAsync(e => e.EntryId == id && e.UserId == user.UserId);

            if (entry == null)
                return NotFound("Entry not found");

            DbContext.Entries.Remove(entry);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetEntriesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var entries = await DbContext.Entries
                .Include(e => e.Symptoms)
                .Where(e => e.UserId == user.UserId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(entries);
        }

        [HttpGet("current-period")]
        public async Task<IActionResult> GetCurrentPeriodEntries()
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var activePeriod = await DbContext.Periods
                .FirstOrDefaultAsync(p => p.UserId == user.UserId && p.IsActive);

            if (activePeriod == null)
                return NotFound("No active period found");

            var entries = await DbContext.Entries
                .Include(e => e.Symptoms)
                .Where(e => e.PeriodId == activePeriod.PeriodId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(entries);
        }
    }
}