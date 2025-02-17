using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SocialMedia.Services
{
    public class Authentication : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("user") == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "Controller", "Login" },
            });
            }
            base.OnActionExecuting(context);
        }
    }
}
