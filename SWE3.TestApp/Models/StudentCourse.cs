using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class StudentCourse
    {
        [PrimaryKey]
        public int Id { get; set; }
        public int? CurrentGrade { get; set; }
        [ForeignKey(Type = typeof(Student))]
        public int StudentId { get; set; }
        public Student Student { get; set; }
        [ForeignKey(Type = typeof(Course))]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}