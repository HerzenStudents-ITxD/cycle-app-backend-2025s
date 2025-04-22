using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntriesController : ControllerBase
    {
        private readonly CycleDbContext _dbContext;
        private readonly ILogger<EntriesController> _logger;

        public EntriesController(
            CycleDbContext dbContext,
            ILogger<EntriesController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry(
            [FromBody] CreateEntryRequest request,
            CancellationToken ct)
        {
            try
            {
                // Проверка существования пользователя
                var userExists = await _dbContext.Users.AnyAsync(u => u.UserId == request.user_id, ct);
                if (!userExists)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var entry = new Entry
                {
                    UserId = request.user_id,
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
                _logger.LogError(dbEx, "Database error while creating entry");
                return StatusCode(500, new { error = "Database error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating entry");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}