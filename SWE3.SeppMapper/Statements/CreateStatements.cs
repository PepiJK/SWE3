using System;
using System.Linq;
using System.Text;
using Npgsql;
using Serilog;
using SWE3.SeppMapper.Helpers;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper.Statements
{
    /// <summary>Postgres create table commands.</summary>
    public class CreateStatements
    {
        /// <summary>Holds the database connection string.</summary>
        private readonly string _connection;

        /// <summary>Set the database connection string.</summary>
        /// <param name="connection"></param>
        public CreateStatements(string connection)
        {
            _connection = connection;
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
                if (SeppEntityController.IsPropertySkippable(prop)) continue;

                stmt.Append($"{prop.Name} ");

                if (!prop.IsSerial) stmt.Append(PostgresTypeHelper.GetPostgresType(prop.Type));
                if (prop.IsSerial) stmt.Append("SERIAL");
                if (prop.IsRequired) stmt.Append(" NOT NULL");

                stmt.Append(", ");
            }

            stmt.Length = stmt.Length - 2;
            stmt.Append(")");

            return stmt.ToString();
        }


    }
}