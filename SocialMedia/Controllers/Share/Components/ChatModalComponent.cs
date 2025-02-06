using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;

namespace SocialMedia.Controllers.Share.Components
{
    public class ChatModalComponent : ViewComponent
    {
        private SocialNetworkContext _context;
        public ChatModalComponent(SocialNetworkContext context)
        {
            this._context = context;
        }
        public IViewComponentResult Invoke(User friend)
        {
            return View(friend);
        }
    }
}
