using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Helpers
{
    public interface IPasswordHelper
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
