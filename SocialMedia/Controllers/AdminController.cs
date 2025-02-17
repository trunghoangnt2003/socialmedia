using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using PagedList;

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
                return RedirectToAction("Home");
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
        public IActionResult ManageAccount(int? page)
        {

            int pageSize = 4;
            int currentPage = page ?? 1;

            var user = _context.Users.ToPagedList(currentPage, pageSize);

            ViewBag.CurrentPage = currentPage;


            return View(user);
        }

        [HttpPost]
        public IActionResult ManageAccount(int id, bool isActive, int? page, string? searchByName)
        {
            // Toggle account status
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsActive = isActive;
                _context.SaveChanges();
            }

            // Handle search functionality
            List<User> users = _context.Users.ToList();

            if (!string.IsNullOrEmpty(searchByName))
            {
                // Case-insensitive partial match
                users = users.Where(u => u.Name.Contains(searchByName)).ToList();
            }

            // Pagination (if needed)
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var pagedUsers = users.OrderBy(u => u.Name).ToPagedList(pageNumber, pageSize);

            return View(pagedUsers); 
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
