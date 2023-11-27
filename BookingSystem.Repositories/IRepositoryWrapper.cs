using System;
namespace BookingSystem.Repositories
{
    public interface IRepositoryWrapper
    {
       ITmpUserRepository TblTmpUser { get; }
       IUserRepository TblUser { get; }
       ICountryRepository TblCountry { get; }
       IPackageRepository TblPackage { get; }
       IClassRepository TblClass { get; }
       IUserTransactionRepository TblUserTransaction { get; }
       IBookingTransactionRepository TblBookingTransaction { get; }
    }
}
