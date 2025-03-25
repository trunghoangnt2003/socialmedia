using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class PostController : Controller
    {
        private readonly ILogger<PostController> _logger;
        private readonly CloudinaryServices _cloudinaryServices;
        private readonly SocialNetworkContext _socialNetworkContext;
        private readonly IHubContext<SignalRService> _signalRHubContext;

        public PostController(
            IHubContext<SignalRService> signalRHubContext,
            ILogger<PostController> logger,
            CloudinaryServices cloudinaryServices,
            SocialNetworkContext socialNetworkContext)
        {
            _logger = logger;
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext = socialNetworkContext;
            _signalRHubContext = signalRHubContext;
        }

        public IActionResult PostDetail(int id)
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userDB = _socialNetworkContext.Users.FirstOrDefault(u => u.Id == userID);
            ViewBag.User = userDB;
            var post = _socialNetworkContext.Posts
                .Include(p => p.Reactions)
                .Include(p => p.Resources)
                .Include(p => p.Comments)
                .ThenInclude(c=>c.AuthorNavigation)
                .Include(p => p.AuthorNavigation)
                .FirstOrDefault(p => p.Id == id);
            return View(post);
        }

        public class CommentRequest
        {
            public string content { get; set; }
            public int postId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            var post = _socialNetworkContext.Posts.Find(request.postId);
            if (post == null) return NotFound();

            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            User user1 = _socialNetworkContext.Users.Find(userID);

            Comment comment = new Comment
            {
                ModifyTime = DateTime.Now,
                Post = request.postId,
                Contents = request.content,
                Author = userID
            };
            _socialNetworkContext.Comments.Add(comment);
            await _socialNetworkContext.SaveChangesAsync();

            // Save notification to database
            if (post.Author != userID)
            {
                var notification = new Notification
                {
                    Sender = userID,
                    Receiver = post.Author,
                    SendTime = DateTime.Now,
                    Message = $"đã bình luận bài viết của bạn",
                    Status = 1 
                };
                _socialNetworkContext.Notifications.Add(notification);
                await _socialNetworkContext.SaveChangesAsync();


                if (SignalRService.ConnectedUsers.TryGetValue(post.Author.ToString(), out string receiverConnectionId))
                {
                    await _signalRHubContext.Clients.Client(receiverConnectionId).SendAsync("ReceiveNotification",
                        user1.Name,
                        notification.Message,
                        notification.SendTime?.ToString("o"),post.Id,notification.Id);
                }
            }

            return Ok(new
            {
                success = true,
                avatar = user1.Avatar,
                commentName = user1.Name,
                createAt = comment.ModifyTime,
                content = request.content
            });
        }

        public class LikeRequest
        {
            public int PostId { get; set; }
            public bool Like { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
        {
            var post = _socialNetworkContext.Posts.Find(request.PostId);
            if (post == null) return NotFound();

            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var userLike = _socialNetworkContext.Reactions
                .FirstOrDefault(l => l.Post == request.PostId && l.User == userID);

            bool isLiked = false;
            if (userLike != null)
            {
                _socialNetworkContext.Reactions.Remove(userLike);
            }
            else
            {
                _socialNetworkContext.Reactions.Add(new Reaction { Post = request.PostId, User = userID });
                isLiked = true;
            }

            await _socialNetworkContext.SaveChangesAsync();


            if (isLiked && post.Author != userID)
            {
                var sender = _socialNetworkContext.Users.Find(userID)?.Name ?? "Unknown";
                var notification = new Notification
                {
                    Sender = userID,
                    Receiver = post.Author,
                    SendTime = DateTime.Now,
                    Message = $"đã thích bài viết của bạn",
                    Status = 1
                };
                _socialNetworkContext.Notifications.Add(notification);
                await _socialNetworkContext.SaveChangesAsync();

                if (SignalRService.ConnectedUsers.TryGetValue(post.Author.ToString(), out string receiverConnectionId))
                {
                    await _signalRHubContext.Clients.Client(receiverConnectionId).SendAsync("ReceiveNotification",
                        sender,
                        notification.Message,
                        notification.SendTime?.ToString("o"),
                        post.Id,
                        notification.Id);
                }
            }

            var likeCount = _socialNetworkContext.Posts
                .Include(p => p.Reactions)
                .FirstOrDefault(p => p.Id == request.PostId)?.Reactions.Count ?? 0;

            return Ok(new { liked = isLiked, likeCount });
        }
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = _socialNetworkContext.Notifications.Find(notificationId);
            if (notification == null) return NotFound();

            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            if (notification.Receiver != userID) return Unauthorized();

            notification.Status = 3; // Đã đọc
            _socialNetworkContext.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var notifications = _socialNetworkContext.Notifications
                .Where(n => n.Receiver == userID && n.Status == 1);

            foreach (var notification in notifications)
            {
                notification.Status = 3;
            }
            _socialNetworkContext.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            string user = HttpContext.Session.GetString("User");
            int userID = int.Parse(user);
            var notifications = _socialNetworkContext.Notifications.Include(n=>n.SenderNavigation)
                .Where(n => n.Receiver == userID)
                .OrderByDescending(n => n.SendTime)
                .Take(5)
                .Select(n => new
                {
                    n.Id,
                    Sender = n.SenderNavigation.Name,
                    n.Message,
                    n.SendTime,
                    n.Status
                })
                .ToList();
            return Json(notifications);
        }
    }
}