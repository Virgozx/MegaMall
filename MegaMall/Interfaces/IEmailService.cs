namespace MegaMall.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOTP(string otp, string receiver);
    }
}
