using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CycleApp.Authorization
{
    public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResourceOwnerHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            // Получаем ClaimsPrincipal из контекста
            ClaimsPrincipal user = context.User;
            if (!user.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // Получаем userId из маршрута
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.Request.RouteValues.TryGetValue("userId", out var routeUserId))
            {
                // Получаем ID текущего пользователя из клаймов
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim != null && 
                    int.TryParse(userIdClaim.Value, out int authenticatedUserId) && 
                    int.TryParse(routeUserId.ToString(), out int requestedUserId))
                {
                    // Проверяем, совпадают ли ID
                    if (authenticatedUserId == requestedUserId)
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
