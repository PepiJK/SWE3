using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class SeppProperty
    {
        [PrimaryKey]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public Type SeppEntityType { get; set; }
        public SeppEntity SeppEntity { get; set; }
    }
}