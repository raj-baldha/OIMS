using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Common;

namespace OIMS.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
