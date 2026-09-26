using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OIMS.Application.Helpers
{
    public static class StringExtensions
    {
        public static string Clean(this string value)
        {
            return value.Trim().ToLower();
        }
    }
}
