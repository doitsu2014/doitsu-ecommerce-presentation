using Doitsu.Ecommerce.Presentation.Server.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Doitsu.Ecommerce.Presentation.Server.Services
{
    public class AuthMessageService : IEmailSender
    {
        private readonly SmtpClientSettings settings;

        public AuthMessageService(IOptions<SmtpClientSettings> smtpClientSettingsOptions)
        {
            settings = smtpClientSettingsOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Mail, settings.Password),
                EnableSsl = settings.EnableSsl
            };

            await client.SendMailAsync(
                new MailMessage(settings.Mail, email, subject, htmlMessage) { IsBodyHtml = true }
            );
        }
    }
}
