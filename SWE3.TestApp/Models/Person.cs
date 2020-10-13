using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Person
    {
        [PrimaryKeyAttribute]
        public int Id { get; set; }
        [RequiredAttribute]
        public string FirstName { get; set; }
        [RequiredAttribute]
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        [RequiredAttribute]
        public DateTime BirthDate { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}