using System;

namespace SWE3.SeppMapper.Attributes
{
    /// <summary>This attribute is used to mark a property as a primary key.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}