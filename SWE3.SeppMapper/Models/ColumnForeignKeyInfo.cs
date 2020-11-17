namespace SWE3.SeppMapper.Models
{
    /// <summary>Represents the foreign key information of a column.</summary>
    public class ColumnForeignKeyInfo
    {
        /// <summary>The referencing table name of the foreign key.</summary>
        public string ReferencingTable { get; set; }
        
        /// <summary>The column of the table the foreign key is referencing.</summary>
        public string ReferencingColumn { get; set; }
        
        /// <summary>The name of the foreign key constraint in the database.</summary>
        public string ConstraintName { get; set; }
    }
}