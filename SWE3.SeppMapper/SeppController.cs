using System;
using System.Collections.Generic;
using System.Reflection;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;

namespace SWE3.SeppMapper
{
    public static class SeppController
    {
        public static IList<SeppEntity> SeppEntities { get; set; }

        public static void Inititalize(SeppContext context)
        {
            var seppTypes = GetSeppSetTypes(context);
            SeppEntities = GetSeppEntities(seppTypes);    
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

        private static IList<SeppEntity> GetSeppEntities (IList<Type> seppTypes)
        {
            var seppEntities = new List<SeppEntity>();

            foreach (var seppType in seppTypes)
            {
                seppEntities.Add(new SeppEntity{
                    Type = seppType,
                    SeppProperties = new List<SeppProperty>(GetSeppProperties(seppType))
                });
            }

            return seppEntities;
        } 

        private static IList<SeppProperty> GetSeppProperties(Type type)
        {
            var seppProperties = new List<SeppProperty>();

            foreach(var prop in type.GetProperties())
            {
                if (prop.GetSetMethod() == null) continue;

                var isPrimary = false;
                var isRequired = false;
                // GetCustomAttribute(typeof(PrimaryKeyAttribute))
                foreach(var attr in prop.GetCustomAttributes())
                {
                    isPrimary = attr is PrimaryKeyAttribute;
                    isRequired = attr is RequiredAttribute || attr is PrimaryKeyAttribute;
                }

                if (seppProperties.Find(p => p.IsPrimaryKey) != null && isPrimary) throw new Exception("There is already a primary key on the entity " + type);

                seppProperties.Add(new SeppProperty{
                    Name = prop.Name,
                    Type = prop.PropertyType,
                    IsPrimaryKey = isPrimary,
                    IsRequired = isRequired
                });
            }
        
            return seppProperties;
        }
    }
}