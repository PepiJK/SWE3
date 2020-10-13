using System;
using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class SeppEntity
    {
        [PrimaryKey]
        public Type Type { get; set; }
        public List<SeppProperty> SeppProperties { get; set; }
    }
}