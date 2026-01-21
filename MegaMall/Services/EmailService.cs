using MegaMall.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MegaMall.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly MegaMall.Interfaces.IAIService _aiService;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, MegaMall.Interfaces.IAIService aiService)
        {
            _configuration = configuration;
            _logger = logger;
            _aiService = aiService;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Optionally generate subject/body using AI templates
                var useAi = _configuration.GetValue<bool>("EmailSettings:UseAITemplates", false);
                if (useAi && _aiService != null)
                {
                    var aiResult = await _aiService.GenerateEmailAsync(subject ?? "general", new { Recipient = to });
                    if (aiResult != null)
                    {
                        if (!string.IsNullOrWhiteSpace(aiResult.Subject))
                            subject = aiResult.Subject;
                        if (!string.IsNullOrWhiteSpace(aiResult.Body))
                            body = aiResult.Body;
                    }
                }

                var emailSettings = _configuration.GetSection("EmailSettings");
                var mail = emailSettings["Mail"];
                var displayName = emailSettings["DisplayName"];
                var password = emailSettings["Password"];
                var host = emailSettings["Host"];
                if (string.IsNullOrWhiteSpace(host)) host = "localhost";
                var portStr = emailSettings["Port"];
                var port = int.TryParse(portStr, out int p) ? p : 587;

                if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email settings are missing. Skipping email send.");
                    return;
                }

                var fromAddress = new MailAddress(mail, displayName);
                var toAddress = new MailAddress(to);

                using (var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, password)
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                    _logger.LogInformation($"[Email Sent] To: {to}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
            }
        }

        public async Task<bool> SendOTP(string otp, string receiver)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var mail = emailSettings["Mail"];
                var displayName = emailSettings["DisplayName"];
                var password = emailSettings["Password"];
                var host = emailSettings["Host"];
                if (string.IsNullOrWhiteSpace(host)) host = "localhost";
                var portStr = emailSettings["Port"];
                var port = int.TryParse(portStr, out int p) ? p : 587;

                if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email settings are missing. Cannot send OTP.");
                    return false;
                }

                var fromAddress = new MailAddress(mail, displayName);
                var toAddress = new MailAddress(receiver);

                string subject = "MegaMall - Xác thực OTP";
                string body = $"Xin chào!\n\nMã OTP của bạn là: {otp}\nVui lòng không chia sẻ cho bất kỳ ai.\n\nTrân trọng!";

                using (var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, password)
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    await smtp.SendMailAsync(message);
                    _logger.LogInformation($"[OTP Sent] To: {receiver}");
                }

                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"❌ Lỗi gửi OTP: {ex.Message}");
                return false;
            }
        }
    }
}
