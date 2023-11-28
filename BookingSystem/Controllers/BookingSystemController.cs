using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using BookingSystem.Entities;
using BookingSystem.Repositories;
using System.Data;
using BookingSystem.Operational;
using BookingSystem.Entities.DTO;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace BookingSystem
{
    public class BookingSystemController : BaseController
    {
        private AppDb _objdb;
        private IRepositoryWrapper _repositoryWrapper;

        private readonly IDistributedCache _cache;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1); // Limit concurrency to 1

        public BookingSystemController(IRepositoryWrapper RW, AppDb DB, IDistributedCache cache)
        {
            _repositoryWrapper = RW;
            _objdb = DB;
            _cache = cache;

        }

        [HttpPost("ChangePassword", Name = "ChangePassword")]

        public dynamic ChangePassword([FromBody] ChangePasswordRequest param)
        {
            dynamic objResponse = null;
            try
            {
                int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
                dynamic obj = param;
                string CurrentPassword = obj.currentpassword;
                string NewPassword = obj.newpassword;
                string message = "";
                int status = 0;

                if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
                {
                    status = 2;
                    message = "CurrentPassword and NewPassword are require.";
                    return objResponse = new { status = status, message = message };
                }

                if (CurrentPassword == NewPassword)
                {
                    status = 3;
                    message = "CurrentPassword and NewPassword are same.";
                    return objResponse = new { status = status, message = message };
                }

                var validateUserQuery = _repositoryWrapper.TblUser.FindByCondition(x => x.UserID == LogInUserID).FirstOrDefault();

                bool isvalidatepass = Operational.Encrypt.SaltedHash.Verify(validateUserQuery.Salt, validateUserQuery.Password, CurrentPassword);

                if (isvalidatepass)
                {

                    string salt = Operational.Encrypt.SaltedHash.GenerateSalt();
                    string PWD = Operational.Encrypt.SaltedHash.ComputeHash(salt, NewPassword);
                    validateUserQuery.Salt = salt;
                    validateUserQuery.Password = PWD;
                    _repositoryWrapper.TblUser.Update(validateUserQuery);
                    status = 1;
                    message = "Change Password Successfully.";
                    return objResponse = new { status = status, message = message };
                }
                else
                {
                    status = 4;
                    message = "CurrentPassword is not correct.";
                    return objResponse = new { status = status, message = message };
                }
            }
            catch (Exception ex)
            {
                objResponse = new { status = 0, message = "Fail ChangePassword." };
                Log.Error("Error in ChangePassword " + ex.Message + " " + ex.StackTrace);
                return objResponse;
            }
        }

        [HttpGet("RetrieveProfile", Name = "RetrieveProfile")]
        public GetProfileListResponse RetrieveProfile()
        {
            string message = "RetrieveProfile Unsuccessfully";
            int status = 0;
            int LoginUserID = Convert.ToInt32(_tokenData.LoginUserID);
            dynamic datalist = null;
            try
            {
                TblUser objUser = _repositoryWrapper.TblUser.FindByCondition(x => x.UserID == LoginUserID).FirstOrDefault();
                if (objUser != null)
                {
                    datalist = _repositoryWrapper.TblUser.GetProfile(LoginUserID);
                    status = 1;
                    message = "Retrieve Profile Successfully.";
                }
                else
                {
                    status = 2;
                    message = "User Not Found.";

                }
                GetProfileListResponse obj = new GetProfileListResponse();
                obj.data = datalist;
                obj.status = status;
                obj.message = message;

                return obj;
            }
            catch (Exception ex)
            {
                GetProfileListResponse obj = new GetProfileListResponse();
                obj.data = datalist;
                obj.status = 0;
                obj.message = "Fail RetrieveProfile";
                Log.Error("Error in RetrieveProfile " + ex.Message + " " + ex.StackTrace);
                return obj;
            }
        }

        [HttpGet("RetrieveCountryList", Name = "RetrieveCountryList")]
        public GetCountryListResponse RetrieveCountryList()
        {
            string message = "RetrieveCountryList Unsuccessfully";
            int status = 0;
            int LoginUserID = Convert.ToInt32(_tokenData.LoginUserID);
            dynamic datalist = null;
            try
            {
                datalist = _repositoryWrapper.TblCountry.GetCountryList();
                status = 1;
                message = "Retrieve CountryList Successfully.";

                GetCountryListResponse obj = new GetCountryListResponse();
                obj.data = datalist;
                obj.status = status;
                obj.message = message;

                return obj;
            }
            catch (Exception ex)
            {
                GetCountryListResponse obj = new GetCountryListResponse();
                obj.data = datalist;
                obj.status = 0;
                obj.message = "Fail RetrieveCountryList";
                Log.Error("Error in RetrieveCountryList " + ex.Message + " " + ex.StackTrace);
                return obj;
            }
        }
        [HttpPost("RetrievePackageListByCountry", Name = "RetrievePackageListByCountry")]

        public GetPackageListResponse RetrievePackageListByCountry([FromBody] GetPackageListRequest param)
        {
            dynamic objrequest = param;
            int CountryID = objrequest.CountryID;
            dynamic datalist = null;
            try
            {
                datalist = _repositoryWrapper.TblPackage.GetPackageByCountry(CountryID);
                GetPackageListResponse obj = new GetPackageListResponse();
                obj.data = datalist;
                obj.status = 1;
                obj.message = "Retrieve PackageList by Country Successfully.";
                return obj;
            }
            catch (Exception ex)
            {
                GetPackageListResponse obj = new GetPackageListResponse();
                obj.data = datalist;
                obj.status = 0;
                obj.message = "Fail RetrieveCountryList";
                Log.Error("Error in RetrieveCountryList " + ex.Message + " " + ex.StackTrace);
                return obj;
            }
        }

        [HttpPost("RetrieveClassListByPackage", Name = "RetrieveClassListByPackage")]

        public GetClassListResponse RetrieveClassListByPackage([FromBody] GetClassListRequest param)
        {
            dynamic objrequest = param;
            int PackageID = objrequest.PackageID;
            dynamic datalist = null;
            try
            {
                datalist = _repositoryWrapper.TblClass.GetClassByPackage(PackageID);
                GetClassListResponse obj = new GetClassListResponse();
                obj.data = datalist;
                obj.status = 1;
                obj.message = "Retrieve ClassList by Package Successfully.";
                return obj;
            }
            catch (Exception ex)
            {
                GetClassListResponse obj = new GetClassListResponse();
                obj.data = datalist;
                obj.status = 0;
                obj.message = "Fail RetrieveClassListByPackage";
                Log.Error("Error in RetrieveClassListByPackage " + ex.Message + " " + ex.StackTrace);
                return obj;
            }
        }

        [HttpPost("BuyPackage", Name = "BuyPackage")]

        public dynamic BuyPackage([FromBody] BuyPackageRequest param)
        {
            int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
            dynamic objresponse = null;
            dynamic objrequest = param;
            int CountryID = objrequest.CountryID;
            int PackageID = objrequest.PackageID;
            string CardNumber = objrequest.CardNumber;
            string ExpirationDate = objrequest.ExpirationDate;
            string CardholderName = objrequest.CardholderName;
            string CVV = objrequest.CardNumber;

            int status = 0;
            string message = "";
            try
            {
                if (PackageID <= 0)
                {
                    status = 2;
                    message = "Please choose package to buy.";
                    return objresponse = new { status = status, message = message };
                }
                else
                {
                    if (string.IsNullOrEmpty(CardNumber) || string.IsNullOrEmpty(ExpirationDate) || string.IsNullOrEmpty(CardholderName) || string.IsNullOrEmpty(CVV))
                    {
                        status = 2;
                        message = "Incomplete or invalid payment information provided.";
                        return objresponse = new { status = status, message = message };
                    }
                    else
                    {
                        TblPackage objPackage = _repositoryWrapper.TblPackage.FindByCondition(x => x.PackageID == PackageID && x.CountryID == CountryID).FirstOrDefault();
                        if (objPackage == null)
                        {
                            status = 3;
                            message = "Ther is no package.";
                            return objresponse = new { status = status, message = message };
                        }
                        else
                        {
                            if (System.DateTime.Now.Date >= objPackage.ExpiredDate.Date)
                            {
                                status = 4;
                                message = "Package Expired.";
                                return objresponse = new { status = status, message = message };
                            }
                            else
                            {
                                TblUserTransaction objuserTransaction = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID).FirstOrDefault();
                                if (objuserTransaction != null)
                                {
                                    status = 4;
                                    message = "This Package is already bought.";
                                    return objresponse = new { status = status, message = message };
                                }
                                else
                                {
                                    TblUserTransaction newobj = new TblUserTransaction();
                                    newobj.UserID = LogInUserID;
                                    newobj.PackageID = PackageID;
                                    newobj.Type = 0;
                                    newobj.Credit = objPackage.Credit;
                                    newobj.Debit = 0;
                                    newobj.CreatedDate = System.DateTime.Now;
                                    newobj.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblUserTransaction.Create(newobj);

                                    PaymentCharge objbuy = new PaymentCharge();
                                    objbuy.Amount = objPackage.Price;
                                    objbuy.CardNumber = CardNumber;
                                    objbuy.CardholderName = CardholderName;
                                    objbuy.CVV = CVV;
                                    objbuy.ExpirationDate = ExpirationDate;
                                    makePayment(objbuy);
                                    status = 1;
                                    message = "Buy Package Successfully.";
                                    return objresponse = new { status = status, message = message };
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail BuyPackage." };
                Log.Error("Error in BuyPackage " + ex.Message + " " + ex.StackTrace);
                return objresponse;
            }
        }

        [HttpPost("RetrieveUserPurchasedPackageList", Name = "RetrieveUserPurchasedPackageList")]

        public GetUserPurchasedPackageListResponse RetrieveUserPurchasedPackageList([FromBody] GetUserPurchasedPackageListRequest param)
        {
            dynamic datalist = null;
            dynamic pobj = param;
            try
            {
                int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
                int currentPage = 0;
                int rowsPerPage = 0;
                if (pobj.pageNumber != null)
                {
                    currentPage = (int)pobj.pageNumber;
                }

                if (pobj.pageSize != null)
                {
                    rowsPerPage = (int)pobj.pageSize;
                }
                var usertransaction = (from m in _objdb.TblUserTransaction where m.UserID == LogInUserID select m);
                var list = (
                           from usrtran in _objdb.TblUserTransaction
                           join usr in _objdb.TblPackage on usrtran.PackageID equals usr.PackageID
                           join country in _objdb.TblCountry on usr.CountryID equals country.CountryID
                           where usrtran.UserID == LogInUserID
                           select new
                           {
                               PackageName = usr.PackageName,
                               Price = usr.Price,
                               Credit = usr.Credit,
                               PackageStatus = System.DateTime.Now.Date >= usr.ExpiredDate.Date ? "Expired" : "Active",
                               CountryName = country.CountryName,
                               CountryCode = country.CountryCode,
                               BalanceCredit = usertransaction.Where(x => x.UserID == LogInUserID && x.PackageID == usrtran.PackageID && x.Type == 0).Select(x => x.Credit ).SingleOrDefault() - usertransaction.Where(x => x.UserID == LogInUserID && x.PackageID == usrtran.PackageID && x.Type == 1).Select(x => x.Debit).SingleOrDefault()
                           }).Distinct().ToList();
                datalist = list.AsQueryable();
                int totalCount = list.Count;
                if (currentPage > 0 && totalCount > 0)
                {
                    IEnumerable<dynamic> alarmTmp = PaginatedList<dynamic>.Create(datalist, currentPage, rowsPerPage);
                    var resultlist = alarmTmp.Select(x => x).ToList();
                    GetUserPurchasedPackageListResponse obj = new GetUserPurchasedPackageListResponse();
                    obj.data = resultlist;
                    obj.total = totalCount;
                    obj.status = 1;
                    obj.message = "GetUserPurchasedPackageList Successfully.";
                    return obj;
                }
                else
                {
                    GetUserPurchasedPackageListResponse obj = new GetUserPurchasedPackageListResponse();
                    obj.data = datalist;
                    obj.total = totalCount;
                    obj.status = 1;
                    obj.message = "GetUserPurchasedPackageList Successfully.";
                    return obj;
                }

            }
            catch (Exception ex)
            {
                GetUserPurchasedPackageListResponse obj = new GetUserPurchasedPackageListResponse();
                obj.data = datalist;
                obj.total = 0;
                obj.status = 0;
                obj.message = "Fail GetUserPurchasedPackageList";
                Log.Error("Error in GetUserPurchasedPackageList " + ex.Message + " " + ex.StackTrace);
                return obj;
            }

        }

        [HttpPost("ClassBooking", Name = "ClassBooking")]

        public dynamic ClassBooking([FromBody] ClassBookingRequest param)
        {
            // Use a distributed lock to ensure atomicity across multiple instances
            Semaphore.Wait();
            
            dynamic objresponse = null;
            dynamic robj = param;
            int PackageID = robj.PackageID;
            int ClassID = robj.ClassID;
            int CountryID = robj.CountryID;
            int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
            int status = 0;
            string message = "";
            try
            {
                var objPackage = _repositoryWrapper.TblPackage.FindByCondition(x => x.CountryID == CountryID && x.PackageID == PackageID).FirstOrDefault();
                if (objPackage != null)
                {
                    DateTime PackageExpireDate = objPackage.ExpiredDate;
                    if (System.DateTime.Now.Date >= PackageExpireDate)
                    {
                        status = 5;
                        message = "This Package have been expired.";
                        return objresponse = new { status = status, message = message };
                    }
                }
                else
                {
                    status = 6;
                    message = "There is no package.";
                    return objresponse = new { status = status, message = message };
                }
                var objClass = _repositoryWrapper.TblClass.FindByCondition(x => x.ClassID == ClassID && x.PackageID == PackageID).FirstOrDefault();
                if (objClass == null)
                {
                    status = 2;
                    message = "There is no class.";
                    return objresponse = new { status = status, message = message };
                }
                else
                {
                    int MaxBookingCount = objClass.MaxBookingCount;
                    string bookedUsersCountStr = _cache.GetString("BookedUsersCount");
                    int bookedUsersCount = string.IsNullOrEmpty(bookedUsersCountStr) ? 0 : int.Parse(bookedUsersCountStr);
                    if (bookedUsersCount < MaxBookingCount) //Add booking list
                    {
                        // Simulate some processing time
                        Thread.Sleep(1000);

                        // Increment the booked users count
                        bookedUsersCount++;

                        // Update the cache with the new count
                        _cache.SetString("BookedUsersCount", bookedUsersCount.ToString());

                        var objbookingTransaction = _repositoryWrapper.TblBookingTransaction.FindByCondition(x => x.UserID == LogInUserID && x.ClassID == ClassID).FirstOrDefault();
                        if (objbookingTransaction != null)
                        {
                            status = 3;
                            message = "This class is already booked.";
                            return objresponse = new { status = status, message = message };
                        }
                        else
                        {
                            int BookedUserCount = _repositoryWrapper.TblBookingTransaction.FindByCondition(x => x.ClassID == ClassID && x.Type == 0 && x.Status == 2).Count();
                            if (BookedUserCount < MaxBookingCount)
                            {
                                var objUserTransactionForBooking = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                                var objUserTransactionForBuyPackage = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 0).FirstOrDefault();

                                int Credit = 0;
                                int Debit = 0;
                                int PackageCreditBalance = 0;
                                int CreditofBookedClass = objClass.Credit;
                                if (objUserTransactionForBuyPackage != null) Credit = objUserTransactionForBuyPackage.Credit;
                                if (objUserTransactionForBooking != null) Debit = objUserTransactionForBooking.Debit;
                                PackageCreditBalance = Credit - Debit;

                                if (PackageCreditBalance >= CreditofBookedClass)
                                {

                                    TblBookingTransaction newobj = new TblBookingTransaction();
                                    newobj.UserID = LogInUserID;
                                    newobj.ClassID = ClassID;
                                    newobj.Type = 0;
                                    newobj.Credit = objClass.Credit;
                                    newobj.Status = 2; //Booked
                                    newobj.BookingDate = System.DateTime.Now;
                                    newobj.CreatedDate = System.DateTime.Now;
                                    newobj.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblBookingTransaction.Create(newobj);

                                    if (objUserTransactionForBooking != null)
                                    {
                                        int tmpDebit = objUserTransactionForBooking.Debit;
                                        objUserTransactionForBooking.Debit = tmpDebit + objClass.Credit;
                                        objUserTransactionForBooking.ModifiedDate = System.DateTime.Now;
                                        _repositoryWrapper.TblUserTransaction.Update(objUserTransactionForBooking);
                                    }
                                    else
                                    {
                                        TblUserTransaction newUserTransaction = new TblUserTransaction();
                                        newUserTransaction.UserID = LogInUserID;
                                        newUserTransaction.PackageID = PackageID;
                                        newUserTransaction.Type = 1;//For BookingOrWaiting
                                        newUserTransaction.Credit = 0;
                                        newUserTransaction.Debit = objClass.Credit;
                                        newUserTransaction.CreatedDate = System.DateTime.Now;
                                        newUserTransaction.ModifiedDate = System.DateTime.Now;
                                        _repositoryWrapper.TblUserTransaction.Create(newUserTransaction);
                                    }
                                    status = 1;
                                    message = "Class Booking Successfully.";
                                    return objresponse = new { status = status, message = message };

                                }
                                else
                                {
                                    status = 4;
                                    message = "Not Enough Package Credit Balance.";
                                    return objresponse = new { status = status, message = message };
                                }
                            }
                            else//Add Waiting List
                            {
                                var objUserTransactionForBooking = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                                var objUserTransactionForBuyPackage = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 0).FirstOrDefault();

                                int Credit = 0;
                                int Debit = 0;
                                int PackageCreditBalance = 0;
                                int CreditofBookedClass = objClass.Credit;
                                if (objUserTransactionForBuyPackage != null) Credit = objUserTransactionForBuyPackage.Credit;
                                if (objUserTransactionForBooking != null) Debit = objUserTransactionForBooking.Debit;
                                PackageCreditBalance = Credit - Debit;

                                if (PackageCreditBalance >= CreditofBookedClass)
                                {

                                    TblBookingTransaction newobj = new TblBookingTransaction();
                                    newobj.UserID = LogInUserID;
                                    newobj.ClassID = ClassID;
                                    newobj.Type = 1;
                                    newobj.Credit = objClass.Credit;
                                    newobj.Status = 0; //Pending
                                    newobj.BookingDate = System.DateTime.Now;
                                    newobj.CreatedDate = System.DateTime.Now;
                                    newobj.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblBookingTransaction.Create(newobj);

                                    if (objUserTransactionForBooking != null)
                                    {
                                        int tmpDebit = objUserTransactionForBooking.Debit;
                                        objUserTransactionForBooking.Debit = tmpDebit + objClass.Credit;
                                        objUserTransactionForBooking.ModifiedDate = System.DateTime.Now;
                                        _repositoryWrapper.TblUserTransaction.Update(objUserTransactionForBooking);
                                    }
                                    else
                                    {
                                        TblUserTransaction newUserTransaction = new TblUserTransaction();
                                        newUserTransaction.UserID = LogInUserID;
                                        newUserTransaction.PackageID = PackageID;
                                        newUserTransaction.Type = 1;//For BookingOrWating
                                        newUserTransaction.Credit = 0;
                                        newUserTransaction.Debit = objClass.Credit;
                                        newUserTransaction.CreatedDate = System.DateTime.Now;
                                        newUserTransaction.ModifiedDate = System.DateTime.Now;
                                        _repositoryWrapper.TblUserTransaction.Create(newUserTransaction);
                                    }
                                    status = 1;
                                    message = "Class Booking Successfully.";
                                    return objresponse = new { status = status, message = message };
                                }
                                else
                                {
                                    status = 4;
                                    message = "Not Enough Package Credit Balance.";
                                    return objresponse = new { status = status, message = message };
                                }
                            }


                        }
                    }
                    else //Add Waiting List
                    {
                        var objbookingTransaction = _repositoryWrapper.TblBookingTransaction.FindByCondition(x => x.UserID == LogInUserID && x.ClassID == ClassID).FirstOrDefault();
                        if (objbookingTransaction != null)
                        {
                            status = 3;
                            message = "This class is already booked.";
                            return objresponse = new { status = status, message = message };
                        }
                        else
                        {
                            var objUserTransactionForBooking = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                            var objUserTransactionForBuyPackage = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 0).FirstOrDefault();

                            int Credit = 0;
                            int Debit = 0;
                            int PackageCreditBalance = 0;
                            int CreditofBookedClass = objClass.Credit;
                            if (objUserTransactionForBuyPackage != null) Credit = objUserTransactionForBuyPackage.Credit;
                            if (objUserTransactionForBooking != null) Debit = objUserTransactionForBooking.Debit;
                            PackageCreditBalance = Credit - Debit;

                            if (PackageCreditBalance >= CreditofBookedClass)
                            {

                                TblBookingTransaction newobj = new TblBookingTransaction();
                                newobj.UserID = LogInUserID;
                                newobj.ClassID = ClassID;
                                newobj.Type = 1;
                                newobj.Credit = objClass.Credit;
                                newobj.Status = 0; //Pending
                                newobj.BookingDate = System.DateTime.Now;
                                newobj.CreatedDate = System.DateTime.Now;
                                newobj.ModifiedDate = System.DateTime.Now;
                                _repositoryWrapper.TblBookingTransaction.Create(newobj);

                                if (objUserTransactionForBooking != null)
                                {
                                    int tmpDebit = objUserTransactionForBooking.Debit;
                                    objUserTransactionForBooking.Debit = tmpDebit + objClass.Credit;
                                    objUserTransactionForBooking.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblUserTransaction.Update(objUserTransactionForBooking);
                                }
                                else
                                {
                                    TblUserTransaction newUserTransaction = new TblUserTransaction();
                                    newUserTransaction.UserID = LogInUserID;
                                    newUserTransaction.PackageID = PackageID;
                                    newUserTransaction.Type = 1;//For BookingOrWating
                                    newUserTransaction.Credit = 0;
                                    newUserTransaction.Debit = objClass.Credit;
                                    newUserTransaction.CreatedDate = System.DateTime.Now;
                                    newUserTransaction.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblUserTransaction.Create(newUserTransaction);
                                }
                                status = 1;
                                message = "Class Booking Successfully.";
                                return objresponse = new { status = status, message = message };
                            }
                            else
                            {
                                status = 4;
                                message = "Not Enough Package Credit Balance.";
                                return objresponse = new { status = status, message = message };
                            }


                        }
                    }
                }

            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                objresponse = new { status = 0, message = "Fail ClassBooking." };
                Log.Error("Error in ClassBooking " + ex.Message + " " + ex.StackTrace);
                return objresponse;
            }
            finally
            {
                // Release the lock
                Semaphore.Release();
            }
        }
        [HttpPost("RetrieveBookingList", Name = "RetrieveBookingList")]

        public RetrieveBookingListResponse RetrieveBookingList([FromBody] RetrieveBookingListRequest param)
        {
            dynamic datalist = null;
            dynamic pobj = param;
            try
            {
                int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
                int currentPage = 0;
                int rowsPerPage = 0;
                if (pobj.pageNumber != null)
                {
                    currentPage = (int)pobj.pageNumber;
                }

                if (pobj.pageSize != null)
                {
                    rowsPerPage = (int)pobj.pageSize;
                }
                var list = (
                           from trans in _objdb.TblBookingTransactions
                           join cl in _objdb.TblCalss on trans.ClassID equals cl.ClassID
                           where trans.UserID == LogInUserID
                           select new
                           {
                               ClassName = cl.ClassName,
                               Type = trans.Type == 0 ? "Booking" : "Waiting",
                               Credit = trans.Credit,
                               Status = trans.Status == 0 ? "Pending" : (trans.Status == 1 ? "Cancel" : "Booked"),
                               StartTime = cl.StartTime,
                               StartDate = cl.StartDate.ToString("yyyy-MM-dd"),
                               EndDate = cl.EndDate.ToString("yyyy-MM-dd"),
                               }).Distinct().ToList();
                datalist = list.AsQueryable();
                int totalCount = list.Count;
                if (currentPage > 0 && totalCount > 0)
                {
                    IEnumerable<dynamic> alarmTmp = PaginatedList<dynamic>.Create(datalist, currentPage, rowsPerPage);
                    var resultlist = alarmTmp.Select(x => x).ToList();
                    RetrieveBookingListResponse obj = new RetrieveBookingListResponse();
                    obj.data = resultlist;
                    obj.total = totalCount;
                    obj.status = 1;
                    obj.message = "RetrieveBookingList Successfully.";
                    return obj;
                }
                else
                {
                    RetrieveBookingListResponse obj = new RetrieveBookingListResponse();
                    obj.data = datalist;
                    obj.total = totalCount;
                    obj.status = 1;
                    obj.message = "RetrieveBookingList Successfully.";
                    return obj;
                }

            }
            catch (Exception ex)
            {
                RetrieveBookingListResponse obj = new RetrieveBookingListResponse();
                obj.data = datalist;
                obj.total = 0;
                obj.status = 0;
                obj.message = "Fail RetrieveBookingList";
                Log.Error("Error in RetrieveBookingList " + ex.Message + " " + ex.StackTrace);
                return obj;
            }

        }

        [HttpPost("CancelClassBooking", Name = "CancelClassBooking")]

        public dynamic CancelClassBooking([FromBody] CancelClassBookingRequest param)
        {
            dynamic objresponse = null;
            dynamic robj = param;
            int ClassID = robj.ClassID;
            int PackageID = robj.PackageID;
            int CountryID = robj.CountryID;
            int LogInUserID = Int32.Parse(_tokenData.LoginUserID);
            int status = 0;
            string message = "";

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int BookingCancelHr = Convert.ToInt32(Configuration.GetSection("appsettings:BookingCancelHr").Value);
            
            try
            {
                var objBookingTransaction = _repositoryWrapper.TblBookingTransaction.FindByCondition(x => x.UserID == LogInUserID && x.ClassID == ClassID).FirstOrDefault();
                if (objBookingTransaction == null)
                {
                    status = 2;
                    message = "There is no class.";
                    return objresponse = new { status = status, message = message };
                }
                else
                {
                    if (objBookingTransaction.Status == 1)
                    {
                        status = 3;
                        message = "This class already canceled.";
                        return objresponse = new { status = status, message = message };
                    }
                    else
                    {
                        TblCalss objClass = _repositoryWrapper.TblClass.FindByCondition(x => x.ClassID == ClassID).FirstOrDefault();
                        if (objClass == null)
                        {
                            status = 4;
                            message = "There is no class.";
                            return objresponse = new { status = status, message = message };
                        }
                        else
                        {

                            TimeSpan StartTime = objClass.StartTime;
                            DateTime ClassStartDateTime = objClass.StartDate.Add(StartTime);
                            DateTime CompareStartDateTime = ClassStartDateTime.AddHours(-BookingCancelHr);
                            if (System.DateTime.Now <= CompareStartDateTime) //Refurn Credit
                            {
                                var objUserTransactionForBooking = _repositoryWrapper.TblUserTransaction.FindByCondition(x => x.UserID == LogInUserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                                if (objUserTransactionForBooking != null)
                                {
                                    int tmpDebit = objUserTransactionForBooking.Debit;
                                    objUserTransactionForBooking.Debit = tmpDebit - objClass.Credit;
                                    objUserTransactionForBooking.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblBookingTransaction.Update(objUserTransactionForBooking);

                                }
                                objBookingTransaction.Status = 1;//change to cancel status
                                objBookingTransaction.ModifiedDate = System.DateTime.Now;
                                _repositoryWrapper.TblBookingTransaction.Update(objBookingTransaction);

                                status = 1;
                                message = "CancelClassBooking Successfully.";
                                return objresponse = new { status = status, message = message };
                            }
                            else
                            {
                                objBookingTransaction.Status = 1;//change to cancel status
                                objBookingTransaction.ModifiedDate = System.DateTime.Now;
                                _repositoryWrapper.TblBookingTransaction.Update(objBookingTransaction);

                                status = 1;
                                message = "CancelClassBooking Successfully.";
                                return objresponse = new { status = status, message = message };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                objresponse = new { status = 0, message = "Fail CancelClassBooking." };
                Log.Error("Error in CancelClassBooking " + ex.Message + " " + ex.StackTrace);
                return objresponse;
            }
        }

        public class PaymentCharge
        {
            public decimal Amount { get; set; }
            public string CardNumber { get; set; }
            public string ExpirationDate { get; set; }
            public string CardholderName { get; set; }
            public string CVV { get; set; }
        }
        bool makePayment(PaymentCharge obj)
        {
            return true;
        }


    }
}
