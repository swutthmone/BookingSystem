using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public interface ICountryRepository : IRepositoryBase<TblCountry>
    {
        dynamic GetCountryList();
    }
}