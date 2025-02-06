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
