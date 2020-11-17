using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    /// <summary>Represents a property of an managed entity.</summary>
    public class Property
    {
        /// <summary>The name of the property.</summary>
        public string Name { get; set; }
        
        /// <summary>The type of the property.</summary>
        public Type Type { get; set; }
        
        /// <summary>Whether the property has the PrimaryKeyAttribute.</summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>Whether the property has the RequiredAttribute.</summary>
        public bool IsRequired { get; set; }
        
        /// <summary>Whether the property has the SerialAttribute.</summary>
        public bool IsSerial {get; set; }
        
        /// <summary>The foreign key information if the property has the ForeignKeyAttribute.</summary>
        public ForeignKeyAttribute ForeignKeyInfo { get; set; }
    }
}