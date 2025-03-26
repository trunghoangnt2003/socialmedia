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
        .Where(p => (p.Group == null &&p.Type == 1&& (friendIds.Contains(p.Author) || p.Author == userID)) || // Non-group posts by friends or user
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
            var stories = _socialNetworkContext.Posts
        .Include(p => p.AuthorNavigation)
        .Include(p => p.Resources)
        .Where(p => p.Type == 2 && (friendIds.Contains(p.Author) || p.Author == userID))
        .OrderByDescending(p => p.ModifyTime)
        .ToList();
            ViewBag.Stories = stories;
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
        [HttpPost]
        public IActionResult AddFriend(int friendId)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thêm bạn.";
                return RedirectToAction("Index");
            }

            int userID = int.Parse(user);

            // Check if friendship already exists
            if (_socialNetworkContext.Friends.Any(f => (f.User == userID && f.Friend1 == friendId) ||
                                                       (f.User == friendId && f.Friend1 == userID)))
            {
                TempData["ErrorMessage"] = "Bạn đã là bạn bè hoặc đã gửi lời mời.";
                return RedirectToAction("Index");
            }

            // Add friend request
            var friendRequest = new Friend
            {
                User = userID,
                Friend1 = friendId,
                Status = 1, // 1 = pending
                SendTime = DateTime.Now
            };
            _socialNetworkContext.Friends.Add(friendRequest);
            _socialNetworkContext.SaveChanges();

            TempData["SuccessMessage"] = "Đã gửi lời mời kết bạn thành công!";
            return RedirectToAction("Index");
        }
        public IActionResult FriendRequests()
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return RedirectToAction("Login");

            int userID = int.Parse(user);

            // Fetch pending friend requests where the current user is the recipient (Friend1)
            var friendRequests = _socialNetworkContext.Friends
                .Where(f => f.Friend1 == userID && f.Status == 1)
                .Include(f => f.UserNavigation)
                .ToList();

            // Calculate mutual friends for each request
            var friendRequestViewModels = friendRequests.Select(request =>
            {
                var mutualFriendsCount = _socialNetworkContext.Friends
                    .Where(f => f.User == request.User && f.Status == 2)
                    .Count(f => _socialNetworkContext.Friends
                        .Any(f2 => f2.User == userID && f2.Friend1 == f.Friend1 && f2.Status == 2));

                return new FriendRequestViewModel
                {
                    FriendRequest = request,
                    MutualFriendsCount = mutualFriendsCount
                };
            }).ToList();

            return View(friendRequestViewModels);
        }
        [HttpPost]
        public IActionResult AcceptFriendRequest(int id)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để chấp nhận lời mời.";
                return RedirectToAction("FriendRequests");
            }

            int userID = int.Parse(user);
            var friendRequest = _socialNetworkContext.Friends
                .FirstOrDefault(f => f.Id == id && f.Friend1 == userID && f.Status == 1);

            if (friendRequest == null)
            {
                TempData["ErrorMessage"] = "Lời mời kết bạn không tồn tại.";
                return RedirectToAction("FriendRequests");
            }

            friendRequest.Status = 2; // 2 = accepted
            _socialNetworkContext.SaveChanges();

            TempData["SuccessMessage"] = "Đã chấp nhận lời mời kết bạn!";
            return RedirectToAction("FriendRequests");
        }

        // Action to cancel a friend request
        [HttpPost]
        public IActionResult CancelFriendRequest(int id)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để hủy lời mời.";
                return RedirectToAction("FriendRequests");
            }

            int userID = int.Parse(user);
            var friendRequest = _socialNetworkContext.Friends
                .FirstOrDefault(f => f.Id == id && f.Friend1 == userID && f.Status == 1);

            if (friendRequest == null)
            {
                TempData["ErrorMessage"] = "Lời mời kết bạn không tồn tại.";
                return RedirectToAction("FriendRequests");
            }

            _socialNetworkContext.Friends.Remove(friendRequest);
            _socialNetworkContext.SaveChanges();

            TempData["SuccessMessage"] = "Đã hủy lời mời kết bạn!";
            return RedirectToAction("FriendRequests");
        }
        // Load the story creation modal
        public IActionResult LoadCreateStory()
        {
            return PartialView("_CreateStory");
        }

        // Handle story creation
        [HttpPost]
        public IActionResult CreateStory(string contents, IFormFile image)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để tạo tin.";
                return RedirectToAction("Index");
            }

            int userID = int.Parse(user);

            // Create a new story (Post with Type = 2)
            var story = new Post
            {
                Contents = contents,
                ModifyTime = DateTime.Now,
                Author = userID,
                Type = 2 // Story type
            };
            _socialNetworkContext.Posts.Add(story);
            _socialNetworkContext.SaveChanges();

            // Handle image upload (if provided)
            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                // Save the image as a Resource linked to the story
                var resource = new Resource
                {
                    //Post = story.Id,
                    // = "/uploads/" + fileName,
                    //Type = "image"
                };
                _socialNetworkContext.Resources.Add(resource);
                _socialNetworkContext.SaveChanges();
            }

            TempData["SuccessMessage"] = "Đã tạo tin thành công!";
            return RedirectToAction("Index");
        }
        public IActionResult LoadStoryViewer(int storyId)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(user))
                {
                    return Content("Bạn cần đăng nhập để xem tin.");
                }

                if (!int.TryParse(user, out int userID))
                {
                    return Content("ID người dùng không hợp lệ.");
                }

                // Fetch all stories from the user and their friends
                var friendIds = _socialNetworkContext.Friends
                    .Where(f => f.User == userID && f.Status == 1)
                    .Select(f => f.Friend1)
                    .ToList();

                var stories = _socialNetworkContext.Posts
                    .Include(p => p.AuthorNavigation)
                    .Include(p => p.Resources)
                    .Where(p => p.Type == 2 && (friendIds.Contains(p.Author) || p.Author == userID))
                    .OrderByDescending(p => p.ModifyTime)
                    .ToList();

                // Find the index of the selected story
                var selectedStoryIndex = stories.FindIndex(s => s.Id == storyId);
                if (selectedStoryIndex == -1)
                {
                    return Content("Không tìm thấy tin.");
                }

                // Map to DTO to avoid serialization issues
                var storyDtos = stories.Select(s => new StoryDto
                {
                    Id = s.Id,
                    Contents = s.Contents,
                    ModifyTime = s.ModifyTime,
                    AuthorName = s.AuthorNavigation?.Name,
                    ResourceLinks = s.Resources?.Select(r => r.Url).ToList() ?? new List<string>()
                }).ToList();

                ViewBag.Stories = storyDtos;
                ViewBag.SelectedStoryIndex = selectedStoryIndex;

                return PartialView("_StoryViewer");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadStoryViewer: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while loading the story viewer: {ex.Message}");
            }
        }

        [HttpGet]
        public JsonResult GetResults(string query)
        {
            var users = _socialNetworkContext.Users
                .Where(u => u.Name.Contains(query))
                .Select(u => new { u.Id, u.Name, u.Avatar })
                .ToList();

            var groups = _socialNetworkContext.Groups
                .Where(g => g.Name.Contains(query))
                .Select(g => new { g.Id, g.Name, g.Avatar })
                .ToList();

            return Json(new { users, groups });
        }


    }
    public class StoryDto
    {
        public int Id { get; set; }
        public string Contents { get; set; }
        public DateTime? ModifyTime { get; set; }
        public string AuthorName { get; set; }
        public List<string> ResourceLinks { get; set; }
    }
    public class FriendRequestViewModel
    {
        public Friend FriendRequest { get; set; }
        public int MutualFriendsCount { get; set; }
    }
}
