using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;
using SocialMedia.Helper;

namespace SocialMedia.Controllers
{
	public class RegisterController : Controller
	{
		private readonly SocialNetworkContext _context;

		public RegisterController(SocialNetworkContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			ViewBag.HideHeaderFooter = true;
			return View();
		}

		[HttpPost]
		public IActionResult Index(string email, string password, string passwordConfirm, string address, string fullName, string phoneNumber, DateTime dateOfBirth, string gender)
		{
			var checkGender = gender == "male";
			var account = _context.Users.FirstOrDefault(acc => acc.Email == email);
			var existingPhone = _context.Users.FirstOrDefault(acc => acc.Phone == phoneNumber);
			var passwordEncrype = LoginController.Encrypt(password, true);
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
				string subject = "Xác nhận tài khoản của bạn";
				string body = $"Vui lòng nhấp vào link sau để xác nhận tài khoản: <a href='{confirmLink}'>Xác nhận tài khoản</a>";

				SendMail.SendVerificationEmail(email, subject, body);
                ViewBag.UserEmail = email;
				ViewBag.HideHeaderFooter = true;
				return View("RegisterSuccess");
			}
			ViewBag.HideHeaderFooter = true;
			return View();
		}

		public IActionResult ConfirmEmail(string email)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == email);
			if (user != null && user.IsActive == false)
			{
				user.IsActive = true;
				_context.SaveChanges();
				ViewBag.HideHeaderFooter = true;
				return View("ConfirmSuccess"); 
			}
			ViewBag.HideHeaderFooter = true;
			return View("ConfirmFailed"); 
		}
	}
}
