using Microsoft.AspNetCore.Mvc.Filters;
using Network.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Network.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var repository = resultContext.HttpContext.RequestServices.GetService<INetworkRepository>();
            var user = await repository.GetUser(userId, true);
            user.LastActive = DateTime.Now;
            await repository.SaveAll();
        }
    }
}
