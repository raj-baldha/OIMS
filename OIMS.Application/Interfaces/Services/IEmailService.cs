using OIMS.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string body);
    }
}
