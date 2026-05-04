using EComm_Project.Data;
using EComm_Project_DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EComm_Project_DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbset = _context.Set<T>();
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            // to filter the data using Where 
            IQueryable<T> query = dbset;
            if(filter != null)
                query = query.Where(filter);
            //fetch data from Multiple tables
            if(includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(new[] {','} ,StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }
        public T Get(int id)
        {
            return dbset.Find(id);
        }

        public IEnumerable<T> GetAll(Expression<Func<T,bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includeProperties = null)
        {
            //to filter the data with WHERE 
            IQueryable<T> query =dbset;
            if(filter != null)
              query = query.Where(filter);
            //fetch data from Multiple tables
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties
                    .Split(new[] { ',' }, 
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
                //To Sort the data 
                if (orderBy != null)
                    return orderBy(query).ToList();
            }
            return query.ToList();
        }
        
        public void Remove(int id)
        {
            dbset.Remove(Get(id));
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        } 

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            // forget everything previous and track current operation
            _context.ChangeTracker.Clear();
            dbset.Update(entity);
        }
    }
}
 