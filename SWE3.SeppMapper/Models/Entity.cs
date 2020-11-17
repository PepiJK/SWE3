using System;
using System.Collections.Generic;

namespace SWE3.SeppMapper.Models
{
    /// <summary>Represents an entity which is managed by the framework.</summary>
    public class Entity
    {
        /// <summary>The type of the managed entity.</summary>
        public Type Type { get; set; }
        
        /// <summary>The properties of this entity.</summary>
        public IEnumerable<Property> Properties { get; set; }
    }
}