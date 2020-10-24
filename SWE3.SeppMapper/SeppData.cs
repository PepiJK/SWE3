using System.Collections;
using System.Collections.Generic;
using SWE3.SeppMapper.Models;
using System.Linq;
using Serilog;
using Microsoft.Data.Sqlite;
using System.IO;
using System;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Text;

namespace SWE3.SeppMapper
{
    public static class SeppData
    {
        private static string _connection = "Data Source=Data/sqlite.db";
        private static IEnumerable<Entity> Entities { get; set; }
        private static IDictionary<Type, string> DotNet2SqliteTypeDict = new Dictionary<Type, string>{
            {typeof(Int32), "INTEGER"}, {typeof(Nullable<Int32>), "INTEGER"}, {typeof(String), "TEXT"}, {typeof(DateTime), "TEXT"}
        };

        public static void Initilize(IEnumerable<Entity> entities)
        {
            Entities = entities;

            foreach (var entity in entities)
            {
                if (TableExists(entity))
                {
                    Log.Debug($"SeppData :: Table {entity.Type.Name} exists, updating...");
                    // TODO: Lookup tables and check properties for differences
                }
                else
                {
                    Log.Debug($"SeppData :: Table {entity.Type.Name} does not exist, creating...");
                    CreateTable(entity);
                    Log.Debug($"SeppData :: Created table {entity.Type.Name}");
                }
            }

            // TODO: Setup Relations
            foreach (var entity in entities)
            {
                AddForeignKeys(entity);
            }
        }

        private static bool TableExists(Entity entity)
        {
            using (var queryBuilder = new QueryFactory(new SqliteConnection(_connection), new SqliteCompiler()))
            {
                return queryBuilder.Query("sqlite_master").Select("name").Where("type", "table").Where("name", entity.Type.Name).Get().Count() == 1;
            }
        }

        private static void AddForeignKeys(Entity entity)
        {
            var props = entity.Properties.Where(p => p.ForeignKeyInfo != null);

            using (var sqlite = new SqliteConnection(_connection))
            {
                sqlite.Open();

                foreach (var prop in props)
                {
                    Log.Debug($"SeppData :: Adding foreign key {prop.Name} to {entity.Type.Name}");

                    var command = sqlite.CreateCommand();
                    var stmt = new StringBuilder($"ALTER TABLE {entity.Type.Name} ADD COLUMN {prop.Name} {GetSqliteType(prop.Type)}");

                    // TODO: Add not null constraint (maybe sqlite forces foreign key to be default null) and referential action

                    stmt.Append($" REFERENCES {prop.ForeignKeyInfo.ReferencingType.Name}({prop.ForeignKeyInfo.ReferencingColumn})");

                    Log.Information(stmt.ToString());
                    command.CommandText = stmt.ToString();
                    command.ExecuteNonQuery();
                }
            }
            
        }

        private static void CreateTable(Entity entity)
        {
            var createTableStatement = BuildCreateStatement(entity);

            using (var sqlite = new SqliteConnection(_connection))
            {
                sqlite.Open();
                var command = sqlite.CreateCommand();

                command.CommandText = createTableStatement;
                Log.Information($"SeppData :: {command.CommandText}");

                command.ExecuteNonQuery();                
            }
        }

        private static string BuildCreateStatement(Entity entity)
        {
            var stmt = new StringBuilder($"CREATE TABLE {entity.Type.Name} (");

            foreach (var prop in entity.Properties)
            {
                if (SkipProperty(prop)) continue;

                stmt.Append($"{prop.Name} ");
                stmt.Append(GetSqliteType(prop.Type));

                // TODO: remove not null on primary key, maybe make int default 0?
                    
                if (prop.IsPrimaryKey) stmt.Append(" PRIMARY KEY");
                if (prop.IsRequired) stmt.Append(" NOT NULL");

                stmt.Append(", ");
            }

            stmt.Length = stmt.Length - 2;
            stmt.Append(")");

            return stmt.ToString();
        }

        private static string GetSqliteType(Type dotNetType)
        {
            string sqliteType;
            if (DotNet2SqliteTypeDict.TryGetValue(dotNetType, out sqliteType))
            {
                return sqliteType;
            }
            // TODO: Create custom Exception
            Log.Error($"SeppData :: Type {dotNetType.Name} is not supported.");
            throw new Exception($"Type {dotNetType.Name} is not supported.");
        }

        private static bool SkipProperty(Property property)
        {
            if (property.ForeignKeyInfo != null) return true;
            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
            return Entities.Select(e => e.Type).Contains(property.Type);
        }

  
    }
}