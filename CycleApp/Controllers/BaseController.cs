using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace CycleApp.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly CycleDbContext DbContext;

        protected BaseController(CycleDbContext dbContext)
        {
            DbContext = dbContext;
        }

        /// <summary>
        /// Reads the NameIdentifier claim, parses it as a Guid,
        /// and loads the corresponding User entity from the database.
        /// Returns null if the claim is missing/invalid or the user does not exist.
        /// </summary>
        protected async Task<User?> GetUserFromClaimsAsync(CancellationToken cancellationToken = default)
        {
            // 1) Grab the claim
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return null;

            // 2) Fetch from DB
            return await DbContext.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }
    }
}