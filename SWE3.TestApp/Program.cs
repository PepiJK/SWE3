using System;
using System.Linq;
using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    class Program
    {
        /// <summary>
        /// Very simple app that queries, creates, updates and deletes entities defined in the TestAppContext.
        /// Should be used with appropriate breakpoints and the debugger to test the functionality.
        /// I suggest putting a breakpoint before the Delete Commands on line 75.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var context = new TestAppContext("Server=localhost;Port=5432;Database=sepp;User Id=sepp;Password=123456;");

            // Get persons from db
            var persons = context.Persons.Get();

            // Create a new Person
            var newPersonDb = context.Persons.Create(new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            });

            // Create a new University
            var newUniDb = context.Universities.Create(new University {
                Name = "FH",
                Address = "Wien"
            });

            // Create a new Course
            var newCourseDb = context.Courses.Create(new Course {
                ECTS = 3,
                Name = "SWE"
            });

            // Create a new Student with 1:1 relation to Person and n:1 relation to University
            var newStudentDb = context.Students.Create(new Student {
                CurrentSemester = 5,
                PersonId = newPersonDb.Id,
                UniversityId = newUniDb.Id
            });

            // Create new StudentCourse (m:n relation table between Student and Course)
            var newStudentCourseDb = context.StudentCourses.Create(new StudentCourse {
                CourseId = newCourseDb.Id,
                StudentId = newStudentDb.Id
            });

            // Update the newly created Person and set the 1:1 relation to Student
            newPersonDb.FirstName = "Joe";
            newPersonDb.LastName = "Cook";
            newPersonDb.StudentId = newStudentDb.Id;
            var updatedPersonDb = context.Persons.Update(newPersonDb);

            // Get Persons with specific expression from db
            var where1 = context.Persons.Get(p => p.Id == updatedPersonDb.Id);
            var where2 = context.Persons.Get(p => p.BirthDate < DateTime.Now);
            var where3 = context.Persons.Get(p => p.FirstName != "Josef");

            // Loading of related 1:1 entity could look like this (not ideal of course, but its posible)
            var personWithStudentAndCourses = context.Persons.Get(p => p.Id == updatedPersonDb.Id).FirstOrDefault();
            personWithStudentAndCourses.Student = context.Students.Get(s => s.Id == personWithStudentAndCourses.StudentId).FirstOrDefault();

            // Loading of related courses of the student (again not ideal, but possible)
            personWithStudentAndCourses.Student.StudentCourses = context.StudentCourses.Get(sc => sc.StudentId == personWithStudentAndCourses.Student.Id);
            personWithStudentAndCourses.Student.StudentCourses.ToList().ForEach(sc => sc.Course = context.Courses.Get(c => c.Id == sc.CourseId).FirstOrDefault());
            
            // Delete StudentCourse first because of default referential action defined in model is no action
            context.StudentCourses.Delete(newStudentCourseDb);

            // Delete Person wich also will delete related Student because of defined referantial action in Student model is cascade
            context.Persons.Delete(updatedPersonDb);

            // Delete Course and University
            context.Courses.Delete(newCourseDb);
            context.Universities.Delete(newUniDb);

            // Now there should be no rows left in the db
        }
    }
}
