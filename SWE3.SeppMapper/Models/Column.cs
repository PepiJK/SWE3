namespace SWE3.SeppMapper.Models
{
    /// <summary>Represents a column of a table in the database.</summary>
    public class Column
    {
        /// <summary>The name of the column in the database.</summary>
        public string Name { get; set; }
        
        /// <summary>The type of the column in the database.</summary>
        public string Type { get; set; }
        
        /// <summary>Whether the column is a primary key.</summary>
        public bool IsPrimary { get; set; }
        
        /// <summary>Whether the column is not nullable.</summary>
        public bool IsRequired { get; set; }
        
        /// <summary>Whether the column is serial (autoincrement).</summary>
        public bool IsSerial { get; set; }
        
        /// <summary>The foreign key information if the column is a foreign key.</summary>
        public ColumnForeignKeyInfo ForeignKeyInfo { get; set; }
    }
}