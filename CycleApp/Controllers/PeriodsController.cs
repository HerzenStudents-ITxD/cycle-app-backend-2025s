using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace CycleApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PeriodsController : BaseController
    {
        private readonly CycleDbContext _dbContext;
        public PeriodsController(CycleDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePeriodRequest request,
            CancellationToken ct)
        {
            // one-liner to get the User entity (or null)
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");

            var period = new Period(
                user.UserId,
                request.StartDate,
                request.EndDate,
                request.IsActive,
                request.DayBeforePeriod);

            await DbContext.Periods.AddAsync(period, ct);
            await DbContext.SaveChangesAsync(ct);

            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> GetByUser(CancellationToken ct)
        {
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");
            var periods = await _dbContext.Periods
                .Where(p => p.UserId == user.UserId)
                .Select(p => new PeriodDto(p.PeriodId, user.UserId, p.StartDate, p.EndDate ?? DateTime.UtcNow, p.IsActive,
                    p.IsPredicted))
                .ToListAsync(ct);

            return Ok(periods);
        }

        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetPeriodsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken ct)
        {
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");
            try
            {
                var periods = await _dbContext.Periods
                    .Where(p => p.UserId == user.UserId && p.StartDate >= startDate && p.EndDate <= endDate)
                    .Select(p => new PeriodDto(
                        p.PeriodId,
                        user.UserId,
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