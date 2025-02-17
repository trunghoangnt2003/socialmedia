using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;
using SocialMedia.Helper;
using System.Security.Cryptography;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
	public class RegisterController : Controller
	{
		private readonly SocialNetworkContext _context;
        private readonly MD5CryptoService _contextCrypt;
		private readonly SendMail _contextMail;

        public RegisterController(SocialNetworkContext context, MD5CryptoService _contextCrypts, SendMail _contextSendMail)
		{
			_context = context;
			_contextCrypt = _contextCrypts;
			_contextMail = _contextSendMail;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Index(string email, string password, string passwordConfirm, string address, string fullName, string phoneNumber, DateTime dateOfBirth, string gender)
		{
			var checkGender = gender == "male";
			var account = _context.Users.FirstOrDefault(acc => acc.Email == email);
			var existingPhone = _context.Users.FirstOrDefault(acc => acc.Phone == phoneNumber);
			var passwordEncrype = _contextCrypt.Encrypt(password, true);
			var avatarUser = checkGender
				? "https://res.cloudinary.com/ddg2gdfee/image/upload/v1738900139/avatar_male_default_yjf1du.jpg"
				: "https://res.cloudinary.com/ddg2gdfee/image/upload/v1738900139/avatar_female_default_tkxgkl.jpg";

			if (account != null)
			{
				ViewBag.ErrorEmail = "Email is exist";
			}
			else if (password != passwordConfirm)
			{
				ViewBag.ErrorPassword = "Password confirm does not match.";
			}else if(existingPhone != null)
			{
				ViewBag.ErrorPhone = "Phone is exist";
			}
			else if(dateOfBirth == null)
			{
                ViewBag.ErrorDob = "Please true date of birth.";
            }
			else
			{
				var user = new User
				{
					Email = email,
					Password = passwordEncrype,
					Address = address,
					Name = fullName,
					Phone = phoneNumber,
					Dob = DateOnly.FromDateTime(dateOfBirth),
					Gender = checkGender,
					Avatar = avatarUser,
					Role = 2,
					IsActive = false 
				};

				_context.Users.Add(user);
				_context.SaveChanges();
				string confirmLink = Url.Action("ConfirmEmail", "Register", new { email = email }, Request.Scheme);
                string subject = "Confirm Your Account";
                string body = $"Please click the following link to confirm your account: <a href='{confirmLink}'>Confirm Account</a>";


                _contextMail.SendVerificationEmail(email, subject, body);
                ViewBag.UserEmail = email;
				return View("RegisterSuccess");
			}
			return View();
		}

		public IActionResult ConfirmEmail(string email)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == email);
			if (user != null && user.IsActive == false)
			{
				user.IsActive = true;
				_context.SaveChanges();
				return View("ConfirmSuccess"); 
			}
			return View("ConfirmFailed"); 
		}
	}
}
