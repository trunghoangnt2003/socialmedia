using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;
using System.Diagnostics;

namespace SocialMedia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CloudinaryServices _cloudinaryServices;
        private SocialNetworkContext _socialNetworkContext;

        public HomeController(ILogger<HomeController> logger, CloudinaryServices cloudinaryServices, SocialNetworkContext socialNetworkContext)
        {
            _logger = logger;
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext=socialNetworkContext;
        }

        public IActionResult Index()
        {
            var listFriends = _socialNetworkContext.Friends.Where(f => f.User == 1).Include(f => f.Friend1Navigation);
            ViewBag.Friends = listFriends.ToList();
            return View();
        }
        [HttpPost]
        public  IActionResult Index(IFormFile[] images)
        {
            //if (images == null)return View();
            //var res =  _cloudinaryServices.PutFilesToCloundinary(images);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
