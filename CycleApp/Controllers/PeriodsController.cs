using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.AspNetCore.Authorization;
using CycleApp.Services;
using Microsoft.Extensions.Logging;

namespace CycleApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PeriodsController : BaseController
    {
        private readonly CycleDbContext _dbContext;
        private readonly ICycleCalculatorService _calculator;
        private readonly IPeriodTableService _periodTableService;
        private readonly ILogger<PeriodsController> _logger;

        public PeriodsController(
            CycleDbContext dbContext, 
            ICycleCalculatorService calculator,
            IPeriodTableService periodTableService,
            ILogger<PeriodsController> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _calculator = calculator;
            _periodTableService = periodTableService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePeriodRequest request,
            CancellationToken ct)
        {
            var user = await GetUserFromClaimsAsync(ct);
            if (user == null)
                return NotFound("User not found");

            // Create the actual period
            var period = new Period(
                user.UserId,
                request.StartDate,
                request.EndDate,
                request.IsActive,
                request.DayBeforePeriod);

            await DbContext.Periods.AddAsync(period, ct);
            await DbContext.SaveChangesAsync(ct);

            // Update cycle variations
            _calculator.UpdateCycleVariations(user);

            // Clean up any existing predictions
            var existingPredictions = await DbContext.Periods
                .Where(p => p.UserId == user.UserId && p.IsPredicted)
                .ToListAsync(ct);
            DbContext.Periods.RemoveRange(existingPredictions);

            // Generate predictions for the next 3 cycles
            DateTime baseDate = request.StartDate;
            for (int i = 0; i < 3; i++)
            {
                var (ovulationStart, ovulationEnd) = _calculator.CalculateNextOvulation(user, baseDate);
                var (periodStart, periodEnd) = _calculator.CalculateNextPeriod(user, baseDate);

                // Add ovulation prediction
                await DbContext.Ovulations.AddAsync(new Ovulation
                {
                    UserId = user.UserId,
                    StartDate = ovulationStart,
                    EndDate = ovulationEnd,
                    IsPredicted = true
                }, ct);

                // Add period prediction
                await DbContext.Periods.AddAsync(new Period
                {
                    UserId = user.UserId,
                    StartDate = periodStart,
                    EndDate = periodEnd,
                    IsActive = false,
                    IsPredicted = true,
                    DayOfCycle = (i + 1) * user.CycleLength
                }, ct);

                // Update base date for next iteration
                baseDate = periodStart;
            }

            await DbContext.SaveChangesAsync(ct);

            return Ok();
        }

        // [HttpGet]
        // public async Task<IActionResult> GetByUser(CancellationToken ct)
        // {
        //     var user = await GetUserFromClaimsAsync(ct);
        //     if (user == null)
        //         return NotFound("User not found");
        //     var periods = await _dbContext.Periods
        //         .Where(p => p.UserId == user.UserId)
        //         .OrderBy(p => p.StartDate)
        //         .Select(p => new PeriodDto(p.PeriodId, user.UserId, p.StartDate, p.EndDate ?? DateTime.UtcNow, p.IsActive,
        //             p.IsPredicted))
        //         .ToListAsync(ct);
        //
        //     return Ok(periods);
        // }
        //
        // [HttpGet("by-date-range")]
        // public async Task<IActionResult> GetPeriodsByDateRange(
        //     [FromQuery] DateTime startDate,
        //     [FromQuery] DateTime endDate,
        //     CancellationToken ct)
        // {
        //     var user = await GetUserFromClaimsAsync(ct);
        //     if (user == null)
        //         return NotFound("User not found");
        //     try
        //     {
        //         var periods = await _dbContext.Periods
        //             .Where(p => p.UserId == user.UserId && p.StartDate >= startDate && p.EndDate <= endDate)
        //             .OrderBy(p => p.StartDate)
        //             .Select(p => new PeriodDto(
        //                 p.PeriodId,
        //                 user.UserId,
        //                 p.StartDate,
        //                 p.EndDate ?? DateTime.UtcNow,
        //                 p.IsActive,
        //                 p.IsPredicted
        //             ))
        //             .ToListAsync(ct);
        //
        //         return Ok(periods);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        //     }
        // }

        [HttpGet("table")]
        public async Task<IActionResult> GetPeriodTable([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var user = await GetUserFromClaimsAsync();
            if (user == null)
                return NotFound("User not found");

            var table = await _periodTableService.GetPeriodTableAsync(user.UserId, startDate, endDate);
            return Ok(table);
        }
    }
}