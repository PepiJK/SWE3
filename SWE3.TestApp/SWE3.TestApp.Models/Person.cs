using System;
using System.ComponentModel.DataAnnotations;

namespace SWE3.TestApp.Models
{
    public class Person
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName { get => FirstName + " " + LastName; }
        [Required]
        public DateTime BirthDate { get; set; }
        public int StudentID { get; set; }
        public Student Student { get; set; }
    }
}