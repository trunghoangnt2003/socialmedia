using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;
using System.Security.Claims;
using System.Text;
using SocialMedia.Services;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace SocialMedia.Controllers
{
	public class LoginController : Controller
	{
		private readonly SocialNetworkContext _context;
        private readonly MD5CryptoService _contextCrypt;

        public LoginController(SocialNetworkContext context, MD5CryptoService _contextCryptT)
		{
			_context = context;
			_contextCrypt = _contextCryptT;
		}

		public IActionResult Index()
		{
			if (HttpContext.Session.GetString("User") == null)
			{
                return View();
			}
			else
			{
				return RedirectToAction("Privacy", "Home");
			}
		}
		[HttpPost]
		public async Task<IActionResult> Index(string usename, string password)
		{
            ClaimsIdentity identity = null;
			bool isAuthenticate = false;
			string encryptedInputPassword = _contextCrypt.Encrypt(password, true);
			var account = _context.Users.FirstOrDefault(a => a.Email == usename && a.Password == encryptedInputPassword);
			if (account != null && account.IsActive == true)
			{
                var userJson = JsonConvert.SerializeObject(account, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                HttpContext.Session.SetString("UserFull", userJson);
                HttpContext.Session.SetString("User", account.Id.ToString());
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Email),
                    new Claim("UserId", account.Id.ToString()),
                    new Claim(ClaimTypes.Role, account.Role == 1 ? "Admin" : "User") 
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                if (account.Role == 1) 
                {
                    return RedirectToAction("Index", "Test"); 
                }
                else 
                {
                    return RedirectToAction("Index", "Home"); 
                }

			}
			else if(account == null)
			{
				ViewBag.ErrorMessage = "Username or password not true!";
			} else if(account.IsActive == false)
			{
				 ViewBag.ErrorMessage = "Account is not active!";
			}

            return View();
		}

		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Login");
		}

    }
}
