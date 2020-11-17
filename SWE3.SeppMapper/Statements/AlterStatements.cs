using System;
using System.Linq;
using System.Text;
using Npgsql;
using Serilog;
using SWE3.SeppMapper.Helpers;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper.Statements
{
    /// <summary>Postgres alter table commands.</summary>
    public class AlterStatements
    {
        /// <summary>Holds the database connection string.</summary>
        private readonly string _connection;

        /// <summary>Set the database connection string.</summary>
        /// <param name="connection"></param>
        public AlterStatements(string connection)
        {
            _connection = connection;
        }

        /// <summary>Drop all foreign key constraints of the provided table.</summary>
        /// <param name="table"></param>
        public void DropForeignKeyConstraintsOfTable(Table table)
        {
            foreach (var col in table.Columns)
            {
                if (col.ForeignKeyInfo != null)
                {
                    var stmt = new StringBuilder($"ALTER TABLE {table.Name} DROP CONSTRAINT {col.ForeignKeyInfo.ConstraintName}");
                    Log.Information($"AlterStatements :: {stmt}");

                    using (var pg = new NpgsqlConnection(_connection))
                    {
                        pg.Open();
                        var command = pg.CreateCommand();

                        command.CommandText = stmt.ToString();
                        command.ExecuteNonQuery();
                    }
                }
            }
            
            Log.Debug($"AlterStatements :: Dropped all foreign key constraints of table {table.Name}");
        }

        /// <summary>Drop primary key constraint of provided table. This only works if no foreign key constraint references the primary key.</summary>
        /// <param name="table"></param>
        public void DropPrimaryKeyConstraintOfTable(Table table)
        {
            var stmt = $"ALTER TABLE {table.Name} DROP CONSTRAINT IF EXISTS {table.Name}_pk";
            Log.Information($"AlterStatements :: {stmt}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"AlterStatements :: Dropped primary key constraint of table {table.Name}");
        }

        /// <summary>Drop a column in a table.</summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        public void DropColumnFromTable(Table table, Column column)
        {
            var stmt = new StringBuilder($"ALTER TABLE {table.Name} DROP COLUMN {column.Name}");
            Log.Information($"AlterStatements :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"AlterStatements :: Dropped column {column.Name} from {table.Name}");
        }

        /// <summary>Add a property to an existing table.</summary>
        /// <param name="table"></param>
        /// <param name="prop"></param>
        public void AddPropertyToTable(Table table, Property prop)
        {
            var stmt = new StringBuilder($"ALTER TABLE {table.Name} ADD COLUMN {prop.Name} ");

            if (!prop.IsSerial) stmt.Append(PostgresTypeHelper.GetPostgresType(prop.Type));
            if (prop.IsSerial) stmt.Append("SERIAL");
            if (prop.IsRequired) stmt.Append(" NOT NULL");

            Log.Information($"AlterStatements :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();
                var command = pg.CreateCommand();

                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"AlterStatements :: Added property {prop.Name.ToLower()} to {table.Name}");
        }

        /// <summary>Add primary key constraint of en entity to an exisiting table.</summary>
        /// <param name="entity"></param>
        public void AddPrimaryKeyConstraint(Entity entity)
        {
            var stmt = new StringBuilder($"ALTER TABLE {entity.Type.Name.ToLower()} ADD CONSTRAINT {entity.Type.Name.ToLower()}_pk PRIMARY KEY (");

            var primaryProps = entity.Properties.Where(p => p.IsPrimaryKey);
            foreach(var primaryProp in primaryProps)
            {   
                stmt.Append($"{primaryProp.Name.ToLower()}, ");
            }

            stmt.Length = stmt.Length - 2;
            stmt.Append(")");

            Log.Information($"AlterStatements :: {stmt.ToString()}");

            using (var pg = new NpgsqlConnection(_connection))
            {
                pg.Open();

                var command = pg.CreateCommand();
                command.CommandText = stmt.ToString();
                command.ExecuteNonQuery();
            }

            Log.Debug($"AlterStatements :: Added primary key constraint to table {entity.Type.Name.ToLower()}");
        }

        /// <summary>Add all foreign key constraints of an entity to an existing table.</summary>
        /// <param name="entity"></param>
        public void AddForeignKeyConstraints(Entity entity)
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

                    Log.Information($"AlterStatements :: {stmt.ToString()}");
                    command.CommandText = stmt.ToString();
                    command.ExecuteNonQuery();

                    Log.Debug($"AlterStatements :: Added foreign key {prop.Name.ToLower()} to {entity.Type.Name.ToLower()}");
                }
            }

        }
    }
}