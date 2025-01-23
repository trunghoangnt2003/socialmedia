using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;
using SocialMedia.Services;
using System.Diagnostics;

namespace SocialMedia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CloudinaryServices _cloudinaryServices;

        public HomeController(ILogger<HomeController> logger, CloudinaryServices cloudinaryServices)
        {
            _logger = logger;
            _cloudinaryServices = cloudinaryServices;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile[] images)
        {
            if (images == null)return View();
            var res = await _cloudinaryServices.PutFilesToCloundinary(images);
            return View(res);
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
