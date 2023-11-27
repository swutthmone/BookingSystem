using BookingSystem;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Http;
namespace BookingSystem.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private AppDb _Context;
        public static IHttpContextAccessor _httpContextAccessor;
        public RepositoryWrapper(AppDb context)
        {
            _Context = context;
        }

        private ITmpUserRepository _TblTmpUser;
        public ITmpUserRepository TblTmpUser
        {
            get
            {
                if (_TblTmpUser == null)
                {
                    _TblTmpUser = new TmpUserRepository(_Context);
                }

                return _TblTmpUser;
            }
        }
        private IUserRepository _TblUser;
        public IUserRepository TblUser
        {
            get
            {
                if (_TblUser == null)
                {
                    _TblUser = new UserRepository(_Context);
                }

                return _TblUser;
            }
        }
        private ICountryRepository _TblCountry;
        public ICountryRepository TblCountry
        {
            get
            {
                if (_TblCountry == null)
                {
                    _TblCountry = new CountryRepository(_Context);
                }

                return _TblCountry;
            }
        }
        private IPackageRepository _TblPackage;
        public IPackageRepository TblPackage
        {
            get
            {
                if (_TblPackage == null)
                {
                    _TblPackage = new PackageRepository(_Context);
                }

                return _TblPackage;
            }
        }
        private IClassRepository _TblClass;
        public IClassRepository TblClass
        {
            get
            {
                if (_TblClass == null)
                {
                    _TblClass = new ClassRepository(_Context);
                }

                return _TblClass;
            }
        }
        private IUserTransactionRepository _TblUserTransaction;
        public IUserTransactionRepository TblUserTransaction
        {
            get
            {
                if (_TblUserTransaction == null)
                {
                    _TblUserTransaction = new UserTransactionRepository(_Context);
                }

                return _TblUserTransaction;
            }
        }
        private IBookingTransactionRepository _TblBookingTransaction;
        public IBookingTransactionRepository TblBookingTransaction
        {
            get
            {
                if (_TblBookingTransaction == null)
                {
                    _TblBookingTransaction = new BookingTransactionRepository(_Context);
                }

                return _TblBookingTransaction;
            }
        }

    }
}