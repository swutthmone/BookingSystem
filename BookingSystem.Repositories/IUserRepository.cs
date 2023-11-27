using System.Collections.Generic;
using BookingSystem.Entities;

namespace BookingSystem.Repositories
{
    public interface IUserRepository : IRepositoryBase<TblUser>
    {
         List<TblUser> GetLoginInfo(string username, string password, string loginemail);
         dynamic GetProfile(int UserID);
    }
}