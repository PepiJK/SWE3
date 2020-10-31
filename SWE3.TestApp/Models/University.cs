using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class University
    {
        [PrimaryKey]
        [Serial]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public IEnumerable<Student> Students { get; set; }
    }
}