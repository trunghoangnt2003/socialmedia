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
                    Type =  (int?)Types.TEXT,
                    SendTime = DateTime.Now,
                    Status = (int?)Status.SEND
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

        [HttpGet]
        public void ReadMessage(int friendId)
        {
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return ;

            int userID = int.Parse(user);
            var messagesNotSeen = _socialNetworkContext.Chats
                    .Where(c => (c.Status == (int?)Status.SEND 
                                && c.Sender == friendId)
                                && c.Receiver == userID)
                    .ToList();

            foreach (var message in messagesNotSeen)
            {
                message.Status = (int?)Status.SEEN;
            }
            _socialNetworkContext.UpdateRange(messagesNotSeen);
            _socialNetworkContext.SaveChanges();
        }


        public List<Chat> getMessageNotification()
        {
            List<Chat> chats = new List<Chat>();
            string user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return chats;

            int userId = int.Parse(user);

            var sqlResult = _socialNetworkContext.Users
                            .Join(
                                _socialNetworkContext.Chats
                                    .Where(c => c.Sender == userId || c.Receiver == userId)
                                    .Select(c => new
                                    {
                                        UserId = c.Sender == userId ? c.Receiver : c.Sender,
                                        c.SendTime,
                                        c.Status,
                                        c.Contents,
                                        c.Sender,
                                        c.Receiver,
                                    })
                                    .GroupBy(c => c.UserId) 
                                    .Select(g => new
                                    {
                                        UserId = g.Key,
                                        LatestSendTime = g.Max(c => c.SendTime), 
                                        Status = g.OrderByDescending(c => c.SendTime).Select(c => c.Status).FirstOrDefault(),
                                        Contents = g.OrderByDescending(c => c.SendTime).Select(c => c.Contents).FirstOrDefault(),
                                        Sender = g.OrderByDescending(c => c.SendTime).Select(c => c.Sender).FirstOrDefault(),
                                        Receiver = g.OrderByDescending(c => c.SendTime).Select(c => c.Receiver).FirstOrDefault(),
                                    }),
                                user => user.Id,
                                chat => chat.UserId,
                                (user, chat) => new
                                {
                                    user.Id,
                                    user.Name,
                                    user.Avatar,
                                    chat.LatestSendTime,
                                    chat.Status,
                                    chat.Contents,
                                    chat.Sender,
                                    chat.Receiver,
                                })
                            .OrderByDescending(x => x.LatestSendTime) 
                            .ToList();


            foreach (var item in sqlResult)
            {
                chats.Add(new Chat { 
                    Id = item.Id,
                    SenderNavigation = new User { Id = item.Id, Name = item.Name, Avatar = item.Avatar },
                    SendTime = item.LatestSendTime,
                    Status = item.Status,
                    Contents = item.Contents,
                    Sender = item.Sender,
                    Receiver = item.Receiver
                } );
            }

            return chats;
        }
    }
}
