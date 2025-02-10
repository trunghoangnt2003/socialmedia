using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
			_context = context; 
		}

		public IActionResult Index()
		{
			if (HttpContext.Session.GetString("user") == null)
			{
				ViewBag.HideHeaderFooter = true;

                return View();
			}
			else
			{
				return RedirectToAction("Privacy", "Home");
			}
		}
		[HttpPost]
		public IActionResult Index(string usename, string password)
		{
			string encryptedInputPassword = Encrypt(password, true);
			var account = _context.Users.FirstOrDefault(a => a.Email == usename && a.Password == encryptedInputPassword);
			if (account != null && account.IsActive == true)
			{
                HttpContext.Session.SetString("User", account.Id.ToString());

                return RedirectToAction("Privacy", "Home");
			}
			else if(account == null)
			{
				ViewBag.ErrorMessage = "Username or password not true!";
			} else if(account.IsActive == false)
			{
				 ViewBag.ErrorMessage = "Account is not active!";
			}
			ViewBag.HideHeaderFooter = true;

            return View();
		}

		public static string Encrypt(string toEncrypt, bool useHashing)
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

		public static string Decrypt(string toDecrypt, bool useHashing)
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
