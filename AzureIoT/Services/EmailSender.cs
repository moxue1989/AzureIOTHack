using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureIoT.Services
{
    public class EmailSender
    {
        public static string SendgridApiKey = "{sendGrid API key}";

        public static async Task SendEmailsAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(SendgridApiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("mobot@mobot.com", "Mobot"),
                Subject = subject,
                HtmlContent = message,
                PlainTextContent = message
            };
            msg.AddTo(new EmailAddress(email));
            await client.SendEmailAsync(msg);
        }
    }
}
