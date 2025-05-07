using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Contracts;
using System;
using System.Threading;
using Microsoft.AspNetCore.Authorization;

namespace CycleApp.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OvulationsController : ControllerBase
    {
        private readonly CycleDbContext _dbContext;

        public OvulationsController(CycleDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet("by-date-range")]
        public async Task<IActionResult> GetOvulationsByDate(
        [FromQuery] DateTime start_date,
        [FromQuery] DateTime end_date,
        [FromQuery] Guid user_id,
        CancellationToken ct)
                {
                    try
                    {
                        var ovulations = await _dbContext.Ovulations
                            .Where(p => p.UserId == user_id && p.StartDate >= start_date && p.EndDate <= end_date)
                            .Select(p => new OvulationDto(
                                p.OvulationId,
                                p.UserId,
                                p.StartDate,
                                p.EndDate,
                                p.IsPredicted,
                                p.Symptoms
                            ))
                            .ToListAsync(ct);

                        return Ok(ovulations);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
         }
        // POST: Рассчитать дни овуляции
        [HttpPost("calculate")] 
        public async Task<IActionResult> CalculateOvulation([FromBody] CalculateOvulationRequest request, CancellationToken ct)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == request.UserId, ct);

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Пример расчета: овуляция обычно происходит за 14 дней до конца цикла
                var ovulationDate = request.StartDate.AddDays(user.CycleLength - 14);

                return Ok(new
                {
                    success = true,
                    ovulationDate = ovulationDate.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}