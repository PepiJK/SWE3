using System;
using System.Collections.Generic;
using System.Reflection;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper
{
    public static class SeppController
    {
        public static IList<Entity> Entities { get; set; }

        public static void Inititalize(SeppContext context)
        {
            var types = GetSeppSetTypes(context);
            Entities = GetEntities(types);    
        }

        private static IList<Type> GetSeppSetTypes(SeppContext context)
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

        private static IList<Entity> GetEntities (IList<Type> seppTypes)
        {
            var entities = new List<Entity>();

            foreach (var seppType in seppTypes)
            {
                entities.Add(new Entity{
                    Type = seppType,
                    Properties = new List<Property>(GetSeppProperties(seppType))
                });
            }

            return entities;
        } 

        private static IList<Property> GetSeppProperties(Type type)
        {
            var properties = new List<Property>();

            foreach(var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() == null) continue;

                properties.Add(new Property{
                    Name = prop.Name,
                    Type = prop.PropertyType,
                    IsPrimaryKey = prop.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null,
                    IsRequired = prop.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null || prop.GetCustomAttribute(typeof(RequiredAttribute)) != null,
                    ForeignKeyInfo = prop.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute
                });
            }
        
            return properties;
        }
    }
}