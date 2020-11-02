namespace SWE3.SeppMapper.Models
{
    public class ForeignKeyInfo
    {
        public string ReferencingTable { get; set; }
        public string ReferencingColumn { get; set; }
        public string ConstraintName { get; set; }
    }
}