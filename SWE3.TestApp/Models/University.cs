using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class University
    {
        [PrimaryKeyAttribute]
        public int Id { get; set; }
        [RequiredAttribute]
        public string Name { get; set; }
        [RequiredAttribute]
        public string Address { get; set; }
        public List<Student> Students { get; set; }
    }
}