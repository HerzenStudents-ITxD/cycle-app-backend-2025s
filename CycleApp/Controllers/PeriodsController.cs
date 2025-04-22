using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
                .Select(p => new PeriodDto(p.PeriodId, p.UserId, p.StartDate, p.EndDate, p.IsActive))
                .ToListAsync(ct);

            return Ok(new GetPeriodsResponse(periods));
        }
    }
}
