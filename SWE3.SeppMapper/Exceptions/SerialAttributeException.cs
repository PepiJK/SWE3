using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>
    /// This exception should be thrown when a property with the SerialAttribute does not fulfill the requirements
    /// of the SerialAttribute.
    /// </summary>
    public class SerialAttributeException : Exception
    {
        /// <summary>
        /// Provide some basic information on why the property does not fulfill the requirements of the SerialAttribute.
        /// </summary>
        /// <param name="info"></param>
        public SerialAttributeException(string info) : base(info) { }
    }
}