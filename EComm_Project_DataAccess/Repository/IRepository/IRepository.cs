using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EComm_Project_DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);  //Save
        void Update(T entity);  //Edit
        T Get(int id);   //Find the single record

        //To Display the table data
        IEnumerable<T> GetAll(Expression<Func<T,bool>> filter = null,  //To add Filter condition 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,    //To sort the data
            string includeProperties = null);  //Fetch the data from multiple table(Category or CoverType)
        T FirstOrDefault(Expression<Func<T,bool>> filter = null,string includeProperties =null); //to fetch the data from non primary-key and multiple table
     
        void Remove (int id);  //passes primary key as a parameter
        void Remove(T entity);  //passes the single record
        void RemoveRange(IEnumerable<T> entities); //Deletes multiple records
    }
}
