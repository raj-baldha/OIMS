using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.Interfaces.Helpers;
using BC = BCrypt.Net.BCrypt;

namespace OIMS.Infrastructure.Helpers
{
    public class PasswordHelper : IPasswordHelper
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            return BC.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BC.Verify(password, passwordHash);
        }
    }
}
