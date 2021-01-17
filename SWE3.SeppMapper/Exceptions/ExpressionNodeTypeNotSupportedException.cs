using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>
    /// This exception should be thrown when a node type of an expression is not supported by SeppMapper.
    /// </summary>
    public class ExpressionNodeTypeNotSupportedException : Exception
    {
        /// <summary>Provide some basic information on why this node type is not supported.</summary>
        /// <param name="info"></param>
        public ExpressionNodeTypeNotSupportedException(string info) : base(info) { }
    }
}