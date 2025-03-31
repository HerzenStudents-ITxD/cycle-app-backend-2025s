using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;

namespace CycleApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EntriesController : ControllerBase
{
    private readonly CycleDbContext _dbContext;

    public EntriesController(CycleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEntryRequest request, CancellationToken ct)
    {
        var entry = new Entry(
            request.user_id,
            request.Date,
            request.PeriodStarted,
            request.PeriodEnded,
            request.Note,
            request.Heaviness,
            request.Symptoms,
            request.Sex,
            request.Mood,
            request.Discharges
        );

        await _dbContext.Entries.AddAsync(entry, ct);
        await _dbContext.SaveChangesAsync(ct);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetByUser(Guid user_id, CancellationToken ct)
    {
        var entries = await _dbContext.Entries
            .Where(e => e.user_id == user_id)
            .Select(e => new EntryDto(
                e.entry_id,
                e.user_id,
                e.Date,
                e.PeriodStarted,
                e.PeriodEnded,
                e.Note,
                e.Heaviness,
                e.Symptoms,
                e.Sex,
                e.Mood,
                e.Discharges
            ))
            .ToListAsync(ct);

        return Ok(new GetEntriesResponse(entries));
    }
}