using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using BookingSystem.Entities;
using Kendo.Mvc.Extensions;
using BookingSystem.Operational;

namespace BookingSystem.Repositories
{
    public class UserTransactionRepository : RepositoryBase<TblUserTransaction>, IUserTransactionRepository
    {
        public UserTransactionRepository(AppDb repositoryContext) : base(repositoryContext) { }
        public dynamic GetUserPurchasedPackageList(int UserID)
        {
            dynamic result = null;
            try
            {
               var querylist = (
                           from usrtran in RepositoryContext.TblUserTransaction
                           join usr in RepositoryContext.TblPackage on usrtran.PackageID equals usr.PackageID
                           join country in RepositoryContext.TblCountry on usr.CountryID equals country.CountryID
                           where usrtran.UserID == UserID
                           select new
                           {
                               PackageName = usr.PackageName,
                               Price = usr.Price,
                               Credit = usr.Credit,
                               PackageStatus = System.DateTime.Now.Date >= usr.ExpiredDate.Date ? "Expired" : "Active",
                               CountryName = country.CountryName,
                               CountryCode = country.CountryCode
                           }).AsQueryable();                           
                           return querylist;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return result;
        }
    }

}
