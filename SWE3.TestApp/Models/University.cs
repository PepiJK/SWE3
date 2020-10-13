using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class University
    {
        [PrimaryKey]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public IList<Student> Students { get; set; }
    }
}