using OIMS.Application.Interfaces.Services;

namespace OIMS.Application.BackgroundJobs;

public class SendOrderConfirmationEmailJob
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task ExecuteAsync(
        string email,
        string customerName,
        int orderId,
        DateTime orderDate,
        decimal totalAmount
    )
    {
        string subject = $"Order Confirmation - #{orderId}";

        string templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "EmailTemplate",
            "OrderConfirmationEmail.html"
        );

        string body = await File.ReadAllTextAsync(templatePath);

        body = body.Replace("{{CustomerName}}", customerName)
            .Replace("{{OrderId}}", orderId.ToString())
            .Replace("{{OrderDate}}", orderDate.ToString("dd-MM-yyyy HH:mm"))
            .Replace("{{TotalAmount}}", totalAmount.ToString("0.00"));

        await _emailService.SendEmailAsync(email, subject, body);
    }
}
