using System;

namespace SWE3.SeppMapper.Attributes
{
    /// <summary>
    /// This attribute is used to mark a property to have the datatype SERIAL in the database.
    /// This will only work on properties of type int.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerialAttribute : Attribute
    {
    }
}