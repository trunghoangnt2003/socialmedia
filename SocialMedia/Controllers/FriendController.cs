using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class FriendController : Controller
    {
        SocialNetworkContext _context;
        SignalRService _signalR;

        public FriendController(SocialNetworkContext context, SignalRService signalR)
        {
            _context = context;
            _signalR = signalR;
        }

        public IActionResult Add(int id)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                int userID = int.Parse(user);
                var account = _context.Users.FirstOrDefault(a => a.Id == userID);

                var friendRequest = new Friend
                {
                    User = userID,
                    Friend1 = id,
                    Status = 1, // 1 = Send
                    SendTime = DateTime.Now
                };
                _context.Friends.Add(friendRequest);

                var notification = new Notification
                {
                    Sender = userID,
                    Receiver = id,
                    SendTime = DateTime.Now,
                    Message = $"đã gủi cho bạn lời mời kết bạn",
                    Status = 1
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                _signalR.SendNotification(account.Name, id.ToString(), notification.Message, notification.SendTime?.ToString("o"), "0", notification.Id.ToString());
                return Ok();
            } catch (Exception ex) {

                return Json("Error");
            }
        }

        public IActionResult CancelSend(int id)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                int userID = int.Parse(user);


                var friend = _context.Friends.FirstOrDefault(f => f.User == userID && f.Friend1 == id && f.Status == 1);

                _context.Friends.Remove(friend);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {

                return Json("Error");
            }
        }

        public IActionResult CancelRequest(int id)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                int userID = int.Parse(user);


                var friend = _context.Friends.FirstOrDefault(f => f.User == id && f.Friend1 == userID && f.Status == 1);

                _context.Friends.Remove(friend);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {

                return Json("Error");
            }
        }
        public IActionResult AcceptRequest(int id)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                int userID = int.Parse(user);


                var friend = _context.Friends.FirstOrDefault(f => f.User == id && f.Friend1 == userID && f.Status == 1);

                friend.Status = 2;

                var newFriend = new Friend
                {
                    User = userID,
                    Friend1 = friend.User,
                    Status = 2,
                    SendTime = DateTime.Now
                };

                _context.Friends.Add(newFriend);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {

                return Json("Error");
            }
        }

        public IActionResult UnFriend(int id)
        {
            try
            {
                string user = HttpContext.Session.GetString("User");
                int userID = int.Parse(user);


                var friend = _context.Friends.FirstOrDefault(f => f.User == userID && f.Friend1 == id && f.Status == 2);
                var friend2 = _context.Friends.FirstOrDefault(f => f.User == id && f.Friend1 == userID && f.Status == 2);

                _context.Friends.Remove(friend);
                _context.Friends.Remove(friend2);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {

                return Json("Error");
            }
        }


    }
}
