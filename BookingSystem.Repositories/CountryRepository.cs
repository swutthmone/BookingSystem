using System;
using System.Data;
using System.Linq;
using BookingSystem.Entities;
using Kendo.Mvc.Extensions;
using BookingSystem.Operational;

namespace BookingSystem.Repositories
{
    public class CountryRepository : RepositoryBase<TblCountry>, ICountryRepository
    {
        public CountryRepository(AppDb repositoryContext) : base(repositoryContext) { }
        public dynamic GetCountryList()
        {
            dynamic querylist = null;
            try
            {
                querylist = (
                           from usr in RepositoryContext.TblCountry
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
