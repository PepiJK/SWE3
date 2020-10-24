using System;
using SWE3.SeppMapper.Attributes;

namespace SWE3.TestApp.Models
{
    public class Person
    {
        [PrimaryKey]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        [Required]
        public DateTime BirthDate { get; set; }
        [ForeignKey(ReferencingType = typeof(Student), ReferencingColumn = "Id")]
        public int? StudentId { get; set; }
        public Student Student { get; set; }
    }
}