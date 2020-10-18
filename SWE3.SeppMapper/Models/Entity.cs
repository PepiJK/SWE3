using System;
using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.SeppMapper.Models
{
    public class Entity
    {
        public Type Type { get; set; }
        public IList<Property> Properties { get; set; }
    }
}