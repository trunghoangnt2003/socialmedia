using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class PostController : Controller
    {
        private readonly ILogger<PostController> _logger;
        private CloudinaryServices _cloudinaryServices;
        private SocialNetworkContext _socialNetworkContext;

        public PostController(ILogger<PostController> logger, CloudinaryServices cloudinaryServices, SocialNetworkContext socialNetworkContext)
        {
            _logger = logger;
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext = socialNetworkContext;
        }
        public IActionResult PostDetail(int id)
        {
            var post = _socialNetworkContext.Posts.Include(p => p.Reactions).Include(p => p.Resources).Include(p=>p.AuthorNavigation).FirstOrDefault(p => p.Id == id);
            return View(post);
        }
        [HttpPost]
        public IActionResult ToggleLike([FromBody] LikeRequest request)
        {
            var post = _socialNetworkContext.Posts.Find(request.PostId);
            if (post == null) return NotFound();
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userLike = _socialNetworkContext.Reactions.FirstOrDefault(l => l.Post == request.PostId && l.User == userID);

            if (userLike != null)
            {
                _socialNetworkContext.Reactions.Remove(userLike);
            }
            else
            {
                _socialNetworkContext.Reactions.Add(new Reaction { Post = request.PostId, User = userID });
            }

            _socialNetworkContext.SaveChanges();

            return Ok( new { liked = request.Like, likeCount = _socialNetworkContext.Posts.Include(p=>p.Reactions).FirstOrDefault(p=>p.Id == post.Id).Reactions.Count });
        }

        public class LikeRequest
        {
            public int PostId { get; set; }
            public bool Like { get; set; }
        }

    }
}
