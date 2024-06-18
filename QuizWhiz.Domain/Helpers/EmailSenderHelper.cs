using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Helpers
{
    public class EmailSenderHelper
    {
        private readonly IConfiguration _configuration;

        public EmailSenderHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }   

        public bool SendEmail(string toEmail, string subject, string message)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                using ( var smtpClient = new SmtpClient(emailSettings["SmtpServer"]))
                {
                    smtpClient.Port = int.Parse(emailSettings["SmtpPort"]);
                    smtpClient.Credentials = new NetworkCredential(emailSettings["SmtpUser"], emailSettings["SmtpPass"]);
                    smtpClient.EnableSsl = true;

                    smtpClient.UseDefaultCredentials = false;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(emailSettings["FromEmail"]);
                        mailMessage.Subject = subject;
                        mailMessage.Body = message;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.To.Add(toEmail);
                        smtpClient.Send(mailMessage);

                        Console.WriteLine("Email sent successfully!");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
                throw; // Optionally re-throw the exception if needed
            }
        }
    }
}
