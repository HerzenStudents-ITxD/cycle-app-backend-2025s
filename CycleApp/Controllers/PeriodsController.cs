using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;

namespace CycleApp.Controllers;

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
        var period = new Period(request.user_id, request.StartDate, request.EndDate, request.IsActive);

        await _dbContext.Periods.AddAsync(period, ct);
        await _dbContext.SaveChangesAsync(ct);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetByUser(Guid user_id, CancellationToken ct)
    {
        var periods = await _dbContext.Periods
            .Where(p => p.user_id == user_id)
            .Select(p => new PeriodDto(p.period_id, p.user_id, p.StartDate, p.EndDate, p.IsActive))
            .ToListAsync(ct);

        return Ok(new GetPeriodsResponse(periods));
    }
}