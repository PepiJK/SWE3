using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>This exception should be thrown when a type of expression is not supported by SeppMapper</summary>
    public class ExpressionNotSupportedException : Exception
    {
        /// <summary>Provide some basic information on why this expression is not supported.</summary>
        /// <param name="info"></param>
        public ExpressionNotSupportedException(string info) : base(info) { }
    }
}