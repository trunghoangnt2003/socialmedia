using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
    public class ChatController : Controller
    {
        private CloudinaryServices _cloudinaryServices;
        private SocialNetworkContext _socialNetworkContext;

        public ChatController( CloudinaryServices cloudinaryServices, SocialNetworkContext socialNetworkContext)
        {
            _cloudinaryServices = cloudinaryServices;
            _socialNetworkContext=socialNetworkContext;
        }
        public async Task<IActionResult> GetChatModal(int friendId)
        {
            var friend = await _socialNetworkContext.Users.FirstOrDefaultAsync(f => f.Id == friendId);
            return ViewComponent("ChatModalComponent", new { friend = friend });
        }

        [HttpPost]
        public void SendMessage(IFormFile[] files, string content, int friendID)
        {
            List<Dictionary<string, string>> resClound = _cloudinaryServices.PutFilesToCloundinary(files);

            if (resClound.Count > 0)
            {
                foreach (Dictionary<string, string> pairs in resClound)
                {
                    var url = pairs["url"];
                    var type = pairs["resource_type"];
                    Chat chat = new Chat
                    {
                        Contents = url,
                        Sender = 1,
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
                    Sender = 1,
                    Receiver = friendID,
                    Status =  (int?)Types.TEXT,
                    SendTime = DateTime.Now,
                };
                _socialNetworkContext.Chats.Add(chat);
            }
                //_socialNetworkContext.SaveChanges();
        }

        [HttpGet]
        public List<Chat> getAllMessages(int friendId)
        {
            int user = 1;
            var listMessage = _socialNetworkContext.Chats
                .Where(c => (c.Sender == user && c.Receiver == friendId) || (c.Sender == friendId && c.Receiver == user))
                .OrderBy(p => p.SendTime);
            return listMessage.ToList();
        }
    }
}
