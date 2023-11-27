using System;
using System.Data;
using System.Linq;
using BookingSystem.Entities;
using Kendo.Mvc.Extensions;
using BookingSystem.Operational;

namespace BookingSystem.Repositories
{
    public class PackageRepository : RepositoryBase<TblPackage>, IPackageRepository
    {
        public PackageRepository(AppDb repositoryContext) : base(repositoryContext) { }
         public dynamic GetPackageByCountry(int CountryID)
        {
            dynamic querylist = null;
            try
            {
                querylist = (
                           from usr in RepositoryContext.TblPackage
                           where usr.CountryID == CountryID && usr.ExpiredDate.Date > System.DateTime.Now.Date
                           select usr).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return querylist;
        }
    }
}
