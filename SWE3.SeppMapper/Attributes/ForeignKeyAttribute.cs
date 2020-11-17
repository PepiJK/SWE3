using System;
using SWE3.SeppMapper.Helpers;

namespace SWE3.SeppMapper.Attributes
{
    /// <summary>This attribute is used to mark a property as a foreign key.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        /// <summary>Set which entity type the foreign key is referencing.</summary>
        public Type ReferencingType { get; set; }

        /// <summary>Set which column of the entity the foreign key is referencing.</summary>
        public string ReferencingColumn { get; set; }
        
        /// <summary>Set the referential action of the foreign key. The default is no action.</summary>
        public ReferentialActions ReferentialAction { get; set; } = ReferentialActions.NoAction;
    }
}