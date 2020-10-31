using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class StudentCourse
    {
        [PrimaryKey]
        [Serial]
        public int Id { get; set; }
        public int? CurrentGrade { get; set; }
        [ForeignKey(ReferencingType = typeof(Student), ReferencingColumn = "Id")]
        public int StudentId { get; set; }
        public Student Student { get; set; }
        [ForeignKey(ReferencingType = typeof(Course), ReferencingColumn = "Id")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}