using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public interface IClassRepository : IRepositoryBase<TblCalss>
    {
        dynamic GetClassByPackage(int PackageID);
    }
}