using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Student
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int CurrentSemester { get; set; }
        public int PersonID { get; set; }
        public Person Person { get; set; }
        public int UniversityId { get; set; }
        public University University { get; set; }
        public IList<StudentCourse> StudentCourses { get; set; }
    }
}