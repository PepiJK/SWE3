using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class StudentCourse
    {
        [PrimaryKeyAttribute]
        public int Id { get; set; }
        public int? CurrentGrade { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}