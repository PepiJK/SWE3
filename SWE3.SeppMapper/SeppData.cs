using System.Collections.Generic;
using SWE3.SeppMapper.Models;
using System.Linq;
using Serilog;
using System;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Text;
using Npgsql;
using SWE3.SeppMapper.Helpers;

namespace SWE3.SeppMapper
{
    public static class SeppData
    {
        private static string _connection;
        private static IEnumerable<Entity> _entities;
        // TODO: PG
        private static IDictionary<Type, string> DotNet2PgTypeDict = new Dictionary<Type, string>{
            {typeof(Int32), "INTEGER"}, {typeof(Nullable<Int32>), "INTEGER"}, {typeof(String), "TEXT"}, {typeof(DateTime), "TIMESTAMP WITHOUT TIME ZONE"}
        };

        public static void Initilize(IEnumerable<Entity> entities, string connection)
        {
            _entities = entities;
            _connection = connection;


            var newEntities = new List<Entity>();
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
                    newEntities.Add(entity);
                    Log.Debug($"SeppData :: Created table {entity.Type.Name}");
                }
            }

            // TODO: Setup Relations
            foreach (var entity in newEntities)
            {
                AddForeignKeys(entity);
            }
        }

        private static bool TableExists(Entity entity)
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                return queryBuilder.Query("information_schema.tables").Select("table_name").Where("table_name", entity.Type.Name.ToLower()).Get().Count() == 1;
            }
        }

        private static void AddForeignKeys(Entity entity)
        {
            var props = entity.Properties.Where(p => p.ForeignKeyInfo != null);

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();

                foreach (var prop in props)
                {
                    Log.Debug($"SeppData :: Adding foreign key {prop.Name} to {entity.Type.Name}");

                    var command = pg.CreateCommand();
                    var foreingKeyCommand = pg.CreateCommand();
                    var stmt = new StringBuilder();

                    stmt.Append($"ALTER TABLE {entity.Type.Name} ADD COLUMN {prop.Name} {GetPostgresType(prop.Type)}");

                    if (prop.IsRequired) stmt.Append(" NOT NULL");

                    stmt.Append($" REFERENCES {prop.ForeignKeyInfo.ReferencingType.Name}({prop.ForeignKeyInfo.ReferencingColumn})");

                    switch (prop.ForeignKeyInfo.ReferentialAction)
                    {
                        case ReferentialActions.Cascade:
                            stmt.Append(" ON DELETE CASCADE");
                            break;

                        case ReferentialActions.NoAction:
                            stmt.Append(" ON DELETE NO ACTION");
                            break;

                        case ReferentialActions.SetNull:
                            stmt.Append(" ON DELETE SET NULL");
                            break;

                        case ReferentialActions.Restrict:
                            stmt.Append(" ON DELETE RESTRICT");
                            break;

                        default:
                            stmt.Append(" ON DELETE NO ACTION");
                            break;
                    }

                    Log.Information(stmt.ToString());
                    command.CommandText = stmt.ToString();
                    command.ExecuteNonQuery();
                }
            }

        }

        private static void CreateTable(Entity entity)
        {
            var createTableStatement = BuildCreateStatement(entity);

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

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

                if (!prop.IsSerial) stmt.Append(GetPostgresType(prop.Type));
                if (prop.IsSerial) stmt.Append("SERIAL");

                if (prop.IsPrimaryKey) stmt.Append(" PRIMARY KEY");
                if (prop.IsRequired) stmt.Append(" NOT NULL");

                stmt.Append(", ");
            }

            stmt.Length = stmt.Length - 2;
            stmt.Append(")");

            return stmt.ToString();
        }

        private static string GetPostgresType(Type dotNetType)
        {
            string pgType;
            if (DotNet2PgTypeDict.TryGetValue(dotNetType, out pgType))
            {
                return pgType;
            }
            // TODO: Create custom Exception
            Log.Error($"SeppData :: Type {dotNetType.Name} is not supported.");
            throw new Exception($"Type {dotNetType.Name} is not supported.");
        }

        private static bool SkipProperty(Property property)
        {
            if (property.ForeignKeyInfo != null) return true;
            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
            return _entities.Select(e => e.Type).Contains(property.Type);
        }


    }
}