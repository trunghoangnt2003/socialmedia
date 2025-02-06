using System.Net;
using System.Net.Mail;

namespace SocialMedia.Helper
{
	public class SendMail
	{
		public static bool SendVerificationEmail(string to, string subject, string body)
		{
			try
			{
				MailMessage msg = new MailMessage(ConstantHelper.emailSender, to, subject, body)
				{
					IsBodyHtml = true
				};

				using (var client = new SmtpClient(ConstantHelper.hostEmail, ConstantHelper.portEmail))
				{
					client.EnableSsl = true;
					client.UseDefaultCredentials = false;
					client.Credentials = new NetworkCredential(ConstantHelper.emailSender, ConstantHelper.passwordSender);
					client.Send(msg);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}
}
