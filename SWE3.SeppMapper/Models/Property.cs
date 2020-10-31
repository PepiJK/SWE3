using System;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Helpers;

namespace SWE3.SeppMapper.Models
{
    public class Property
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSerial {get; set; }
        public ForeignKeyAttribute ForeignKeyInfo { get; set; }
    }
}