using EComm_Project.Data;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
