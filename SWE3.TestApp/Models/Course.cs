using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int ETCS { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
    }
}