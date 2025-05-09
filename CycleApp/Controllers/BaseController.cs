using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CycleApp.DataAccess;
using CycleApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CycleApp.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly CycleDbContext DbContext;
        protected readonly ILogger<BaseController> Logger;

        protected BaseController(CycleDbContext dbContext, ILogger<BaseController> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        /// <summary>
        /// Reads the NameIdentifier claim, parses it as a Guid,
        /// and loads the corresponding User entity from the database.
        /// Returns null if the claim is missing/invalid or the user does not exist.
        /// </summary>
        protected async Task<User?> GetUserFromClaimsAsync(CancellationToken cancellationToken = default)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                Logger.LogWarning("NameIdentifier claim is missing from the token");
                return null;
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                Logger.LogWarning("NameIdentifier claim is not a valid GUID: {UserIdString}", userIdString);
                return null;
            }

            var user = await DbContext.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            if (user == null)
            {
                Logger.LogWarning("User not found for ID: {UserId}", userId);
            }

            return user;
        }
    }
}