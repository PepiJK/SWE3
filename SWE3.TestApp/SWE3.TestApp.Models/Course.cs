using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWE3.TestApp.Models
{
    public class Course
    {
        [Required]
        public string Name { get; set; }
        public int ETCS { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
    }
}