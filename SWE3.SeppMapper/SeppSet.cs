using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace SWE3.SeppMapper
{
    /// <summary>The entity as a List with orm features.</summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SeppSet<TEntity>: List<TEntity> where TEntity: class
    {
        /// <summary>Base List contructor.</summary>
        public SeppSet() : base() {}

        /// <summary>Base List contructor with provided collection.</summary>
        public SeppSet(IEnumerable<TEntity> collection) : base(collection) {}

        /// <summary>Gets all rows of this entity from the db and sets this SeppSet.</summary>
        /// <returns>This SeppSet</returns>
        public SeppSet<TEntity> Get() 
        {
            var data = SeppContextController.GetEntities<TEntity>();

            base.Clear();
            base.AddRange(data);

            return this;
        }

        /// <summary>Gets certain rows of this entity from the db which match the expression.</summary>
        /// <returns>SeppSet of this entity.</returns>
        public SeppSet<TEntity> Get(Expression<Func<TEntity, bool>> expression)
        {
            if (!(expression.Body is BinaryExpression binExpression)) throw new Exception($"Expression {expression.Body.ToString()} is not a BinaryExpression");

            var data = SeppContextController.GetEntities<TEntity>(binExpression);

            var func = expression.Compile();
            Predicate<TEntity> pred = func.Invoke;
            
            // update the current SeppSet
            base.RemoveAll(pred);
            base.AddRange(data);

            // return a new SeppSet with only queried rows   
            return new SeppSet<TEntity>(data);
        }

        /// <summary>Adds a new entity and persits its data in the db.</summary>
        /// <returns>The newly persited entity.</returns>
        public TEntity Create(TEntity entity)
        {
            var newEntity = SeppContextController.SaveEntity<TEntity>(entity);

            base.Add(newEntity);

            return newEntity;
        }

        /// <summary>Removes a given entity based on its primary key.</summary>
        public void Delete(TEntity entity)
        {
            SeppContextController.RemoveEntity<TEntity>(entity);
            base.Remove(entity);
        }

        /// <summary>Updates a given entity based on its primary key.</summary>
        /// <returns>The updated entity</returns>
        public TEntity Update(TEntity entity)
        {
            var updatedEntity = SeppContextController.UpdateEntity<TEntity>(entity);

            base.Remove(entity);
            base.Add(updatedEntity);

            return updatedEntity;
        }
    }
}