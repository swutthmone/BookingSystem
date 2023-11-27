using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public class TmpUserRepository : RepositoryBase<TblTmpUser>, ITmpUserRepository
    {
        public TmpUserRepository(AppDb repositoryContext) : base(repositoryContext) { }
        
    }
}
