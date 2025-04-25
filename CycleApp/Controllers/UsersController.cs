using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.Contracts;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Controllers;
using System.Threading;

namespace CycleApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CycleDbContext _dbContext;

        public UsersController(CycleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Получить данные пользователя по ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId, CancellationToken ct)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                var userDto = new UserDto(
                    user.UserId,
                    user.Email,
                    user.CycleLength,
                    user.PeriodLength,
                    user.RemindPeriod,
                    user.RemindOvulation
                );

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // PUT: Обновить данные пользователя
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Обновляем поля пользователя
                user.CycleLength = request.cycleLength ?? user.CycleLength;
                user.PeriodLength = request.periodLength ?? user.PeriodLength;
                user.RemindPeriod = request.remindPeriod ?? user.RemindPeriod;
                user.RemindOvulation = request.remindOvulation ?? user.RemindOvulation;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync(ct);

                return Ok(new { success = true, message = "User updated successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { error = "Database error", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}