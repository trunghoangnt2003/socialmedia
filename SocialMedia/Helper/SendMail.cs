using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace SocialMedia.Helper
{
	public class SendMail
	{

		private readonly IConfiguration _configuration;
		public SendMail(IConfiguration configuration ) {
			_configuration = configuration;
		}

		public bool SendVerificationEmail(string to, string subject, string body)
		{

			string HostEmail = _configuration.GetSection("MailConfiguration")["SmtpHost"];
            int PortEmail = int.Parse(_configuration.GetSection("MailConfiguration")["MailPort"]);
            string EmailSender = _configuration.GetSection("MailConfiguration")["MailSender"];
            string PasswordSender = _configuration.GetSection("MailConfiguration")["MailPassword"];

            try
			{
				MailMessage msg = new MailMessage(EmailSender, to, subject, body)
				{
					IsBodyHtml = true
				};

				using (var client = new SmtpClient(HostEmail, PortEmail))
				{
					client.EnableSsl = true;
					client.UseDefaultCredentials = false;
					client.Credentials = new NetworkCredential(EmailSender, PasswordSender);
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
