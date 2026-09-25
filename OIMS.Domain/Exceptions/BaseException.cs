using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public List<string>? Errors { get; }
        public string ErrorCode { get; }

        protected BaseException(
            string message,
            int statusCode,
            string errorCode = "APPLICATION_ERROR",
            List<string>? errors = null
        )
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
