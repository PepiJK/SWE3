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

            // Cache test
            var persons = context.Persons.Get();
            var persons2 = context.Persons.Get();
            var persons3 = persons.Get();

            // Save test
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            };
            
            var newPersondb = context.Persons.Add(newPerson);
        }
    }
}
