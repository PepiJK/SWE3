using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>This exception should be thrown when a new column can not be added to a table.</summary>
    public class NewColumnException : Exception
    {
        /// <summary>Provide some basic information on why a new column cannot be added to a table.</summary>
        /// <param name="info"></param>
        public NewColumnException(string info) : base(info) { }
    }
}