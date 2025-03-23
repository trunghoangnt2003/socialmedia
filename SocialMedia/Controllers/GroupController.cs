using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    public class GroupController : Controller
    {
        private readonly SocialNetworkContext _context;

        public GroupController(SocialNetworkContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var group = _context.Groups
            .FirstOrDefault(g => g.Id == id);

            int? userId = int.Parse(HttpContext.Session.GetString("User"));

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var countMember = _context.UserGroups.Count(g => g.Group == id);

            if (group == null) return NotFound();

            var posts = _context.Posts
                .Where(p => p.Group == id)
                .Include(p => p.AuthorNavigation)
                .Include(p => p.Resources)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .ToList();

            ViewBag.Group = group;
            ViewBag.Member = countMember;
            ViewBag.User = user;
            return View(posts);
        }
    }
}
