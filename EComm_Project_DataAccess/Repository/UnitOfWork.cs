using EComm_Project.Data;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
           _context = context;
            Category = new CategoryRepository(context);
            CoverType = new CoverTypeRepository(context);
            Product = new ProductRepository(context);
            Company = new CompanyRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            OrderDetail = new OrderDetailRepository(context);
            UserAddress = new UserAddressRepository(context);
        }
        public ICategoryRepository Category { private set; get;  }

        public ICoverTypeRepository CoverType {  private set; get; }

        public IProductRepository Product {  private set; get; }

        public ICompanyRepository Company { private set; get; }

        public IApplicationUserRepository ApplicationUser {  private set; get; }

        public IShoppingCartRepository ShoppingCart { private set; get; }

        public IOrderHeaderRepository OrderHeader { private set; get; }

        public IOrderDetailRepository OrderDetail { private set; get; }

        public IUserAddressRepository UserAddress { private set; get; }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
