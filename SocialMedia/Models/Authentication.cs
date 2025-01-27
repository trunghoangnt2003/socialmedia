using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SocialMedia.Models
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
				{ "Action", "Login" }
			});
			}
			base.OnActionExecuting(context);
		}
	}
}
