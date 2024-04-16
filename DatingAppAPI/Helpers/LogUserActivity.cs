using DatingAppAPI.Extnsions;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingAppAPI.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContex = await next();
            if(!resultContex.HttpContext.User.Identity.IsAuthenticated) { return; }
            var userId = resultContex.HttpContext.User.getUserId();

            var uow = resultContex.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var user = await uow.UserRepository.GetUserByIdAsync(userId);
            user.LastActive = DateTime.UtcNow;
            await uow.Complete();
        }
    }
}
