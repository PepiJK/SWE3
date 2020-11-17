using System.Collections.Generic;

namespace SWE3.SeppMapper.Models
{
    /// <summary>Represents a table in the database.</summary>
    public class Table
    {
        /// <summary>The table name in the database.</summary>
        public string Name { get; set; }
        
        /// <summary>The columns of the table.</summary>
        public IEnumerable<Column> Columns { get; set; }
        
        /// <summary>Whether the table has rows in the database.</summary>
        public bool HasRows { get; set; }
    }
}