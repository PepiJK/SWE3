using System.Collections.Generic;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Helpers;

namespace SWE3.TestApp.Models
{
    public class Student
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int CurrentSemester { get; set; }
        [ForeignKey(Type = typeof(Person), ReferentialAction = ReferentialActions.Restrict)]
        public int PersonID { get; set; }
        public Person Person { get; set; }
        [ForeignKey(Type = typeof(University), ReferentialAction = ReferentialActions.Restrict)]
        public int UniversityId { get; set; }
        public University University { get; set; }
        public IList<StudentCourse> StudentCourses { get; set; }
    }
}