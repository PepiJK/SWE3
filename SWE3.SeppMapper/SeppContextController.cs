using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;
using System.Linq;
using SWE3.SeppMapper.Exceptions;
using SWE3.SeppMapper.Helpers;
using System.Linq.Expressions;

namespace SWE3.SeppMapper
{
    /// <summary>Manages entities from the SeppContext.</summary>
    public static class SeppContextController
    {
        /// <summary>Entities defined in the SeppContext.</summary>
        private static IEnumerable<Entity> Entities { get; set; }

        /// <summary>SeppDataController responsible for controlling db access.</summary>
        private static SeppDataController SeppDataController { get; set; }

        /// <summary>SeppContext which contains the entities.</summary>
        private static SeppContext SeppContext { get; set; }

        /// <summary>Gather metadata from SeppContext, check for correctness and update database.</summary>
        /// <param name="context"></param>
        /// <param name="connection"></param>
        public static void Initialize(SeppContext context, string connection)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt")
                .CreateLogger();

            SeppContext = context;

            Log.Debug($"SeppContextController :: Gathering information on all provided entities from {context.GetType().Name}...");
            var types = GetSeppSetTypes(context);
            Entities = GetEntities(types);
            Log.Debug($"SeppContextController :: done, gathered {Entities.Count()} entities from {context.GetType().Name}");

            Log.Debug($"SeppContextController :: Checking foreign key constraints of all entities...");
            CheckForeignKeyConstraints(Entities);
            Log.Debug($"SeppContextController :: done, everything seems fine");

            Log.Debug($"SeppContextController :: Updating database according to gathered entities...");
            SeppDataController = new SeppDataController(Entities, connection);
            Log.Debug($"SeppContextController :: done updating, database is up to date");
        }

        /// <summary>Check if property of entity can be skipped for table creation and updating.</summary>
        /// <param name="property"></param>
        /// <returns>If property is skippable.</returns>
        public static bool IsPropertySkippable(Property property)
        {
            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
            return Entities.Select(e => e.Type).Contains(property.Type);
        }

        /// <summary>Gets all rows of the provided TEntity type from the db.</summary>
        /// <returns>Queried entities.</returns>
        public static IEnumerable<TEntity> GetEntities<TEntity>() where TEntity: class
        {
            return SeppDataController.GetEntities<TEntity>();
        }

        /// <summary>Gets all rows of the provided TEntity type from the db where the expression fits</summary>
        /// <returns>Queried entities.</returns>
        public static IEnumerable<TEntity> GetEntities<TEntity>(BinaryExpression binaryExpression) where TEntity: class
        {
            return SeppDataController.GetEntities<TEntity>(binaryExpression);
        }

        /// <summary>Saves provided entity in the db.</summary>
        /// <returns>The newly saved entity.</returns>
        public static TEntity SaveEntity<TEntity>(TEntity entity) where TEntity: class
        {
            return SeppDataController.SaveEntity<TEntity>(entity, Entities.Single(e => e.Type == typeof(TEntity)));
        } 

        /// <summary>Updates provided entity in the db.</summary>
        /// <returns>The updated entity.</returns>
        public static TEntity UpdateEntity<TEntity>(TEntity entity) where TEntity: class
        {
            return SeppDataController.UpdateEntity<TEntity>(entity, Entities.Single(e => e.Type == typeof(TEntity)));
        } 

        /// <summary>Removes provided entity from the db and remove the related entities which should cascade.</summary>
        public static void RemoveEntity<TEntity>(TEntity entity) where TEntity: class
        {
            var dbEntity = Entities.Single(e => e.Type == typeof(TEntity));
            SeppDataController.RemoveEntity<TEntity>(entity, dbEntity);

            RemoveRelatedEntities(dbEntity, entity);
        }

        /// <summary>Looks up foreign key references and their ReferencingType to remove the underlying related entity from the SeppContext.</summary>
        private static void RemoveRelatedEntities(Entity dbEntity, object entity)
        {
            // TODO: should be called recursevly if the referenced entity has another referenced entity
            // -> then also the data (primary key value) of this referenced entity is needed
            // should also update the referenced entity if the ReferentialAction is SetNull

            var referencedEntityTypes = dbEntity.Properties.Where(p => p.ForeignKeyInfo != null).Select(p => p.ForeignKeyInfo.ReferencingType);
            var referencedEntities = Entities.Where(e => referencedEntityTypes.Contains(e.Type));

            foreach (var refEntity in referencedEntities)
            {
                if (refEntity.Properties.Where(p => p.ForeignKeyInfo?.ReferencingType == dbEntity.Type && p.ForeignKeyInfo?.ReferentialAction == ReferentialActions.Cascade).Count() > 0)
                {
                    var foreignKeys = dbEntity.Properties.Where(p => p.ForeignKeyInfo?.ReferencingType == refEntity.Type);
                    foreach(var foreignKey in foreignKeys)
                    {
                        var propertyName = foreignKey.ForeignKeyInfo.ReferencingColumn;
                        var propertyValue = entity.GetType().GetProperty(foreignKey.Name).GetValue(entity, null);
                        var entityType = refEntity.Type;

                        if (propertyValue == null) continue;

                        var seppSet = SeppContext.GetType().GetProperties().Single(p => p.PropertyType == typeof(SeppSet<>).MakeGenericType(entityType)).GetValue(SeppContext, null);
                        var seppSetRemoveAll = seppSet.GetType().GetMethods().Single(m => m.Name == "RemoveAll");

                        // Lambda p => p.propertyName == propertyValue
                        Type itemType = entityType;
                        ParameterExpression predParam = Expression.Parameter(itemType, "p");
                        Expression left = Expression.Property(predParam, itemType.GetProperty(propertyName));
                        Expression right = Expression.Constant(propertyValue);
                        Expression equality = Expression.Equal(left, right);
                        Type predicateType = typeof(Predicate<>).MakeGenericType(itemType);
                        LambdaExpression lambda = Expression.Lambda(predicateType, equality, predParam);
                        var compiled = lambda.Compile();

                        seppSetRemoveAll.Invoke(seppSet, new object[]{compiled});
                    }
                }
            }
        }

        /// <summary>Gather list of types of the all SeppSet properties in SeppContext.</summary>
        /// <param name="context"></param>
        /// <returns>Types of SeppSet properties in SeppContext.</returns>
        private static IEnumerable<Type> GetSeppSetTypes(SeppContext context)
        {
            var seppSetTypes = new List<Type>();

            foreach (var prop in context.GetType().GetProperties())
            {
                if (prop.PropertyType.IsGenericType)
                {
                    var genericType = prop.PropertyType.GenericTypeArguments[0];
                    if (prop.PropertyType == typeof(SeppSet<>).MakeGenericType(genericType))
                    {
                        seppSetTypes.Add(genericType);
                    } 
                }   
            }

            return seppSetTypes;
        }

        /// <summary>Generate entities from list of types.</summary>
        /// <param name="seppTypes"></param>
        /// <returns>Entities from provided entity types.</returns>
        private static IEnumerable<Entity> GetEntities (IEnumerable<Type> seppTypes)
        {
            var entities = new List<Entity>();

            foreach (var seppType in seppTypes)
            {
                entities.Add(new Entity{
                    Type = seppType,
                    Properties = GetSeppProperties(seppType)
                });
            }

            return entities;
        } 

        /// <summary>Generate properties from provided entity type.</summary>
        /// <param name="type"></param>
        /// <returns>Properties of provided entity type.</returns>
        /// <exception cref="SerialAttributeException"></exception>
        private static IEnumerable<Property> GetSeppProperties(Type type)
        {
            var properties = new List<Property>();

            foreach(var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() == null) continue;

                var isSerial = prop.GetCustomAttribute(typeof(SerialAttribute)) != null;

                if (isSerial && prop.PropertyType != typeof(int))
                {
                    Log.Error($"SeppContextController :: Property {prop.Name} needs to be type int in model {type.Name} to be serial in database");
                    throw new SerialAttributeException($"Property {prop.Name} needs to be type int in model {type.Name} to be serial in database");
                }

                properties.Add(new Property{
                    Name = prop.Name,
                    Type = prop.PropertyType,
                    IsPrimaryKey = prop.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null,
                    IsRequired = Nullable.GetUnderlyingType(prop.PropertyType) == null || prop.GetCustomAttribute(typeof(RequiredAttribute)) != null,
                    IsSerial = isSerial,
                    ForeignKeyInfo = prop.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute
                });
            }
        
            return properties;
        }

        /// <summary>Check all foreign key constraints for correctness.</summary>
        /// <param name="entities"></param>
        /// <exception cref="ForeignKeyAttributeException"></exception>
        private static void CheckForeignKeyConstraints(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var foreignKeyProperties = entity.Properties.Where(p => p.ForeignKeyInfo != null);
                foreach (var foreignKey in foreignKeyProperties)
                {
                    if (entities.Count(e => e.Type == foreignKey.ForeignKeyInfo.ReferencingType) == 1)
                    {
                        var matchedEntityType = entities.First(e => e.Type == foreignKey.ForeignKeyInfo.ReferencingType);
                        var primaryKeys = matchedEntityType.Properties.Where(p => p.IsPrimaryKey);
                        if (primaryKeys.Count(p => p.Name == foreignKey.ForeignKeyInfo.ReferencingColumn) != 1)
                        {
                            Log.Error($"SeppContextController :: Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key");
                            throw new ForeignKeyAttributeException($"Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key");
                        }
                    }
                    else
                    {
                        Log.Error($"SeppContextController :: Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context");
                        throw new ForeignKeyAttributeException($"Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context");
                    }
                }
            }
        }
    }
}