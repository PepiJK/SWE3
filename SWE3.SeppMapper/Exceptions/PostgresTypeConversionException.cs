using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>
    /// This exception should be thrown when a conversion from a C# type to a postgres type or vice versa fails.
    /// </summary>
    public class PostgresTypeConversionException : Exception
    {
        /// <summary>Provide some basic information on why the conversion fails.</summary>
        /// <param name="info"></param>
        public PostgresTypeConversionException(string info) : base(info) { }
    }
}