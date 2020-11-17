using System;

namespace SWE3.SeppMapper.Exceptions
{
    /// <summary>This excpetion should be thrown when the ForeignKeyAttribute is not correctly used.</summary>
    public class ForeignKeyAttributeException : Exception
    {
        /// <summary>Provide some basic information on why the ForeignKeyAttribute is not correctly used.</summary>
        /// <param name="info"></param>
        public ForeignKeyAttributeException(string info) : base(info) { }
    }
}