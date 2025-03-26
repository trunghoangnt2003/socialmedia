
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    [Authorize(Policy = "UserOnly")]
    public class ProfileUserController : Controller
    {
        private readonly SocialNetworkContext _contextDb;
        private readonly CloudinaryServices _cloudinaryServices;
        public ProfileUserController(SocialNetworkContext context, CloudinaryServices cloudinaryServices)
        {
            _contextDb = context;
            _cloudinaryServices = cloudinaryServices;
        }
        public IActionResult Index(int? id)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return View();

            var listFriends = _contextDb.Friends.Where(f => f.User == id).Include(f => f.Friend1Navigation);
           
            var listImages = _contextDb.Posts
            .Where(p => p.Author == id) 
            .Include(p => p.Resources) 
            .SelectMany(p => p.Resources) 
            .Where(r => r.Type == 5) 
            .ToList();


            var listVideos = _contextDb.Posts
            .Where(p => p.Author == id)
            .Include(p => p.Resources)
            .SelectMany(p => p.Resources)
            .Where(r => r.Type == 6)
            .ToList();
            var getUser = _contextDb.Users.FirstOrDefault(us => us.Id == id);
            if (user == null)
            {
                return NotFound();
            }
     //       string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return View();

            int userID = int.Parse(user);
     //       var listFriends = _socialNetworkContext.Friends.Where(f => f.User == userID).Include(f => f.Friend1Navigation);
            var userDB = _contextDb.Users.FirstOrDefault(u => u.Id == userID);
            var AccountFriends = _contextDb.Friends.Where(f => f.User == userID).Include(f => f.Friend1Navigation);
            ViewBag.Friends = listFriends.ToList();
            ViewBag.User = userDB;
            var posts = _contextDb.Posts.Include(p => p.Resources).Include(p => p.Reactions).Include(p => p.Comments)
                .Where(p => p.Author == id && p.Group == null)
                                .OrderByDescending(p => p.ModifyTime)
                                .ToList();

           var isFriend = AccountFriends.FirstOrDefault(f => f.Friend1 == id);

            if (isFriend != null && isFriend.Status == 1 )
            {
                ViewData["isFriend"] = "Send";
            }else if (isFriend != null && isFriend.Status == 2)
            {
                ViewData["isFriend"] = "Friend";
            }
            else
            {
                ViewData["isFriend"] = "None";
            }
            
            
            ViewBag.Posts = posts;
            ViewBag.Friends = listFriends.ToList();
            ViewBag.listImages = listImages;
            ViewBag.listVideos = listVideos;
            ViewBag.user = getUser;
            return View(getUser);
        }

        public async Task<IActionResult> EditProfile(string Name, DateOnly Dob, string Address, IFormFile[] avatarEditInput)
        {
            var userId = HttpContext.Session.GetString("User");
            if(userId == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var user = _contextDb.Users.Find(int.Parse(userId));
            if(user == null)
            {
                return NotFound(); ;
            }
            List<Dictionary<string, string>> resClound = await _cloudinaryServices.PutFilesToCloundinary(avatarEditInput);

            if (resClound.Count > 0)
            {
                foreach (Dictionary<string, string> pairs in resClound)
                {
                    var url = pairs["url"];
                    var type = pairs["resource_type"];
                    user.Avatar = url;
                    _contextDb.Update(user);
                    _contextDb.SaveChanges();
                }
            }
            user.Name = Name;
            user.Dob = Dob;
            user.Address = Address;

            _contextDb.SaveChanges();
            var userJson = JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            HttpContext.Session.SetString("UserFull", userJson);
            return RedirectToAction("Index", new {id = user.Id});
        }


        public IActionResult SearchFriends(string searchTerm, int? id)
        {
            if (id == null) return BadRequest("User ID is required");

            var friends = _contextDb.Friends
                .Where(f => f.User == id && (string.IsNullOrEmpty(searchTerm) || f.Friend1Navigation.Name.Contains(searchTerm)))
                .Include(f => f.Friend1Navigation)
                .Select(f => new
                {
                    Avatar = f.Friend1Navigation.Avatar,
                    Name = f.Friend1Navigation.Name
                })
                .ToList();

            return Json(friends);
        }

        public async Task<IActionResult> EditAvatar(IFormFile[] avatarInput)
        {
            var userId = HttpContext.Session.GetString("User");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var user = _contextDb.Users.Find(int.Parse(userId));
            if (user == null)
            {
                return NotFound(); ;
            }
            List<Dictionary<string, string>> resClound = await _cloudinaryServices.PutFilesToCloundinary(avatarInput);

            if (resClound.Count > 0)
            {
                foreach (Dictionary<string, string> pairs in resClound)
                {
                    var url = pairs["url"];
                    var type = pairs["resource_type"];
                    user.Avatar = url;
                    _contextDb.Update(user);
                    _contextDb.SaveChanges();
                }
            }

            var userJson = JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            HttpContext.Session.SetString("UserFull", userJson);

            return RedirectToAction("Index", new { id = user.Id });
        }

        public IActionResult LoadCreatePost()
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userDB = _contextDb.Users.FirstOrDefault(u => u.Id == userID);
            return PartialView("Home/_CreatePost", userDB);
        }

    
        public IActionResult AddFriend(int id)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thêm bạn.";
                return RedirectToAction("Index");
            }

            int userID = int.Parse(user);

            // Check if friendship already exists
            if (_contextDb.Friends.Any(f => (f.User == userID && f.Friend1 == id) ||
                                                       (f.User == id && f.Friend1 == userID)))
            {
                TempData["ErrorMessage"] = "Bạn đã là bạn bè hoặc đã gửi lời mời.";
                return RedirectToAction("Index");
            }

            // Add friend request
            var friendRequest = new Friend
            {
                User = userID,
                Friend1 = id,
                Status = 1, // 1 = pending
                SendTime = DateTime.Now
            };
            _contextDb.Friends.Add(friendRequest);
            _contextDb.SaveChanges();

            TempData["SuccessMessage"] = "Đã gửi lời mời kết bạn thành công!";

            return RedirectToAction("Index", new {id = id });
        }
    }
}