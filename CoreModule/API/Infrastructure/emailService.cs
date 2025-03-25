using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using MailKit.Security;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmail(string toEmail, string otp, string fullName)
    {
        var smtpSettings = _configuration.GetSection("EmailSettings");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Welcome to Encryptz ERP!", smtpSettings["SenderEmail"]));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = "Your OTP Code";
        email.Body = new TextPart("html")
        {
            Text = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333; text-align: center;'>
                        <div style='max-width: 500px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9;'>
                            <h2 style='color: #007bff;'>Welcome to Encryptz ERP Solutions!</h2>
                            <p>Dear {fullName},</p>
                            <p>Thank you for choosing <b>Encryptz ERP Solutions</b>. To proceed with your login, please use the OTP code below:</p>
                            <div style='font-size: 24px; font-weight: bold; color: #007bff; padding: 10px; background-color: #fff; border: 1px solid #ddd; display: inline-block;'>
                                {otp}
                            </div>
                            <p>This OTP is valid for <b>5 minutes</b>. Please do not share it with anyone.</p>
                            <p>If you did not request this OTP, please ignore this email.</p>
                            <br/>
                            <p style='font-size: 14px; color: #666;'>Best regards, <br/> <b>Encryptz Team</b></p>
                        </div>
                    </body>
                    </html>"
                    };


        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(smtpSettings["SmtpServer"], int.Parse(smtpSettings["Port"]??"465"), bool.Parse(smtpSettings["EnableSSL"] ??"true"));
        await smtp.AuthenticateAsync(smtpSettings["SenderEmail"], smtpSettings["SenderPassword"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
