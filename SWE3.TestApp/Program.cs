using System;
using System.Linq;
using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new TestAppContext("Server=localhost;Port=5432;Database=sepp;User Id=sepp;Password=123456;");

            // Get test
            var persons = context.Persons.Get();
            var persons2 = context.Persons.Get();
            var persons3 = persons.Get();

            // Save test
            var newPersonDb = context.Persons.Create(new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            });

            var newUniDb = context.Universities.Create(new University {
                Name = "FH",
                Address = "Wien"
            });

            var newStudentDb = context.Students.Create(new Student {
                CurrentSemester = 5,
                PersonId = newPersonDb.Id,
                UniversityId = newUniDb.Id
            });

            // Update test
            newPersonDb.FirstName = "Joe";
            newPersonDb.LastName = "Cook";
            newPersonDb.StudentId = newStudentDb.Id;
            var updatedPersonDb = context.Persons.Update(newPersonDb);

            // Delete test
            context.Persons.Delete(updatedPersonDb);
            context.Universities.Delete(newUniDb);
            context.Students.Delete(newStudentDb);
        }
    }
}
