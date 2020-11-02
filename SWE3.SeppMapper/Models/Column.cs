namespace SWE3.SeppMapper.Models
{
    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSerial { get; set; }
        public ForeignKeyInfo ForeignKeyInfo { get; set; }
    }
}