using System.Collections.Generic;

namespace SWE3.SeppMapper
{
    /// <summary>List of an entity.</summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class SeppSet<TEntity>: List<TEntity> where TEntity: class
    {
    }
}