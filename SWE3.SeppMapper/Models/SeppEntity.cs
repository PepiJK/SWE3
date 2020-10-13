using System;
using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class SeppEntity
    {
        public Type Type { get; set; }
        public IList<SeppProperty> SeppProperties { get; set; }
    }
}