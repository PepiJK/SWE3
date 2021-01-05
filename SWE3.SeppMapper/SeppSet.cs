using System;
using System.Linq;
using System.Collections.Generic;

namespace SWE3.SeppMapper
{
    /// <summary>The entity as a List with orm features.</summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SeppSet<TEntity>: List<TEntity> where TEntity: class
    {
        /// <summary>Cache of this entity.</summary>
        private SeppSet<TEntity> Cache { get; set; }

        /// <summary>Base List contructor.</summary>
        public SeppSet() : base() {}

        /// <summary>Base List contructor with provided collection.</summary>
        public SeppSet(IEnumerable<TEntity> collection) : base(collection) {}


        /// <summary>Get data of this entity from cache or from db.</summary>
        /// <returns>SeppSet of this entity.</returns>
        public SeppSet<TEntity> Get() 
        {
            return Cache ?? SetCacheAndElements();
        }

        /// <summary>Get data by a specific predicate.</summary>
        /// <returns>SeppSet of this entity.</returns>
        public SeppSet<TEntity> Get(Func<TEntity, bool> predicate) 
        {
            throw new NotImplementedException();
        }


        /// <summary>Adds a new entity and persits its data in the db.</summary>
        /// <returns>The newly persited entity.</returns>
        public new TEntity Add(TEntity entity)
        {
            var newEntity = SeppContextController.SaveEntity<TEntity>(entity);

            base.Add(newEntity);

            return newEntity;
        }

        /// <summary>Removes a given entity based on its primary key.</summary>
        public new void Remove(TEntity entity)
        {
            throw new NotImplementedException();
            //base.Remove(entity);
        }

        /// <summary>Updates a given entity based on its primary key.</summary>
        /// <returns>The updated entity</returns>
        public TEntity Update(TEntity entity)
        {
            throw new NotImplementedException();
            //base.Remove(entity);
            //base.Add(entity);
        }

        /// <summary>Queries db and sets cache and SeppSet elements.</summary>
        /// <returns>This SeppSet with updated cache.</returns>
        private SeppSet<TEntity> SetCacheAndElements()
        {
            var data = SeppContextController.GetAllRowsFromDb<TEntity>();

            Cache = new SeppSet<TEntity>(data);
            base.AddRange(Cache);

            return this;
        }

    }
}