using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class SeppProperty
    {
        [PrimaryKeyAttribute]
        public int Id { get; set; }
        [RequiredAttribute]
        public string Name { get; set; }
        [RequiredAttribute]
        public string Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public Type SeppEntityType { get; set; }
        public SeppEntity SeppEntity { get; set; }
    }
}