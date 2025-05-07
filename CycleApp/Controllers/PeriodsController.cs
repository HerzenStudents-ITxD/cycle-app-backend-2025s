using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace CycleApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PeriodsController : ControllerBase
    {
        private readonly CycleDbContext _dbContext;

        public PeriodsController(CycleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePeriodRequest request, CancellationToken ct)
        {
            var period = new Period(request.UserId, request.StartDate, request.EndDate, request.IsActive, request.DayBeforePeriod);

            await _dbContext.Periods.AddAsync(period, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
        {   
            var periods = await _dbContext.Periods
                .Where(p => p.UserId == userId)
                .Select(p => new PeriodDto(p.PeriodId, p.UserId, p.StartDate, p.EndDate ?? DateTime.UtcNow, p.IsActive,p.IsPredicted))
                .ToListAsync(ct);

            return Ok( periods);
        }
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetPeriodsByDateRange(
        [FromQuery] DateTime start_date,
        [FromQuery] DateTime end_date,
        [FromQuery] Guid user_id,
        CancellationToken ct)
            {

                try
                {
                    var periods = await _dbContext.Periods
                        .Where(p => p.UserId == user_id && p.StartDate >= start_date && p.EndDate <= end_date)
                        .Select(p => new PeriodDto(
                            p.PeriodId,
                            p.UserId,
                            p.StartDate,
                            p.EndDate ?? DateTime.UtcNow,
                            p.IsActive,
                            p.IsPredicted
                        ))
                        .ToListAsync(ct);

                    return Ok(periods);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "Internal server error", details = ex.Message });
                }
            }
    }
}
