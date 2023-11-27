using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public interface IUserTransactionRepository : IRepositoryBase<TblUserTransaction>
    {
        dynamic GetUserPurchasedPackageList(int UserID);
    }
}