using System;
using System.Collections.Generic;
using Serilog;
using SWE3.SeppMapper.Exceptions;

namespace SWE3.SeppMapper.Helpers
{
    /// <summary>A simple helper class to convert C# types to Postgres types.</summary>
    public static class PostgresTypeHelper
    {
        /// <summary>Dictionary to map C# types to Postgres types.</summary>
        private static readonly IDictionary<Type, string> Type2PgTypeDict = new Dictionary<Type, string>{
            {typeof(int), "INTEGER"},
            {typeof(int?), "INTEGER"},
            {typeof(string), "TEXT"},
            {typeof(DateTime), "TIMESTAMP WITHOUT TIME ZONE"},
            {typeof(float), "REAL"},
            {typeof(bool), "BIT(1)"}
        };

        /// <summary>Convert a C# type to a Postgres type.</summary>
        /// <param name="dotNetType"></param>
        /// <returns>Valid Postgres type in caps.</returns>
        /// <exception cref="Exception"></exception>
        public static string GetPostgresType(Type dotNetType)
        {
            if (Type2PgTypeDict.TryGetValue(dotNetType, out var pgType))
            {
                return pgType;
            }
            
            Log.Error($"PostgresTypeHelper :: Type {dotNetType.Name} is not supported."); 
            throw new PostgresTypeConversionException($"Type {dotNetType.Name} is not supported.");
        }
    }
}