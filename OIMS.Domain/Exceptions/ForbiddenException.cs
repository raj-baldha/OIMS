using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Domain.Exceptions
{
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message)
            : base(message, 403, "FORBIDDEN") { }
    }
}
