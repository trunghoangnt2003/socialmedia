using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Helper;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia.Controllers
{
	public class ForgotPasswordController : Controller
	{
		private readonly SocialNetworkContext _contextDBA;
		private readonly SendMail _contextMail;
		private readonly MD5CryptoService _contextCrypt;

		public ForgotPasswordController(SocialNetworkContext _contextDB, SendMail _contextSendMail, MD5CryptoService _contextCrypts)
		{
			_contextDBA = _contextDB;
			_contextMail = _contextSendMail;
			_contextCrypt = _contextCrypts;
		}
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Index(string email)
		{
			var user = _contextDBA.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				ViewBag.EmailFail = "Email not found.";
				return View();
			}

			string encodedEmail = _contextCrypt.Encrypt(email, true);
			string confirmLink = Url.Action("ConfirmEmail", "ForgotPassword", new { email = encodedEmail }, Request.Scheme);
			string subject = "Forgot password!!";
			string body = $"Please click the following link to get your password: <a href='{confirmLink}'>Confirm Password</a>";
			_contextMail.SendVerificationEmail(email, subject, body);
			ViewBag.UserEmail = email;
			ViewBag.Success = true;
			return View("ForgotSuccess");
		}


		public IActionResult ConfirmEmail(string email)
		{
			string emailDecrypt = _contextCrypt.Decrypt(email, true);
			ViewBag.UserEmail = emailDecrypt;
			return View("ForgotPasswordView");
		}

		public IActionResult ResetPassword(string newPassword, string confirmPassword, string email)
		{

			if (string.IsNullOrEmpty(email))
			{
				ViewBag.ResetFail = "Email is required.";
				return View("ForgotPasswordView");
			}

			var user = _contextDBA.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				ViewBag.ResetFail = "User not found.";
				return View("ForgotPasswordView");
			}

			if (newPassword != confirmPassword)
			{
				ViewBag.ResetFail = "New passwords do not match!";
				return View("ForgotPasswordView");
			}

			user.Password = _contextCrypt.Encrypt(newPassword, true);
			_contextDBA.SaveChanges();

			ViewBag.SuccessMessage = "Password reset successfully!";
			return View();
		}

	}
}
