using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SocialMedia.Models;
using SocialMedia.Services;
using System.Diagnostics;
using System.Drawing.Printing;

namespace SocialMedia.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CloudinaryServices _cloudinaryServices;
        private SocialNetworkContext _socialNetworkContext;

        public HomeController(ILogger<HomeController> logger, CloudinaryServices cloudinaryServices, SocialNetworkContext socialNetworkContext)
        {
            _logger = logger;
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext = socialNetworkContext;
        }
        public IActionResult LoadMorePosts(int page = 1)
        {
            int pageSize = 5;
            var posts = _socialNetworkContext.Posts
                                .OrderByDescending(p => p.ModifyTime)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return PartialView("Home/_ViewPost", posts);
        }

        public IActionResult LoadCreatePost()
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userDB = _socialNetworkContext.Users.FirstOrDefault(u => u.Id == userID);
            return PartialView("Home/_CreatePost", userDB);
        }
        public IActionResult Index()
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return View();

            int userID = int.Parse(user);
            var listFriends = _socialNetworkContext.Friends.Where(f => f.User == userID).Include(f => f.Friend1Navigation);
            var userDB = _socialNetworkContext.Users.FirstOrDefault(u => u.Id == userID);
            ViewBag.Friends = listFriends.ToList();
            ViewBag.User = userDB;
            var suggestedFriends = _socialNetworkContext.Users
        .Where(u => u.Id != userID && // Exclude the current user
                    !_socialNetworkContext.Friends
                        .Any(f => (f.User == userID && f.Friend1 == u.Id) ||
                                  (f.Friend1 == userID && f.User == u.Id))) // Exclude existing friends
        .Take(4) // Limit to 4 suggestions
        .ToList();
            ViewBag.SuggestedFriends = suggestedFriends;
            var friendIds = _socialNetworkContext.Friends
                .Where(f => f.User == userID && f.Status == 2) // Assuming Status == 1 means "accepted"
                .Select(f => f.Friend1)
                .ToList();

            var posts = _socialNetworkContext.Posts
        .Include(p => p.Resources)
        .Include(p => p.Reactions)
        .Include(p => p.Comments)
        .Include(p => p.GroupNavigation)
        .Where(p => (p.Group == null && (friendIds.Contains(p.Author) || p.Author == userID)) || // Non-group posts by friends or user
                    (p.Group != null && _socialNetworkContext.UserGroups
                        .Any(ug => ug.Group == p.Group && ug.User == userID))) // Group posts only if user is a member
        .OrderByDescending(p => p.ModifyTime)
        .ToList();
            ViewBag.Posts = posts;
            var suggestedGroups = _socialNetworkContext.Groups
            .Where(g => !_socialNetworkContext.UserGroups
                .Any(ug => ug.Group == g.Id && ug.User == userID))
            .Take(4) 
            .ToList();
            ViewBag.SuggestedGroups = suggestedGroups;
            var userGroups = _socialNetworkContext.UserGroups
        .Where(ug => ug.User == userID)
        .Include(ug => ug.GroupNavigation)
        .Select(ug => ug.GroupNavigation)
        .ToList();
            ViewBag.UserGroups = userGroups;
            return View();
        }
        public IActionResult PostDetail(int id)
        {
            var post = _socialNetworkContext.Posts.Include(p => p.Reactions).Include(p => p.Resources).FirstOrDefault(p => p.Id == id);
            return View(post);
        }
        [HttpPost]
        public async Task<IActionResult> UploadAsync(List<IFormFile> media, string content)
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            Post post = new Post
            {
                Contents = content,
                ModifyTime = DateTime.Now,
                Author = userID,
                Post1 = null,
                Type = (int?)Types.POST
            };
            _socialNetworkContext.Posts.Add(post);
            _socialNetworkContext.SaveChanges();
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
                    _socialNetworkContext.Resources.Add(resource);
                    _socialNetworkContext.SaveChanges();
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Privacy(string id)
        {
            var session = HttpContext.Session;
            HttpContext.Session.SetString("User", id);
            User user = _socialNetworkContext.Users.FirstOrDefault(u => u.Id == int.Parse(id));
            ViewBag.User = user;
            var userJson = JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            session.SetString("UserFull", userJson);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
