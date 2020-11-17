using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class StudentCourse
    {
        public int? CurrentGrade { get; set; }

        [PrimaryKey]
        [ForeignKey(ReferencingType = typeof(Student), ReferencingColumn = "Id")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [PrimaryKey]
        [ForeignKey(ReferencingType = typeof(Course), ReferencingColumn = "Id")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}