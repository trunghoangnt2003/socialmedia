using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;
using System.Linq;
namespace SocialMedia.Controllers
{
    public class GroupController : Controller
    {
        private readonly SocialNetworkContext _context;
        private CloudinaryServices _cloudinaryServices;
        public GroupController(SocialNetworkContext context, CloudinaryServices cloudinaryServices)
        {
            _context = context;
            _cloudinaryServices = cloudinaryServices;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LoadCreatePost(int? groupId)
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userDB = _context.Users.FirstOrDefault(u => u.Id == userID);
            var group = _context.Groups.Find(groupId);
            ViewBag.Group = group;
            return PartialView("Group/_CreatePost", userDB);
        }
        [HttpPost]
        public IActionResult JoinGroup(int groupId)
        {
            string userString = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userString))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để tham gia nhóm." });
            }

            int userId = int.Parse(userString);
            var existingMembership = _context.UserGroups
                .FirstOrDefault(ug => ug.Group == groupId && ug.User == userId);

            if (existingMembership != null)
            {
                return Json(new { success = false, message = "Bạn đã là thành viên của nhóm này." });
            }

            var userGroup = new UserGroup
            {
                Group = groupId,
                User = userId
                // Add other properties like JoinDate if needed
            };

            _context.UserGroups.Add(userGroup);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult LeaveGroup(int groupId)
        {
            string userString = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userString))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để rời nhóm." });
            }

            int userId = int.Parse(userString);
            var membership = _context.UserGroups
                .FirstOrDefault(ug => ug.Group == groupId && ug.User == userId);

            if (membership == null)
            {
                return Json(new { success = false, message = "Bạn không phải là thành viên của nhóm này." });
            }

            _context.UserGroups.Remove(membership);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> UploadAsync(List<IFormFile> media, string content,int groupId)
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            Post post = new Post
            {
                Contents = content,
                ModifyTime = DateTime.Now,
                Author = userID,
                Post1 = null,
                Type = (int?)Types.POST,
                Group = groupId
            };
            _context.Posts.Add(post);
            _context.SaveChanges();
            List<Dictionary<string, string>> resClound = await _cloudinaryServices.PutFilesToCloundinary(media.ToArray());

            if (resClound.Count > 0)
            {
                foreach (Dictionary<string, string> pairs in resClound)
                {
                    var url = pairs["url"];
                    var type = pairs["resource_type"];
                    Resource resource = new Resource
                    {
                        Url = url,
                        Post = post.Id
                    };

                    if (type == "image")
                    {
                        resource.Type = (int?)Types.IMAGE;
                    }
                    else if (type == "video")
                    {
                        var format = pairs["format"];
                        if (format == "mp3") resource.Type = (int?)Types.AUDIO;
                        else resource.Type = (int?)Types.VIDEO;
                    }
                    _context.Resources.Add(resource);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction(nameof(Details), new { id = groupId });
        }
        public IActionResult Details(int id)
        {
            var group = _context.Groups.Include(g=>g.UserGroups).ThenInclude(u=>u.UserNavigation)
            .FirstOrDefault(g => g.Id == id);

            int? userId = int.Parse(HttpContext.Session.GetString("User"));

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var countMember = _context.UserGroups.Count(g => g.Group == id);

            if (group == null) return NotFound();

            var posts = _context.Posts
                .Where(p => p.Group == id)
                .OrderByDescending(p => p.ModifyTime)
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
            var newGroup = new Group
            {
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
