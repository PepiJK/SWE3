using Npgsql;
using NUnit.Framework;
using SWE3.SeppMapper;
using SWE3.SeppMapper.Models;
using SWE3.SeppMapper.Statements;
using SWE3.TestApp;
using SWE3.TestApp.Models;
using System;
using System.Linq;

namespace SWE3.Tests
{
    public class Tests
    {
        private string connection = "Server=localhost;Port=5432;Database=sepptest;User Id=test;Password=test;";

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            // drop every table in the test db
            using (var pg = new NpgsqlConnection(connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                var select = new SelectStatements(connection);
                var tables = select.GetAllTablesFromDb();
                foreach(var table in tables)
                {
                    command.CommandText += $"DROP TABLE {table.Name} CASCADE;";
                }

                command.ExecuteNonQuery();
            }
        }


        [Test]
        public void Should_create_tables()
        {
            var context = new TestAppContext(connection);
            
            var select = new SelectStatements(connection);
            var tables = select.GetAllTablesFromDb();

            Assert.AreEqual(5, tables.Count());
        }

        [Test]
        public void Should_persist_tables()
        {
            var context = new TestAppContext(connection);
            context = new TestAppContext(connection);

            var select = new SelectStatements(connection);
            var tables = select.GetAllTablesFromDb();

            Assert.AreEqual(5, tables.Count());
        }


        [Test]
        public void Should_save_and_get_new_simple_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(newPerson.FullName, dbPerson.FullName);
            Assert.NotNull(dbPerson.Id);
            Assert.NotZero(dbPerson.Id);
            Assert.AreEqual(dbPerson, dbPersonFromList);
        }

        [Test]
        public void Should_save_and_get_new_11_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now,
                Student = new Student {
                    CurrentSemester = 5
                }
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(newPerson.FullName, dbPerson.FullName);
            Assert.AreEqual(newPerson.Student.CurrentSemester, dbPerson.Student.CurrentSemester);
            Assert.NotNull(dbPerson.Id);
            Assert.NotZero(dbPerson.Id);
            Assert.NotNull(dbPerson.Student.Id);
            Assert.NotZero(dbPerson.Student.Id);
            Assert.AreEqual(dbPerson.Id, dbPerson.Student.PersonId);
            Assert.AreEqual(dbPerson, dbPersonFromList);
        }

        [Test]
        public void Should_save_and_get_new_mn_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now,
                Student = new Student {
                    CurrentSemester = 5
                }
            };
            var newCourse = new Course {
                ECTS = 3,
                Name = "SWE3"
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);
            var dbCourse = context.Courses.Add(newCourse);
            var dbCourseFromList = context.Courses.Get().FirstOrDefault(c => c.Id == dbCourse.Id);
            var dbStudentCourse = context.StudentCourses.Add(new StudentCourse {
                CourseId = dbCourse.Id,
                StudentId = dbPerson.Student.Id
            });
            var dbStudenCourseFromList = context.StudentCourses.Get().FirstOrDefault(sc => sc.CourseId == dbCourse.Id && sc.StudentId == dbPerson.StudentId);

            Assert.AreEqual(dbCourse, dbStudentCourse.Course);
            Assert.AreEqual(dbPerson.Student, dbStudentCourse.Student);
            Assert.AreEqual(dbPerson, dbPersonFromList);
            Assert.AreEqual(dbCourse, dbCourseFromList);
            Assert.AreEqual(dbStudentCourse, dbStudenCourseFromList);
        }

        [Test]
        public void Should_update_and_get_simple_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            dbPerson.FirstName = "Joe";
            var dbUpdatedPerson = context.Persons.Update(dbPerson);
            var dbUpdatedPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(dbPerson, dbUpdatedPerson);
            Assert.AreEqual(dbPerson, dbUpdatedPersonFromList);
        }

        [Test]
        public void Should_update_and_get_11_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now,
                Student = new Student {
                    CurrentSemester = 5
                }
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            dbPerson.Student.CurrentSemester = 6;
            dbPerson.FirstName = "Joe";
            var dbUpdatedPerson = context.Persons.Update(dbPerson);
            var dbUpdatedPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(dbPerson, dbUpdatedPerson);
            Assert.AreEqual(dbPerson, dbUpdatedPersonFromList);
        }

        [Test]
        public void Should_update_and_get_mn_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now,
                Student = new Student {
                    CurrentSemester = 5
                }
            };
            var newCourse = new Course {
                ECTS = 3,
                Name = "SWE3"
            };

            var dbPerson = context.Persons.Add(newPerson);
            var dbCourse = context.Courses.Add(newCourse);
            var dbStudentCourse = context.StudentCourses.Add(new StudentCourse {
                CourseId = dbCourse.Id,
                StudentId = dbPerson.Student.Id
            });

            dbStudentCourse.CurrentGrade = 3;
            var dbUpdatedStudenCourse = context.StudentCourses.Update(dbStudentCourse);
            var dbUpdatedStudenCourseFromList = context.StudentCourses.Get().FirstOrDefault(sc => sc.CourseId == dbCourse.Id && sc.StudentId == dbPerson.StudentId);

            Assert.AreEqual(dbStudentCourse, dbUpdatedStudenCourse);
            Assert.AreEqual(dbStudentCourse, dbUpdatedStudenCourseFromList);
        }

        [Test]
        public void Should_remove_simple_entity()
        {
            var context = new TestAppContext(connection);
            var newPerson = new Person {
                FirstName = "Josef",
                LastName = "Koch",
                BirthDate = DateTime.Now
            };
            
            var dbPerson = context.Persons.Add(newPerson);
            context.Persons.Remove(dbPerson);
            var dbRemovedPerson = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.IsNull(dbRemovedPerson);
        }

    }
}