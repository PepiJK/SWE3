using System;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Helpers;

namespace SWE3.TestApp.Models
{
    public class Person
    {
        [PrimaryKey]
        [Serial]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        [Required]
        public DateTime BirthDate { get; set; }
        [ForeignKey(ReferencingType = typeof(Student), ReferencingColumn = "Id", ReferentialAction = ReferentialActions.SetNull)]
        public int? StudentId { get; set; }
        public Student Student { get; set; }
    }
}