using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SocialMediaBackend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration config)
        {
            // Load SMTP settings from appsettings.json
            _smtpServer = config["Smtp:Server"] ?? throw new ArgumentNullException("Smtp:Server");
            _smtpPort = int.Parse(config["Smtp:Port"] ?? throw new ArgumentNullException("Smtp:Port"));
            _smtpUsername = config["Smtp:Username"] ?? throw new ArgumentNullException("Smtp:Username");
            _smtpPassword = config["Smtp:Password"] ?? throw new ArgumentNullException("Smtp:Password");
            _enableSsl = bool.Parse(config["Smtp:EnableSsl"] ?? "false");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // Create the email message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername), // Sender's email address
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Set to true if the message contains HTML
                };
                mailMessage.To.Add(email); // Recipient's email address

                // Configure the SMTP client
                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtpClient.EnableSsl = _enableSsl;

                    // Send the email
                    await smtpClient.SendMailAsync(mailMessage);
                }

                Console.WriteLine($"Email sent to {email} with subject: {subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {email}. Error: {ex.Message}");
                throw; // Re-throw the exception to handle it in the calling code
            }
        }
    }
}