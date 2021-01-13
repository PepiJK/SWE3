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
            
            var dbPerson = context.Persons.Create(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(newPerson.FullName, dbPerson.FullName);
            Assert.NotNull(dbPerson.Id);
            Assert.NotZero(dbPerson.Id);
            Assert.AreEqual(dbPerson.Id, dbPersonFromList.Id);
            Assert.AreEqual(dbPerson.FullName, dbPersonFromList.FullName);
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
            
            var dbPerson = context.Persons.Create(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(newPerson.FullName, dbPerson.FullName);
            Assert.AreEqual(newPerson.Student.CurrentSemester, dbPerson.Student.CurrentSemester);
            Assert.NotNull(dbPerson.Id);
            Assert.NotZero(dbPerson.Id);
            Assert.NotNull(dbPerson.Student.Id);
            Assert.NotZero(dbPerson.Student.Id);
            Assert.AreEqual(dbPerson.Id, dbPerson.Student.PersonId);
            Assert.AreEqual(dbPerson.Id, dbPersonFromList.Id);
            Assert.AreEqual(dbPerson.FullName, dbPersonFromList.FullName);
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
            
            var dbPerson = context.Persons.Create(newPerson);
            var dbPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);
            var dbCourse = context.Courses.Create(newCourse);
            var dbCourseFromList = context.Courses.Get().FirstOrDefault(c => c.Id == dbCourse.Id);
            var dbStudentCourse = context.StudentCourses.Create(new StudentCourse {
                CourseId = dbCourse.Id,
                StudentId = dbPerson.Student.Id
            });
            var dbStudenCourseFromList = context.StudentCourses.Get().FirstOrDefault(sc => sc.CourseId == dbCourse.Id && sc.StudentId == dbPerson.StudentId);

            Assert.AreEqual(dbCourse.Id, dbStudentCourse.Course.Id);
            Assert.AreEqual(dbPerson.Student.Id, dbStudentCourse.Student.Id);
            Assert.AreEqual(dbPerson.Id, dbPersonFromList.Id);
            Assert.AreEqual(dbCourse.Id, dbCourseFromList.Id);
            Assert.AreEqual(dbStudentCourse.CourseId, dbStudenCourseFromList.CourseId);
            Assert.AreEqual(dbStudentCourse.Course.Id, dbStudenCourseFromList.Course.Id);
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
            
            var dbPerson = context.Persons.Create(newPerson);
            dbPerson.FirstName = "Joe";
            var dbUpdatedPerson = context.Persons.Update(dbPerson);
            var dbUpdatedPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(dbPerson.FirstName, dbUpdatedPerson.FirstName);
            Assert.AreEqual(dbPerson.FirstName, dbUpdatedPersonFromList.FirstName);
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
            
            var dbPerson = context.Persons.Create(newPerson);
            dbPerson.Student.CurrentSemester = 6;
            dbPerson.FirstName = "Joe";
            var dbUpdatedPerson = context.Persons.Update(dbPerson);
            var dbUpdatedPersonFromList = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.AreEqual(dbPerson.Id, dbUpdatedPerson.Id);
            Assert.AreEqual(dbPerson.FullName, dbUpdatedPerson.FullName);
            Assert.AreEqual(dbPerson.Id, dbUpdatedPersonFromList.Id);
            Assert.AreEqual(dbPerson.Student.Id, dbUpdatedPersonFromList.Student.Id);
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

            var dbPerson = context.Persons.Create(newPerson);
            var dbCourse = context.Courses.Create(newCourse);
            var dbStudentCourse = context.StudentCourses.Create(new StudentCourse {
                CourseId = dbCourse.Id,
                StudentId = dbPerson.Student.Id
            });

            dbStudentCourse.CurrentGrade = 3;
            var dbUpdatedStudenCourse = context.StudentCourses.Update(dbStudentCourse);
            var dbUpdatedStudenCourseFromList = context.StudentCourses.Get().FirstOrDefault(sc => sc.CourseId == dbCourse.Id && sc.StudentId == dbPerson.StudentId);

            Assert.AreEqual(dbStudentCourse.CurrentGrade, dbUpdatedStudenCourse.CurrentGrade);
            Assert.AreEqual(dbStudentCourse.CurrentGrade, dbUpdatedStudenCourseFromList.CurrentGrade);
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
            
            var dbPerson = context.Persons.Create(newPerson);
            context.Persons.Delete(dbPerson);
            var dbRemovedPerson = context.Persons.Get().FirstOrDefault(p => p.Id == dbPerson.Id);

            Assert.IsNull(dbRemovedPerson);
        }

        [Test]
        public void Should_remove_entity_with_underlying_entity()
        {
            var context = new TestAppContext(connection);

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

            newPersonDb.StudentId = newStudentDb.Id;
            var updatedPersonDb = context.Persons.Update(newPersonDb);

            context.Persons.Delete(updatedPersonDb);
            var dbRemovedPerson = context.Persons.FirstOrDefault(p => p.Id == updatedPersonDb.Id);
            var dbRemovedStudent = context.Students.FirstOrDefault(p => p.Id == newStudentDb.Id);

            Assert.IsNull(dbRemovedPerson);
            Assert.IsNull(dbRemovedStudent);
        }

    }
}