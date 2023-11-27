
using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public class BookingTransactionRepository : RepositoryBase<TblBookingTransaction>, IBookingTransactionRepository
    {
        public BookingTransactionRepository(AppDb repositoryContext) : base(repositoryContext) { }
        
    }

}
