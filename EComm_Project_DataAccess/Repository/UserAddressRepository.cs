using EComm_Project.Data;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository
{
    public class UserAddressRepository : Repository<UserAddress>, IUserAddressRepository
    {
        private readonly ApplicationDbContext _context;
        public UserAddressRepository(ApplicationDbContext context) : base(context)
        {
            context = _context;
        }
    }
}
