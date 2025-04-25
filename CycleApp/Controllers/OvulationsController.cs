using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Contracts;
using System;
using System.Threading;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvulationsController : ControllerBase
    {
        private readonly CycleDbContext _dbContext;

        public OvulationsController(CycleDbContext dbContext)
        {
            _dbContext = dbContext;
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