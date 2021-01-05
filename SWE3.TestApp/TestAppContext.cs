using System;
using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    public class TestAppContext : SeppContext
    {
        public TestAppContext(string connection) : base(connection) {}

        public SeppSet<Person> Persons { get; set; } = new SeppSet<Person>();
        public SeppSet<Course> Courses { get; set; } = new SeppSet<Course>();
        public SeppSet<Student> Students { get; set; } = new SeppSet<Student>();
        public SeppSet<StudentCourse> StudentCourses { get; set; } = new SeppSet<StudentCourse>();
        public SeppSet<University> Universities { get; set; } = new SeppSet<University>();
    }
}