using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class ChatController : Controller
    {
        private CloudinaryServices _cloudinaryServices;
        private SocialNetworkContext _socialNetworkContext;
        private readonly SignalRService _signalR;

        public ChatController( CloudinaryServices cloudinaryServices, SocialNetworkContext socialNetworkContext, SignalRService signalR)
        {
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext=socialNetworkContext;
            _signalR=signalR;
        }
        public async Task<IActionResult> GetChatModal(int friendId)
        {
            var friend = await _socialNetworkContext.Users.FirstOrDefaultAsync(f => f.Id == friendId);
            return ViewComponent("ChatModalComponent", new { friend = friend });
        }

        [HttpPost]
        public async Task SendMessage(IFormFile[] files, string content, int friendID)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return;

            int userID = int.Parse(user);
            List<Dictionary<string, string>> resClound = await _cloudinaryServices.PutFilesToCloundinary(files);

            if (resClound.Count > 0)
            {
                foreach (Dictionary<string, string> pairs in resClound)
                {
                    var url = pairs["url"];
                    var type = pairs["resource_type"];
                    Chat chat = new Chat
                    {
                        Contents = url,
                        Sender = userID,
                        Receiver = friendID,
                        SendTime = DateTime.Now,
                        Status = (int?)Status.SEND,
                    };

                    if (type == "image")
                    {
                        chat.Type = (int?)Types.IMAGE;
                    }
                    else if (type == "video")
                    {
                        var format = pairs["format"];
                        if (format == "mp3") chat.Type = (int?)Types.AUDIO;
                        else chat.Type = (int?)Types.VIDEO;
                    }
                    _socialNetworkContext.Chats.Add(chat);
                }
            }
            if ( !string.IsNullOrEmpty(content))
            {
                Chat chat = new Chat
                {
                    Contents = content,
                    Sender = userID,
                    Receiver = friendID,
                    Status =  (int?)Types.TEXT,
                    SendTime = DateTime.Now,
                };
                _socialNetworkContext.Chats.Add(chat);
            }
             _socialNetworkContext.SaveChanges();
            await _signalR.SendMessage(userID.ToString(),friendID.ToString());
        }

        [HttpGet]
        public List<Chat> getAllMessages(int friendId)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return new List<Chat>();

            int userID = int.Parse(user);
            var listMessage = _socialNetworkContext.Chats
                .Where(c => (c.Sender == userID && c.Receiver == friendId) || (c.Sender == friendId && c.Receiver == userID))
                .OrderBy(p => p.SendTime);
            return listMessage.ToList();
        }
    }
}
