using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Course
    {
        [PrimaryKey]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int ETCS { get; set; }
        public IList<StudentCourse> StudentCourses { get; set; }
    }
}