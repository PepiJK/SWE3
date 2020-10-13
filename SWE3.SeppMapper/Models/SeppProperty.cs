using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class SeppProperty
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
    }
}