using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWE3.TestApp.Models
{
    public class University
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public List<Student> Students { get; set; }
    }
}