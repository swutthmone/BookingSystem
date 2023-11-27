using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public interface IPackageRepository : IRepositoryBase<TblPackage>
    {
        dynamic GetPackageByCountry(int CountryID);
    }
}