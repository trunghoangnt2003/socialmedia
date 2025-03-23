using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class GroupController : Controller
    {
        SocialNetworkContext _context;
        private readonly CloudinaryServices _cloudinaryServices;

        public GroupController(SocialNetworkContext context, CloudinaryServices cloudinaryServices)
        {
            _context = context;
            _cloudinaryServices = cloudinaryServices;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var group = _context.Groups
                .FirstOrDefault(g => g.Id == id);

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
            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string groupName, IFormFile avatar)
        {
            string? url = null;
            int userId = int.Parse(HttpContext.Session.GetString("User"));

            if (avatar != null && avatar.Length > 0)
            { 
                url = await _cloudinaryServices.PutImageToCloudinary(avatar);
            }
            var newGroup = new Group {
                Name = groupName,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                Admin = userId,
                Avatar = url,
            };

            _context.Groups.Add(newGroup);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = newGroup.Id });
        }


    }
}
