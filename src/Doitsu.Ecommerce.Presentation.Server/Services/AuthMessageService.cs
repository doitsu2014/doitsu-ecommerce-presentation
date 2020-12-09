using Doitsu.Ecommerce.Presentation.Server.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace Doitsu.Ecommerce.Presentation.Server.Services
{
    public class AuthMessageService : IEmailSender
    {
        private readonly ILogger<AuthMessageService> _logger;
        private readonly SmtpClientSettings _settings;

        public AuthMessageService(IOptions<SmtpClientSettings> smtpClientSettingsOptions,
            ILogger<AuthMessageService> logger)
        {
            _logger = logger;
            _settings = smtpClientSettingsOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Policy
                .Handle<SmtpException>()
                .RetryAsync(3,
                    (ex, retryCount, ctx) => { _logger.LogError("AuthMessageService SendEmailAsync exception: ", ex); })
                .ExecuteAsync(
                    async () =>
                    {
                        var client = new SmtpClient(_settings.Host, _settings.Port)
                        {
                            Credentials = new NetworkCredential(_settings.Mail, _settings.Password),
                            EnableSsl = _settings.EnableSsl
                        };

                        await client.SendMailAsync(
                            new MailMessage(_settings.Mail, email, subject, htmlMessage) {IsBodyHtml = true}
                        );
                    }
                );
        }
    }
}