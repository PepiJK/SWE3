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
        [ForeignKey(ReferencingType = typeof(Person), ReferencingColumn = "Id", ReferentialAction = ReferentialActions.Restrict)]
        public int PersonId { get; set; }
        public Person Person { get; set; }
        [ForeignKey(ReferencingType = typeof(University), ReferencingColumn = "Id", ReferentialAction = ReferentialActions.Restrict)]
        public int UniversityId { get; set; }
        public University University { get; set; }
        public IEnumerable<StudentCourse> StudentCourses { get; set; }
    }
}