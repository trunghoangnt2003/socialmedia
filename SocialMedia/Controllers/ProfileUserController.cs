
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
            var listImages = _contextDb.Posts.Where(img => img.Author == id && img.Type == 5).Include(r => r.Resources)
                            .ToList();
            var listVideos = _contextDb.Posts.Where(img => img.Author == id && img.Type == 6).Include(r => r.Resources)
                            .ToList();
            var getUser = _contextDb.Users.FirstOrDefault(us => us.Id == id);
            if (user == null)
            {
                return NotFound();
            }
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
    }
}