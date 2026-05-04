using EComm_Project.Data;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
