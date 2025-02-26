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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ManageAccount()
        {

           

            var user = _context.Users.ToList();


            return View(user);
        }

        [HttpPost]
        public IActionResult ManageAccount(int id, bool isActive, string? searchByName)
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

            var pagedUsers = users.OrderBy(u => u.Name).ToList();

            return View(pagedUsers); 
        }


        [HttpGet] 
        public IActionResult ManagePost()
        {
            // int i = _context.Users.FirstOrDefault(u => u.Id == p.Author);
            // _context.Users.FirstOrDefault( u => u.Id = _context.Users.FirstOrDefault(u => u.Id == p.Author))
            var posts = _context.Posts
                .Select(p => new ManagePost
                {
                    Id = p.Id,
                    Content = p.Contents,
                    CommentCount = _context.Comments.Count(c => c.Post == p.Id),
                    ReactCount = _context.Reactions.Count(c => c.Post == p.Id),
                    Author = _context.Users
                        .Where(u => u.Id == p.Id)
                        .Select(u => u.Name)
                        .FirstOrDefault(),
                    ModifyTime = p.ModifyTime,
                }).ToList();

            return View(posts);
        }

        [HttpPost]
        public IActionResult ManagePost(int? id)
        {
            if (id == null)
            {
                return BadRequest("Post ID is required.");
            }

            try
            {
                var post = _context.Posts
                    .Include(p => p.Comments)
                    .Include(p => p.Reactions)
                    .FirstOrDefault(p => p.Id == id);
                post.Comments.Clear();
                post.Reactions.Clear();
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    _context.SaveChanges();
                }
                return RedirectToAction("ManagePost");
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, "An error occurred while deleting the post.");
            }
        }


        [HttpGet]
        public IActionResult Setting()
        {
            return View();
        }
    }
}
