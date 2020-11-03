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
    //TODO: Refactor, make not static, review toLower usage
    public static class SeppData
    {
        private static string _connection;
        private static IEnumerable<Entity> _entities;
        private static readonly IDictionary<Type, string> Type2PgTypeDict = new Dictionary<Type, string>{
            {typeof(int), "INTEGER"}, {typeof(int?), "INTEGER"}, {typeof(string), "TEXT"}, {typeof(DateTime), "TIMESTAMP WITHOUT TIME ZONE"}, {typeof(float), "REAL"}, {typeof(bool), "BIT(1)"}
        };

        public static void Initilize(IEnumerable<Entity> entities, string connection)
        {
            _entities = entities;
            _connection = connection;

            var remainingTables = GetAllTablesFromDb().ToList();
            var newlyCreatedEntities = new List<Entity>();
            var tablesToUpdate = new Dictionary<Entity, Table>();

            foreach (var entity in entities)
            {
                if (remainingTables.Where(t => t.Name == entity.Type.Name.ToLower()).Count() == 1)
                {
                    var table = remainingTables.First(t => t.Name == entity.Type.Name.ToLower());
                    tablesToUpdate.Add(entity, table);
                    remainingTables.Remove(table);
                }
                else
                {
                    Log.Debug($"SeppData :: Entity {entity.Type.Name} does not exist as table, creating table...");
                    CreateTable(entity);
                    newlyCreatedEntities.Add(entity);
                }
            }

            foreach (var table in remainingTables)
            {
                Log.Debug($"SeppData :: Dropping not needed remaining table {table.Name}...");
                DropTable(table);
            }

            foreach(var table in tablesToUpdate)
            {
                // TODO: Also update foreign Keys
                Log.Debug($"SeppData :: Table {table.Value.Name} exists, updating table...");
                UpdateTableColumns(table.Key, table.Value);
            }

            foreach (var entity in newlyCreatedEntities)
            {
                Log.Debug($"SeppData :: Adding foreign key contraints from entity {entity.Type.Name} to corresponding table");
                AddForeignKeyContraints(entity);
            }
        }

        private static void DropTable(Table table)
        {
            var stmt = new StringBuilder($"DROP TABLE {table.Name} CASCADE");

            Log.Information($"SeppData :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }
        }

        // TODO: Also update foreign keys
        private static void UpdateTableColumns(Entity entity, Table table)
        {
            // only properties and columns which are not primary and foreign keys
            var entityProps = entity.Properties.Where(p => !p.IsPrimaryKey && p.ForeignKeyInfo == null).ToList();
            var tableColumns = table.Columns.Where(c => !c.IsPrimary && c.ForeignKeyInfo == null).ToList();
            var tableHasUpdated = false;

            foreach (var col in tableColumns)
            {
                if (entityProps.Where(p => 
                    p.Name.ToLower() == col.Name && 
                    GetPostgresType(p.Type).ToLower() == col.Type &&
                    p.IsSerial == col.IsSerial &&
                    p.IsRequired == col.IsRequired).Count() == 1)
                {
                    // Table column is represented in the entity
                    var prop = entityProps.First(p => p.Name.ToLower() == col.Name);
                    entityProps.Remove(prop);
                }
                else
                {
                    // Table column is not represented in the entity
                    DropColumnFromTable(table, col);
                    Log.Debug($"SeppData :: Dropped column {col.Name} from table {table.Name}");
                    tableHasUpdated = true;
                }
            }

            // Add properties to table which have not been removed from entityProps list before
            foreach (var prop in entityProps)
            {
                if (SkipProperty(prop)) continue;
                AddPropertyToTable(table, prop);
                Log.Debug($"SeppData :: Added new property {prop.Name} to existing table {table.Name}");
                tableHasUpdated = true;
            }

            if (!tableHasUpdated) Log.Debug($"SeppData :: done, nothing to update");
        }

        private static void DropColumnFromTable(Table table, Column column)
        {
            var stmt = new StringBuilder($"ALTER TABLE {table.Name} DROP COLUMN {column.Name}");

            Log.Information($"SeppData :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }
        }

        private static void AddPropertyToTable(Table table, Property prop)
        {
            var stmt = new StringBuilder($"ALTER TABLE {table.Name} ADD COLUMN {prop.Name} ");

            if (!prop.IsSerial) stmt.Append(GetPostgresType(prop.Type));
            if (prop.IsSerial) stmt.Append("SERIAL");

            if (prop.IsRequired) stmt.Append(" NOT NULL");

            Log.Information($"SeppData :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }
        }

        private static bool TableExists(Entity entity)
        {
            using (var queryBuilder = new QueryFactory(new NpgsqlConnection(_connection), new PostgresCompiler()))
            {
                return queryBuilder.Query("information_schema.tables").Select("table_name").Where("table_name", entity.Type.Name.ToLower()).Get().Count() == 1;
            }
        }

        private static IEnumerable<Table> GetAllTablesFromDb()
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
                    foreach (var col in columnsInfo)
                    {
                        ForeignKeyInfo foreignKeyInfo;
                        foreignKeyInfoDict.TryGetValue(col.column_name, out foreignKeyInfo);

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
                        Columns = columns
                    });
                }
                
                Log.Debug($"SeppData :: Gathered {tables.Count()} Tables from Database");
                return tables;
            }
        }

        // key = column name
        private static IDictionary<string, ForeignKeyInfo> GetForeignKeyInfo(string tableName)
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

                var foreignKeyInfoDict = new Dictionary<string, ForeignKeyInfo>();
                var command = pg.CreateCommand();
                command.CommandText = stmt.ToString();

                Log.Information($"SeppData :: {stmt.ToString()}");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foreignKeyInfoDict.Add(
                            reader.GetString(0),
                            new ForeignKeyInfo{
                                ConstraintName =  reader.GetString(1),
                                ReferencingTable = reader.GetString(2),
                                ReferencingColumn = reader.GetString(3)
                            }
                        );
                    }
                }

                return foreignKeyInfoDict;
            }
        }

        private static IEnumerable<string> GetPrimaryKeyColumnNames(string tableName)
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

                Log.Information($"SeppData :: {stmt.ToString()}");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        primaryKeyColumns.Add(reader.GetString(0));
                    }
                }

                return primaryKeyColumns;
            }
        }

        private static void AddForeignKeyContraints(Entity entity)
        {
            var props = entity.Properties.Where(p => p.ForeignKeyInfo != null);

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();

                foreach (var prop in props)
                {
                    var command = pg.CreateCommand();
                    var stmt = new StringBuilder();

                    stmt.Append($"ALTER TABLE {entity.Type.Name} ADD CONSTRAINT {entity.Type.Name.ToLower()}_{prop.Name.ToLower()}_fkey FOREIGN KEY ({prop.Name.ToLower()}) ");
                    stmt.Append($"REFERENCES {prop.ForeignKeyInfo.ReferencingType.Name}({prop.ForeignKeyInfo.ReferencingColumn})");

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

                    Log.Debug($"SeppData :: Adding foreign key {prop.Name} to {entity.Type.Name}");
                    Log.Information($"SeppData :: {stmt.ToString()}");
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

            Log.Debug($"SeppData :: Created table {entity.Type.Name.ToLower()}");
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
            if (Type2PgTypeDict.TryGetValue(dotNetType, out pgType))
            {
                return pgType;
            }
            
            Log.Error($"SeppData :: Type {dotNetType.Name} is not supported."); 
            throw new Exception($"Type {dotNetType.Name} is not supported. If its an entity type, you should add it to your context class."); // TODO: Create custom Exception
        }

        private static bool SkipProperty(Property property)
        {
            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
            return _entities.Select(e => e.Type).Contains(property.Type);
        }


    }
}