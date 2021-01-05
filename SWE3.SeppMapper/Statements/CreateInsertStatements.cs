using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using SWE3.SeppMapper.Helpers;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper.Statements
{
    /// <summary>Postgres create table and insert commands.</summary>
    public class CreateInsertStatements
    {
        /// <summary>Holds the database connection string.</summary>
        private readonly string _connection;

        /// <summary>Set the database connection string.</summary>
        /// <param name="connection"></param>
        public CreateInsertStatements(string connection)
        {
            _connection = connection;
        }

        /// <summary>Inserts a new row.</summary>
        /// <returns>Dictionary (column name, value) of the new primary keys.</returns>
        public IDictionary<string, object> InsertRow(string tableName, IDictionary<string, object> data, IEnumerable<string> primaryKeys)
        {
            var newPks = new Dictionary<string, object>();

            var stmt = new StringBuilder($"INSERT INTO {tableName} (");
            foreach (var element in data)
            {
                stmt.Append($"{element.Key}, ");
            }
            stmt.Length -= 2;
            
            stmt.Append(") VALUES (");
            foreach (var element in data)
            {
                stmt.Append($"@{element.Key}, ");
            }
            stmt.Length -= 2;
            
            stmt.Append(") RETURNING ");
            foreach (var pk in primaryKeys)
            {
                stmt.Append($"{pk}, ");
            }
            stmt.Length -= 2;
            
            Log.Information($"CreateStatements :: {stmt}");
            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                foreach (var element in data)
                {
                    command.Parameters.AddWithValue(element.Key, element.Value);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int index = 0;
                        foreach (var pk in primaryKeys)
                        {
                            newPks.Add(pk, reader.GetValue(index));
                            index++;
                        }
                    }
                }
                Log.Debug($"CreateStatements :: Inserted new row in table {tableName}");

            }

            return newPks;
        }

        /// <summary> Create a new table.</summary>
        /// <param name="entity"></param>
        public void CreateTable(Entity entity)
        {
            var stmt = BuildCreateStatement(entity);
            Log.Information($"CreateStatements :: {stmt}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt;
                command.ExecuteNonQuery();
            }

            Log.Debug($"CreateStatements :: Created table {entity.Type.Name.ToLower()}");
        }

        /// <summary>Build the Postgres create table statement.</summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string BuildCreateStatement(Entity entity)
        {
            var stmt = new StringBuilder($"CREATE TABLE {entity.Type.Name} (");

            foreach (var prop in entity.Properties)
            {
                if (SeppContextController.IsPropertySkippable(prop)) continue;

                stmt.Append($"{prop.Name} ");

                if (!prop.IsSerial) stmt.Append(PostgresTypeHelper.GetPostgresType(prop.Type));
                if (prop.IsSerial) stmt.Append("SERIAL");
                if (prop.IsRequired) stmt.Append(" NOT NULL");

                stmt.Append(", ");
            }

            stmt.Length -= 2;
            stmt.Append(")");

            return stmt.ToString();
        }


    }
}