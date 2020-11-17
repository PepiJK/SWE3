using System;

namespace SWE3.SeppMapper.Attributes
{
    /// <summary>This attribute is used to mark a property to be not nullable in the database.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
    }
}