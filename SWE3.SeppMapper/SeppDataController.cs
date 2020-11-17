using System.Collections.Generic;
using SWE3.SeppMapper.Models;
using System.Linq;
using Serilog;
using System;
using SqlKata.Execution;
using SWE3.SeppMapper.Statements;
using SWE3.SeppMapper.Helpers;
using SWE3.SeppMapper.Exceptions;

namespace SWE3.SeppMapper
{
    /// <summary>Manages database.</summary>
    public class SeppDataController
    {
        /// <summary>Create table statements.</summary>
        private readonly CreateStatements _createStatements;
        
        /// <summary>Drop table statements.</summary>
        private readonly DropStatements _dropStatements;
        
        /// <summary>Alter table statements.</summary>
        private readonly AlterStatements _alterStatements;
        
        /// <summary>Select statements.</summary>
        private readonly SelectStatements _selectStatements;

        /// <summary>Create statement classes with db connection and update the database.</summary>
        /// <param name="entities"></param>
        /// <param name="connection"></param>
        public SeppDataController(IEnumerable<Entity> entities, string connection)
        {
            _createStatements = new CreateStatements(connection);
            _dropStatements = new DropStatements(connection);
            _alterStatements = new AlterStatements(connection);
            _selectStatements = new SelectStatements(connection);

            UpdateDatabase(entities);
        }

        /// <summary>Update database based on the provided entities.</summary>
        /// <param name="entities"></param>
        private void UpdateDatabase (IEnumerable<Entity> entities)
        {
            var remainingTables = _selectStatements.GetAllTablesFromDb().ToList();
            var tablesToUpdate = new Dictionary<Entity, Table>();

            foreach (var entity in entities)
            {
                if (remainingTables.Count(t => t.Name == entity.Type.Name.ToLower()) == 1)
                {
                    Log.Debug($"SeppDataController :: Entity {entity.Type.Name} does exist as table, marking table for updating");
                    var table = remainingTables.First(t => t.Name == entity.Type.Name.ToLower());
                    tablesToUpdate.Add(entity, table);
                    remainingTables.Remove(table);
                }
                else
                {
                    Log.Debug($"SeppDataController :: Entity {entity.Type.Name} does not exist as table, creating table...");
                    _createStatements.CreateTable(entity);
                }
            }

            foreach (var table in remainingTables)
            {
                Log.Debug($"SeppDataController :: Dropping redundant table {table.Name}...");
                _dropStatements.DropTable(table);
            }

            foreach(var entityTable in tablesToUpdate)
            {
                Log.Debug($"SeppDataController :: Table {entityTable.Value.Name} exists, removing foreign key constraints...");
                _alterStatements.DropForeignKeyConstraintsOfTable(entityTable.Value);
            }

            foreach(var entityTable in tablesToUpdate)
            {
                Log.Debug($"SeppDataController :: Table {entityTable.Value.Name} exists, removing primary key constraint...");
                _alterStatements.DropPrimaryKeyConstraintOfTable(entityTable.Value);
            }

            foreach(var entityTable in tablesToUpdate)
            {
                Log.Debug($"SeppDataController :: Updating table {entityTable.Value.Name} columns...");
                UpdateTableColumns(entityTable.Key, entityTable.Value);
            }

            foreach (var entity in entities)
            {
                Log.Debug($"SeppDataController :: Adding primary key constraint from entity {entity.Type.Name} to corresponding table");
                _alterStatements.AddPrimaryKeyConstraint(entity);
            }

            foreach (var entity in entities)
            {
                Log.Debug($"SeppDataController :: Adding foreign key constraints from entity {entity.Type.Name} to corresponding table");
                _alterStatements.AddForeignKeyConstraints(entity);
            }
        }

        /// <summary>Update all columns except the primary keys of the provided table.</summary>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <exception cref="NewColumnException"></exception>
        private void UpdateTableColumns(Entity entity, Table table)
        {
            // only properties and columns which are not primary keys
            var entityProps = entity.Properties.ToList();
            var tableColumns = table.Columns.ToList();
            var tableHasUpdated = false;

            foreach (var col in tableColumns)
            {
                if (entityProps.Count(p => 
                    p.Name.ToLower() == col.Name && 
                    PostgresTypeHelper.GetPostgresType(p.Type).ToLower() == col.Type &&
                    p.IsSerial == col.IsSerial &&
                    p.IsRequired == col.IsRequired) == 1)
                {
                    // Table column is represented in the entity
                    var prop = entityProps.First(p => p.Name.ToLower() == col.Name);
                    entityProps.Remove(prop);
                }
                else
                {
                    // Table column is not represented in the entity
                    _alterStatements.DropColumnFromTable(table, col);
                    Log.Debug($"SeppDataController :: Dropped column {col.Name} from table {table.Name}");
                    tableHasUpdated = true;
                }
            }

            // Add properties to table which have not been removed from entityProps list before
            foreach (var prop in entityProps)
            {
                if (SeppEntityController.IsPropertySkippable(prop)) continue;
                if (table.HasRows && prop.IsRequired)
                {
                    Log.Error($"SeppDataController :: New column {prop.Name} cannot be added because it is not nullable and table {table.Name} holds data");
                    throw new NewColumnException($"New column {prop.Name} cannot be added because it is not nullable and table {table.Name} holds data");
                }

                _alterStatements.AddPropertyToTable(table, prop);
                Log.Debug($"SeppDataController :: Added new property {prop.Name} to existing table {table.Name}");
                tableHasUpdated = true;
            }

            if (!tableHasUpdated) Log.Debug($"SeppDataController :: done, nothing to update");
            else Log.Debug($"SeppDataController :: done");
        }
        
    }
}