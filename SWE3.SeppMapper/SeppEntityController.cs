using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;
using System.Linq;
using SWE3.SeppMapper.Exceptions;

namespace SWE3.SeppMapper
{
    /// <summary>Manages entities from the SeppContext.</summary>
    public static class SeppEntityController
    {
        /// <summary>Entities defined in the SeppContext.</summary>
        private static IEnumerable<Entity> Entities { get; set; }

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

            Log.Debug($"SeppEntityController :: Gathering information on all provided entities from {context.GetType().Name}...");
            var types = GetSeppSetTypes(context);
            Entities = GetEntities(types).ToList();
            Log.Debug($"SeppEntityController :: done, gathered {Entities.Count()} entities from {context.GetType().Name}");

            Log.Debug($"SeppEntityController :: Checking foreign key constraints of all entities...");
            CheckForeignKeyConstraints(Entities);
            Log.Debug($"SeppEntityController :: done, everything seems fine");

            Log.Debug($"SeppEntityController :: Updating database according to gathered entities...");
            var dataController = new SeppDataController(Entities, connection);
            Log.Debug($"SeppEntityController :: done updating, database is up to date");
        }

        /// <summary>Check if property of entity can be skipped for table creation and updating.</summary>
        /// <param name="property"></param>
        /// <returns>If property is skippable.</returns>
        public static bool IsPropertySkippable(Property property)
        {
            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return true;
            return Entities.Select(e => e.Type).Contains(property.Type);
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
                    Log.Error($"SeppEntityController :: Property {prop.Name} needs to be type int in model {type.Name} to be serial in database");
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
                            Log.Error($"SeppEntityController :: Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key");
                            throw new ForeignKeyAttributeException($"Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key");
                        }
                    }
                    else
                    {
                        Log.Error($"SeppEntityController :: Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context");
                        throw new ForeignKeyAttributeException($"Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context");
                    }
                }
            }
        }
    }
}