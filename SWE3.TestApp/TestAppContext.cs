using System;
using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    public class TestAppContext : SeppContext
    {
        public TestAppContext(string connection) : base(connection) {}

        public SeppSet<Person> Persons { get; set; }
        public SeppSet<Course> Courses { get; set; }
        public SeppSet<Student> Students { get; set; }
        public SeppSet<StudentCourse> StudentCourses { get; set; }
        public SeppSet<University> Universities { get; set; }
    }
}