using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Course
    {
        [PrimaryKeyAttribute]
        public int Id { get; set; }
        [RequiredAttribute]
        public string Name { get; set; }
        public int ETCS { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
    }
}