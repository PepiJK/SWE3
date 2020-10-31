using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Course
    {
        [PrimaryKey]
        [Serial]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int ECTS { get; set; }
        public IEnumerable<StudentCourse> StudentCourses { get; set; }
    }
}