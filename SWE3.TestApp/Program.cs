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

            // Where test
            var where1 = context.Persons.Get(p => p.Id == updatedPersonDb.Id);
            var where2 = context.Persons.Get(p => p.BirthDate < DateTime.Now);
            var where3 = context.Persons.Get(p => p.FirstName != "Josef");
            
            // Delete test
            context.Persons.Delete(updatedPersonDb);
            context.Universities.Delete(newUniDb);
            context.Students.Delete(newStudentDb);
        }
    }
}
