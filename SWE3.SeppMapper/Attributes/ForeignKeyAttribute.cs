using System;
using SWE3.SeppMapper.Helpers;

namespace SWE3.SeppMapper.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public Type Type { get; set; }
        public ReferentialActions ReferentialAction { get; set; } = ReferentialActions.Cascade;
    }
}