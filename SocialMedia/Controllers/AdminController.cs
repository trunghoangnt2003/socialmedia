using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    public class AdminController : Controller
    {

        SocialNetworkContext _context;

        public AdminController(SocialNetworkContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DashBoard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null) 
            {
                return RedirectToAction("DashBoard");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password, please again!";
                return View();
            }

            HttpContext.Session.SetString("admin", user.Name);

        }
    }
}
