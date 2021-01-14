using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Npgsql;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper.Statements
{
    /// <summary>Postgres select commands.</summary>
    public class SelectStatements
    {
        /// <summary>Holds the database connection string.</summary>
        private readonly string _connection;

        /// <summary>Set the database connection string.</summary>
        /// <param name="connection"></param>
        public SelectStatements(string connection)
        {
            _connection = connection;
        }

        /// <summary>Gets all rows of the provided TEntity type from the db.</summary>
        /// <returns>Queried entities.</returns>
        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                Log.Debug($"SelectStatements :: Select all rows from table {typeof(TEntity).Name.ToLower()}");
                return queryBuilder.Query(typeof(TEntity).Name.ToLower()).Select("*").Get<TEntity>();
            }
        }

        /// <summary>Gets all rows of the provided TEntity type with a simple where expression from the db.</summary>
        /// <returns>Queried entities.</returns>
        public IEnumerable<TEntity> GetEntities<TEntity>(string column, string whereOperator, object columnValue) where TEntity : class
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                Log.Debug($"SelectStatements :: Select all rows from table {typeof(TEntity).Name.ToLower()} where {column} {whereOperator} {columnValue.ToString()}");
                return queryBuilder.Query(typeof(TEntity).Name.ToLower()).Select("*").Where(column, whereOperator, columnValue).Get<TEntity>();
            }
        }

        /// <summary>Gets the first row of the provided TEntity type where the provided primary key(s) match</summary>
        /// <returns>Entity with matching primary key(s)</returns>
        public TEntity GetEntity<TEntity>(IDictionary<string, object> primaryKeys) where TEntity : class
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                Log.Debug($"SelectStatements :: Select one row from table {typeof(TEntity).Name.ToLower()} by its primary key(s)");

                var query = queryBuilder.Query(typeof(TEntity).Name.ToLower()).Select("*");

                foreach (var pk in primaryKeys)
                {
                    query.Where(pk.Key, pk.Value);
                }

                return query.FirstOrDefault<TEntity>();
            }
        }

        /// <summary>Check if a table with this name exists.</summary>
        /// <param name="tableName"></param>
        /// <returns>Boolean if table exists.</returns>
        public bool TableExists(string tableName)
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                Log.Debug($"SelectStatements :: Check if table {tableName} exists");
                return queryBuilder.Query("information_schema.tables").Select("table_name").Where("table_name", tableName).Get().Count() == 1;
            }
        }

        /// <summary>Get all tables form the database.</summary>
        /// <returns></returns>
        public IEnumerable<Table> GetAllTablesFromDb()
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                var tables = new List<Table>();
                var tableNames = queryBuilder.Query("information_schema.tables").Select("table_name").Where("table_schema", "public").Get<string>();
                foreach (var name in tableNames)
                {
                    var foreignKeyInfoDict = GetForeignKeyInfo(name);
                    var primaryKeys = GetPrimaryKeyColumnNames(name).ToList();

                    var columns = new List<Column>();
                    var columnsInfo = queryBuilder.Query("information_schema.columns").Select("column_name", "data_type", "is_nullable", "column_default").Where("table_name", name).Get();
                    var tableRowCount = queryBuilder.Query(name).SelectRaw("COUNT(*)").First<int>();
                    foreach (var col in columnsInfo)
                    {
                        foreignKeyInfoDict.TryGetValue(col.column_name, out ColumnForeignKeyInfo foreignKeyInfo);

                        columns.Add(new Column{
                            Name = col.column_name,
                            Type = col.data_type,
                            IsRequired = col.is_nullable == "NO",
                            IsSerial = col.column_default?.Contains("nextval") ?? false,
                            IsPrimary = primaryKeys.Contains(col.column_name),
                            ForeignKeyInfo = foreignKeyInfo
                        });
                    }

                    tables.Add(new Table{
                        Name = name,
                        Columns = columns,
                        HasRows = tableRowCount > 0
                    });
                }
                
                Log.Debug($"SelectStatements :: Gathered {tables.Count} Tables from the database");
                return tables;
            }
        }

        /// <summary>Gathers all the foreign key information of a table.</summary>
        /// <param name="tableName"></param>
        /// <returns>Dictionary (column name, foreign key info of this column)></returns>
        private IDictionary<string, ColumnForeignKeyInfo> GetForeignKeyInfo(string tableName)
        {
            var stmt = new StringBuilder(@"
                SELECT
                    kcu.column_name,
                    tc.constraint_name,
                    ccu.table_name AS foreign_table_name,
                    ccu.column_name AS foreign_column_name
                FROM
                    information_schema.table_constraints AS tc
                    JOIN information_schema.key_column_usage AS kcu
                    ON tc.constraint_name = kcu.constraint_name
                    AND tc.table_schema = kcu.table_schema
                    JOIN information_schema.constraint_column_usage AS ccu
                    ON ccu.constraint_name = tc.constraint_name
                    AND ccu.table_schema = tc.table_schema
                WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_name=
            ").Append($"'{tableName}'");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();

                var foreignKeyInfoDict = new Dictionary<string, ColumnForeignKeyInfo>();
                var command = pg.CreateCommand();
                command.CommandText = stmt.ToString();

                Log.Information($"SelectStatements :: {stmt.ToString()}");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foreignKeyInfoDict.Add(
                            reader.GetString(0),
                            new ColumnForeignKeyInfo{
                                ConstraintName = reader.GetString(1),
                                ReferencingTable = reader.GetString(2),
                                ReferencingColumn = reader.GetString(3)
                            }
                        );
                    }
                }

                Log.Debug($"SelectStatements :: Gathered foreing key information from {tableName}");
                return foreignKeyInfoDict;
            }
        }

        /// <summary>Get all primary keys of a table.</summary>
        /// <param name="tableName"></param>
        /// <returns>List of all primary key column names</returns>
        private IEnumerable<string> GetPrimaryKeyColumnNames(string tableName)
        {
            var stmt = new StringBuilder(@"
                SELECT
                    pg_attribute.attname
                FROM pg_index, pg_class, pg_attribute, pg_namespace
                WHERE
                    pg_class.oid = ").Append($"'{tableName}'").Append(@"::regclass AND
                    indrelid = pg_class.oid AND
                    nspname = 'public' AND
                    pg_class.relnamespace = pg_namespace.oid AND
                    pg_attribute.attrelid = pg_class.oid AND
                    pg_attribute.attnum = any(pg_index.indkey)
                    AND indisprimary
            ");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();

                var primaryKeyColumns = new List<string>();
                var command = pg.CreateCommand();
                command.CommandText = stmt.ToString();

                Log.Information($"SelectStatements :: {stmt.ToString()}");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        primaryKeyColumns.Add(reader.GetString(0));
                    }
                }

                Log.Debug($"SelectStatements :: Gathered primary key column names from {tableName}");
                return primaryKeyColumns;
            }
        }
    }
}