using System;
using System.Text;
using Npgsql;
using Serilog;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper.Statements
{
    /// <summary>Postgres drop table and delete commands.</summary>
    public class DropDeleteStatements
    {
        /// <summary>Holds the database connection string.</summary>
        private readonly string _connection;
        
        /// <summary>Set the database connection string.</summary>
        /// <param name="connection"></param>
        public DropDeleteStatements(string connection)
        {
            _connection = connection;
        }
        
        /// <summary>Drop an existing table and all its referential integrity constraints.</summary>
        /// <param name="table"></param>
        public void DropTable(Table table)
        {
            var stmt = new StringBuilder($"DROP TABLE {table.Name} CASCADE");
            Log.Information($"DropStatements :: {stmt}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"DropStatements :: Dropped table {table.Name}");
        }
    }
}