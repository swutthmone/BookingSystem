using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using BookingSystem.Entities;
using Kendo.Mvc.Extensions;
using BookingSystem.Operational;

namespace BookingSystem.Repositories
{
    public class UserRepository : RepositoryBase<TblUser>, IUserRepository
    {
        public UserRepository(AppDb repositoryContext) : base(repositoryContext) { }
        public List<TblUser> GetLoginInfo(string username, string password, string loginemail)
        {
            string _name = username;
            string _password = password;
            string _email = loginemail;
            dynamic querylist = null;
            try
            {
                querylist = (
                           from usr in RepositoryContext.TblUser
                           where usr.UserName == _name && usr.Email == loginemail
                           select usr).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return querylist;
        }
        public dynamic GetProfile(int UserID)
        {
            dynamic querylist = null;
            try
            {
                querylist = (
                           from usr in RepositoryContext.TblUser
                           where usr.UserID == UserID
                           select new
                           {
                               usr.UserName,
                               usr.Email,
                               usr.FirstName,
                               usr.LastName,
                               DateOfBirth = usr.DOB == null ? "" : usr.DOB.ToString(),
                               Gender = usr.Gender == false ? "Male": "Female",
                               usr.CreatedDate,
                               usr.ModifiedDate
                           }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return querylist;
        }

    }

}
