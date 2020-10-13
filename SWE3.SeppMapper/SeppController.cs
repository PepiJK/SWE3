using System;
using System.Collections.Generic;
using System.Reflection;
using SWE3.SeppMapper.Attributes;
using SWE3.SeppMapper.Models;
using System.ComponentModel.DataAnnotations;

namespace SWE3.SeppMapper
{
    public class SeppController
    {
        private List<SeppEntity> SeppEntities { get; set; }

        public SeppController(SeppContext context)
        {
            var seppTypes = GetSeppSetTypes(context);
            
            foreach (var seppType in seppTypes)
            {
                GetSeppProperties(seppType);
            }
        }

        private IEnumerable<Type> GetSeppSetTypes(SeppContext context)
        {
            var types = new List<Type>();

            foreach (var prop in context.GetType().GetProperties())
            {
                if (prop.PropertyType.IsGenericType)
                {
                    var genericType = prop.PropertyType.GenericTypeArguments[0];
                    if (prop.PropertyType == typeof(SeppSet<>).MakeGenericType(genericType))
                    {
                        types.Add(genericType);
                    } 
                }
                    
            }

            return types;
        }

        private IEnumerable<SeppProperty> GetSeppProperties(Type type)
        {
            Console.WriteLine(type);

            var seppProperties = new List<SeppProperty>();

            foreach(var prop in type.GetProperties())
            {
                Console.WriteLine(prop.Name);

                var isPrimary = false;
                var isRequired = false;
                foreach(var attr in prop.GetCustomAttributes())
                {
                    isPrimary = attr is PrimaryKey;
                    isRequired = attr is Required;
                }


                seppProperties.Add(new SeppProperty{
                    Name = prop.Name,
                    SeppEntityType = type
                });
            }
        
            return seppProperties;
        } 
    }
}