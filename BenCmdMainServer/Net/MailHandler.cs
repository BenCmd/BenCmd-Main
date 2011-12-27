using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using BenCmdMainServer;

namespace BenCmdMainServer.Net
{
	public static class MailHandler
	{
		public static void SendEmail(string recipient, string subject, string htmlBody, string plainBody)
		{
			MailMessage msg = new MailMessage();
			msg.From = new MailAddress(Security.GetEmailAddress());
			msg.To.Add(new MailAddress(recipient));
			msg.Subject = subject;
			msg.BodyEncoding = Encoding.UTF8;
			msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
			msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainBody, null, "text/plain"));
			SmtpClient smtpSender = new SmtpClient(Security.GetSmtpServer());
			smtpSender.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpSender.UseDefaultCredentials = false;
			smtpSender.EnableSsl = true;
			smtpSender.Credentials = new NetworkCredential(Security.GetEmailUsername(), Security.GetEmailPassword());
			smtpSender.Port = 587;
			Logger.Log(Logger.Level.Debug, smtpSender.Credentials.ToString());
			ServicePointManager.ServerCertificateValidationCallback = 
                delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
			{
				return true; };
			smtpSender.Send(msg);
		}
		
		public static void SendWelcomeEmail(string recipient, string username, string verificationCode)
		{
			string msgHtml =
				"<P>Hello " + username + ",</P>" +
				"<P>Thank you for signing your Minecraft server up for access to the<BR />" +
				"BenCmd main servers.</P>" +
				"<P>Before your server will be able to connect to these servers, you<BR />" +
				"will need to verify that you do, in fact, own this email address. In<BR />" +
				"order to do this, please visit the link below:</P>" +
				"<P><A HREF=\"http://bencmd.servehttp.com/confirmemail?code=" + verificationCode + "\">http://bencmd.servehttp.com/confirmemail?code=" + verificationCode + "</A></P>" +
				"<P>If you did not request this email, then do not worry. Simply delete<BR />" +
				"this email and the account will be automatically deleted after 24 hours.</P>" +
				"<P>Thanks,<BR />" +
				"The BenCmd Team</P>";
			string msgPlain =
				"Hello " + username + ",\n\n" +
				"Thank you for signing your Minecraft server up for access to the\n" +
				"BenCmd main servers.\n\n" +
				"Before your server will be able to connect to these servers, you\n" +
				"will need to verify that you do, in fact, own this email address. In\n" +
				"order to do this, please visit the link below:\n\n" +
				"http://bencmd.servehttp.com/confirmemail?code=" + verificationCode + "\n\n" +
				"If you did not request this email, then do not worry. Simply delete\n" +
				"this email and the account will be automatically deleted after 24 hours.\n\n" +
				"Thanks,\n" +
				"The BenCmd Team";
			new Thread(delegate()
			{
				try
				{
					SendEmail(recipient, "BenCmd Server Signup", msgHtml, msgPlain);
				}
				catch (Exception e)
				{
					Logger.Log(Logger.Level.Warning, "Failed to send welcome email to " + recipient + ": " + e.Message);
					Logger.Log(Logger.Level.Debug, e.ToString());
				}
			})
			{
				Name = "Welcome Mail Send Thread"
			}.Start();
		}
	}
}

