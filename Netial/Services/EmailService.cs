using MailKit.Net.Smtp;
using MimeKit;

namespace Netial.Services; 

public class EmailService {
    private readonly IConfiguration _configuration;
    private SmtpClient client;
    public string FromEmail { get; }
    public string SmtpAddress { get; }
    public int SmtpPort { get; }
    public string SmtpLogin { get; }
    public string SmtpPassword { get; }

    public EmailService(IConfiguration configuration) {
        _configuration = configuration.GetSection("Email");
        FromEmail = _configuration["From"];
        SmtpAddress = _configuration["SmtpAddress"];
        SmtpPort = Convert.ToInt32(_configuration["SmtpPort"]);
        SmtpLogin = _configuration["SmtpLogin"];
        SmtpPassword = _configuration["SmtpPassword"];
        InitilizeAsync().ConfigureAwait(true).GetAwaiter().GetResult();
    }



    public async Task SendEmailAsync(string email, string subject, string message) {
        var emailMessage = new MimeMessage();
        
        emailMessage.From.Add(new MailboxAddress("Автоматизированное письмо Netial", FromEmail));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = message
        };

        await client.SendAsync(emailMessage);
    }

    private async Task InitilizeAsync() {
        client = new SmtpClient();
        await client.ConnectAsync(SmtpAddress, SmtpPort, true);
        await client.AuthenticateAsync(SmtpLogin, SmtpPassword);
    }
}