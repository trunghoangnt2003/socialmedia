
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;

namespace SocialMedia.Controllers
{
    [Authorize(Policy = "UserOnly")]
    public class ProfileUserController : Controller
    {
        private readonly SocialNetworkContext _contextDb;
        public ProfileUserController(SocialNetworkContext context)
        {
            _contextDb = context;
        }
        public IActionResult Index(int? id)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return View();

            var listFriends = _contextDb.Friends.Where(f => f.User == id).Include(f => f.Friend1Navigation);

            var getUser = _contextDb.Users.FirstOrDefault(us => us.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Friends = listFriends.ToList();
            ViewBag.user = getUser;
            return View(getUser);
        }

    }
}