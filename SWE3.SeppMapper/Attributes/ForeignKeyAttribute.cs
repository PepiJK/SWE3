using System;
using SWE3.SeppMapper.Helpers;

namespace SWE3.SeppMapper.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public Type ReferencingType { get; set; }
        public string ReferencingColumn { get; set; }
        public ReferentialActions ReferentialAction { get; set; } = ReferentialActions.NoAction;
    }
}