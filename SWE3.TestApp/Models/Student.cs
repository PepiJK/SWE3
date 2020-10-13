using System.Collections.Generic;

namespace SWE3.TestApp.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int CurrentSemester { get; set; }
        public int PersonID { get; set; }
        public Person Person { get; set; }
        public int UniversityID { get; set; }
        public University University { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
    }
}