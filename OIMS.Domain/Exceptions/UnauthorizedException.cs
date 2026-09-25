using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Domain.Exceptions
{
    public sealed class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message)
            : base(message, 401, "UNAUTHORIZED") { }
    }
}
