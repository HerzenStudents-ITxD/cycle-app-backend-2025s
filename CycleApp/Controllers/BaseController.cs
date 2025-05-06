using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CycleApp.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Получает ID пользователя из токена
        /// </summary>
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Проверяет, имеет ли текущий пользователь доступ к ресурсу с указанным userId
        /// </summary>
        protected bool IsResourceOwner(int resourceUserId)
        {
            var currentUserId = GetUserId();
            return currentUserId > 0 && currentUserId == resourceUserId;
        }
        
        /// <summary>
        /// Проверяет владение ресурсом и возвращает соответствующий результат
        /// </summary>
        protected IActionResult ForbiddenIfNotResourceOwner(int resourceUserId)
        {
            if (!IsResourceOwner(resourceUserId))
            {
                return Forbid();
            }
            
            return null;
        }
    }
}
