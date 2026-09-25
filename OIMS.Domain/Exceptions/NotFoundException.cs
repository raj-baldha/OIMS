using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Domain.Exceptions
{
    public sealed class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message, 404, "NOT_FOUND") { }
    }
}
