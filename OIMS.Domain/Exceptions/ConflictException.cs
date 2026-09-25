using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Domain.Exceptions
{
    public sealed class ConflictException : BaseException
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT") { }
    }
}
