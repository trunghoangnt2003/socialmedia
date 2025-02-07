using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                HttpContext.Session.SetString("adminId", user.Id.ToString());
                return RedirectToAction("DashBoard");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password, please again!";
                return View();
            }

            

        }

        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ManageAccount()
        {
            return View(_context.Users.ToList());
        }

        [HttpPost]
        public IActionResult ManageAccount(int id, bool isActive)
        {
            var user = _context.Users.FirstOrDefault( u =>  u.Id == id);
            if (user != null)
            {
                user.IsActive = isActive;
                _context.SaveChanges();
            }
            return View();
        }

        [HttpGet] 
        public IActionResult ManagePost()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Setting()
        {
            return View();
        }
    }
}
