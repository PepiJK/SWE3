using System.Collections.Generic;

namespace SWE3.SeppMapper.Models
{
    public class Table
    {
        public string Name { get; set; }
        public IEnumerable<Column> Columns { get; set; }
         
    }
}