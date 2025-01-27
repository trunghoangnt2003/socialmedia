using Microsoft.AspNetCore.Mvc;
using SocialMedia.Models;
using System.Security.Cryptography;
using System.Text;

namespace SocialMedia.Controllers
{
	public class LoginController : Controller
	{
		private readonly SocialNetworkContext _context;

		public LoginController(SocialNetworkContext context)
		{
			_context = context; // Dependency Injection
		}
		[HttpGet]
		public IActionResult Login()
		{
			if (HttpContext.Session.GetString("user") == null)
			{
				return View();
			}
			else
			{
				return RedirectToAction("Privacy", "Home");
			}
		}

		[HttpPost]
		public IActionResult login(string usename, string password)
		{
			string encryptedInputPassword = Encrypt(password, true);
			var account = _context.Users.FirstOrDefault(a => a.Email == usename && a.Password == encryptedInputPassword);
			if (account != null)
			{
				HttpContext.Session.SetString("user", usename);
				return RedirectToAction("Privacy", "Home");
			}
			else
			{
				ViewBag.ErrorMessage = "Username or password not true! Please check again!";
				return View();
			}
		}

		public string Encrypt(string toEncrypt, bool useHashing)
		{
			byte[] keyArray;
			byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
			if (useHashing)
			{
				var hashmd5 = new MD5CryptoServiceProvider();
				keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes("iif"));
			}
			else keyArray = Encoding.UTF8.GetBytes("iif");
			var tdes = new TripleDESCryptoServiceProvider
			{
				Key = keyArray,
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			};
			ICryptoTransform cTransform = tdes.CreateEncryptor();
			byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
			return Convert.ToBase64String(resultArray, 0, resultArray.Length);
		}

		public string Decrypt(string toDecrypt, bool useHashing)
		{
			byte[] keyArray;
			byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
			if (useHashing)
			{
				var hashmd5 = new MD5CryptoServiceProvider();
				keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes("iif"));
			}
			else keyArray = Encoding.UTF8.GetBytes("iif");
			var tdes = new TripleDESCryptoServiceProvider
			{
				Key = keyArray,
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			};
			ICryptoTransform cTransform = tdes.CreateDecryptor();
			byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
			return Encoding.UTF8.GetString(resultArray);
		}


	}
}
