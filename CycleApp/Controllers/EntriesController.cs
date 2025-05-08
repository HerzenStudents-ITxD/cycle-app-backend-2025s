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

namespace CycleApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EntriesController : BaseController
    {
        private readonly CycleDbContext _dbContext;

        public EntriesController(CycleDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // POST: Создание новой записи
        [HttpPost]
        public async Task<IActionResult> CreateEntry(
            [FromBody] CreateEntryRequest request,
            CancellationToken ct)
        {
            try
            {
                var user = await GetUserFromClaimsAsync(ct);
                if (user == null)
                    return NotFound("User not found");

                var entry = new Entry
                {
                    UserId = user.UserId,
                    Date = request.date ?? DateTime.UtcNow,
                    PeriodStarted = request.periodStarted,
                    PeriodEnded = request.periodEnded,
                    Note = request.note,
                    Heaviness = request.heaviness,
                    Symptoms = request.symptoms,
                    Sex = request.sex,
                    Mood = request.mood,
                    Discharges = request.discharges
                };

                await _dbContext.Entries.AddAsync(entry, ct);
                await _dbContext.SaveChangesAsync(ct);

                return Ok(new
                {
                    success = true,
                    entryId = entry.EntryId,
                    date = entry.Date
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { error = "Database error" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: Получить конкретную запись по ID
        [HttpGet("{entryId}")]
        public async Task<IActionResult> GetEntryById(Guid entryId, CancellationToken ct)
        {
            try
            {
                var entry = await _dbContext.Entries
                    .FirstOrDefaultAsync(e => e.EntryId == entryId, ct);

                if (entry == null)
                {
                    return NotFound(new { error = "Entry not found" });
                }

                var entryDto = new EntryDto(
                    entry.EntryId,
                    entry.UserId,
                    entry.Date,
                    entry.PeriodStarted ?? false,
                    entry.PeriodEnded ?? false,
                    entry.Note,
                    entry.Heaviness,
                    entry.Symptoms,
                    entry.Sex,
                    entry.Mood,
                    entry.Discharges
                );

                return Ok(entryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // PUT: Обновить конкретную запись по ID
        [HttpPut("{entryId}")]
        public async Task<IActionResult> UpdateEntry(Guid entryId, [FromBody] UpdateEntryRequest request, CancellationToken ct)
        {
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");
            try
            {
                var entry = await _dbContext.Entries
                    .FirstOrDefaultAsync(e => e.EntryId == entryId
                                              && e.UserId    == user.UserId,
                        ct);

                if (entry == null)
                    return NotFound(new { error = "Entry not found" });
                

                // Обновляем поля записи
                entry.Date = request.date ?? entry.Date;
                entry.PeriodStarted = request.periodStarted ?? entry.PeriodStarted;
                entry.PeriodEnded = request.periodEnded ?? entry.PeriodEnded;
                entry.Note = request.note ?? entry.Note;
                entry.Heaviness = request.heaviness ?? entry.Heaviness;
                entry.Symptoms = request.symptoms ?? entry.Symptoms;
                entry.Sex = request.sex ?? entry.Sex;
                entry.Mood = request.mood ?? entry.Mood;
                entry.Discharges = request.discharges ?? entry.Discharges;

                _dbContext.Entries.Update(entry);
                await _dbContext.SaveChangesAsync(ct);

                return Ok(new { success = true, message = "Entry updated successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { error = "Database error" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // DELETE: Удалить конкретную запись по ID
        [HttpDelete("{entryId}")]
        public async Task<IActionResult> DeleteEntry(Guid entryId, CancellationToken ct)
        {
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");
            try
            {
                var entry = await _dbContext.Entries
                    .FirstOrDefaultAsync(e => e.EntryId == entryId
                                              && e.UserId    == user.UserId,
                        ct);

                if (entry == null)
                    return NotFound(new { error = "Entry not found" });


                _dbContext.Entries.Remove(entry);
                await _dbContext.SaveChangesAsync(ct);

                return Ok(new { success = true, message = "Entry deleted successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { error = "Database error" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetEntriesByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct)
            {
                var user = await GetUserFromClaimsAsync(ct);
                if (user == null)
                    return NotFound("User not found");
                try
                {
                    var entries = await _dbContext.Entries
                        .Where(e => e.UserId == user.UserId && e.Date >= startDate && e.Date <= endDate)
                        .Select(e => new EntryDto(
                            e.EntryId,
                            user.UserId,
                            e.Date,
                            e.PeriodStarted ?? false,
                            e.PeriodEnded ?? false,
                            e.Note,
                            e.Heaviness,
                            e.Symptoms,
                            e.Sex,
                            e.Mood,
                            e.Discharges
                        ))
                        .ToListAsync(ct);

                    return Ok(entries);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "Internal server error", details = ex.Message });
                }
            }
    }
}