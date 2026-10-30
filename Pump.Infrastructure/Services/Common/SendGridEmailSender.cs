using Microsoft.Extensions.Configuration;
using Pump.Application.Interfaces.Common;
using SendGrid;
using SendGrid.Helpers.Mail;


namespace Pump.Infrastructure.Services.Common
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailSender(IConfiguration config)
        {
            _apiKey = config["SendGrid:ApiKey"]!;
            _fromEmail = config["SendGrid:FromEmail"]!;
            _fromName = config["SendGrid:FromName"] ?? "PUMP Gym OS";
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlBody);

            var response = await client.SendEmailAsync(msg, ct);
            if ((int)response.StatusCode >= 400)
            {
                var body = await response.Body.ReadAsStringAsync(ct);
                throw new Exception($"SendGrid failed ({response.StatusCode}): {body}");
            }
        }
    }
}
