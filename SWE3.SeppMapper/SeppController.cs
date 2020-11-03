using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;
using System.Linq;

namespace SWE3.SeppMapper
{
    public static class SeppController
    {
        public static IEnumerable<Entity> Entities { get; set; }

        public static void Inititalize(SeppContext context, string connection)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt")
                .CreateLogger();

            Log.Debug($"SeppController :: Gathering information on all provided entities from {context.GetType().Name}...");
            var types = GetSeppSetTypes(context);
            Entities = GetEntities(types);
            Log.Debug($"SeppController :: done, gathered {Entities.Count()} enities from {context.GetType().Name}");

            Log.Debug($"SeppController :: Checking foreign key contraints of all entities...");
            CheckForeignKeyConstraints(Entities);
            Log.Debug($"SeppController :: done, everything seems fine");

            SeppData.Initilize(Entities, connection);
        }

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

        private static IEnumerable<Property> GetSeppProperties(Type type)
        {
            var properties = new List<Property>();

            foreach(var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() == null) continue;

                var isSerial = prop.GetCustomAttribute(typeof(SerialAttribute)) != null;

                if (isSerial && prop.PropertyType != typeof(int))
                {
                    Log.Error($"Property {prop.Name} needs to be type int in model {type.Name} to be serial in database");
                    throw new Exception($"Property {prop.Name} needs to be type int in model {type.Name} to be serial in database"); // TODO: Create custom Exception
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

        private static void CheckForeignKeyConstraints(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var foreignKeyProperties = entity.Properties.Where(p => p.ForeignKeyInfo != null);
                foreach (var foreignKey in foreignKeyProperties)
                {
                    if (entities.Where(e => e.Type == foreignKey.ForeignKeyInfo.ReferencingType).Count() == 1)
                    {
                        var matchedEntityType = entities.Where(e => e.Type == foreignKey.ForeignKeyInfo.ReferencingType).First();
                        var primaryKeys = matchedEntityType.Properties.Where(p => p.IsPrimaryKey);
                        if (primaryKeys.Where(p => p.Name == foreignKey.ForeignKeyInfo.ReferencingColumn).Count() != 1)
                        {
                            Log.Error($"Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key");
                            throw new Exception($"Referencing column {foreignKey.ForeignKeyInfo.ReferencingColumn} on foreign key {foreignKey.Name} on entity {entity.Type.Name} is not a primary key"); // TODO: Create custom Exception
                        }
                    }
                    else
                    {
                        Log.Error($"Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context");
                        throw new Exception($"Cannot find referencing type {foreignKey.ForeignKeyInfo.ReferencingType} in provided context"); // TODO: Create custom Exception
                    }
                }
            }
        }
    }
}