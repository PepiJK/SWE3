using System;
using System.Text;
using Npgsql;
using Serilog;
using SWE3.SeppMapper.Models;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKata.Compilers;

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

        /// <summary>Remove an entity based on provided primary key(s).</summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKeys"></param>
        public void RemoveEntity(string tableName, IDictionary<string, object> primaryKeys)
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                var query = queryBuilder.Query(tableName);

                foreach(var pk in primaryKeys)
                {
                    query.Where(pk.Key, pk.Value);
                }

                query.Delete();
                Log.Debug($"DropDeleteStatements :: Dropped row in table table {tableName}");
            }
        }
        
        /// <summary>Drop an existing table and all its referential integrity constraints.</summary>
        /// <param name="table"></param>
        public void DropTable(Table table)
        {
            var stmt = new StringBuilder($"DROP TABLE {table.Name} CASCADE");
            Log.Information($"DropDeleteStatements :: {stmt}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"DropDeleteStatements :: Dropped table {table.Name}");
        }
    }
}