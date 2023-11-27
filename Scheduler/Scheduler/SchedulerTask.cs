using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingSystem.Entities;
using BookingSystem.Repositories;
using BookingSystem.Operational;
using System.Linq;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.Extensions;

namespace ASPNETCoreScheduler.Scheduler
{
    public class SchedulerTask : ScheduledProcessor
    {
        private IRepositoryWrapper _repositoryFromBS;
        private AppDb _objdb;

        public SchedulerTask(IRepositoryWrapper repositoryFromBS, IServiceScopeFactory serviceScopeFactory, ILoggerFactory DepLoggerFactory, AppDb DB) : base(serviceScopeFactory, DepLoggerFactory)
        {

            _objdb = DB;
            _repositoryFromBS = repositoryFromBS;
        }

        //protected override string Schedule => "*/10 * * * *";
        protected override string Schedule => "CronJobs:QueueTransaction"; //it will need to set at appsettings.json

        public override Task ProcessInScope(IServiceProvider serviceProvider)
        {
            try
            {
                Console.WriteLine(" Scheduler Service Start! " + DateTime.Now);
                DateTime currentDate = DateTime.Now;
                var waitList = (from m in _objdb.TblBookingTransactions
                                where m.Status == 0
                                select m).AsNoTracking().ToList();

                foreach (var obj in waitList)
                {
                    int UserID = obj.UserID;
                    int ClassID = obj.ClassID;
                    var objClass = _repositoryFromBS.TblClass.FindByCondition(x => x.ClassID == ClassID).FirstOrDefault();
                    int MaxBookingCount = 0;
                    int PackageID = 0;
                    DateTime ClassEndDate = System.DateTime.Now;
                    if (objClass != null)
                    {
                        MaxBookingCount = objClass.MaxBookingCount;
                        PackageID = objClass.PackageID;
                        ClassEndDate = objClass.EndDate;
                    }
                    if (System.DateTime.Now.Date >= ClassEndDate.Date) //When class ends, waitlist’s users' credits need to be refunded
                    {
                        var objUsrTranForBooking = _repositoryFromBS.TblUserTransaction.FindByCondition(x => x.UserID == UserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                        if (objUsrTranForBooking != null)
                        {
                            int tmpDebit = objUsrTranForBooking.Debit;
                            objUsrTranForBooking.Debit = tmpDebit - objClass.Credit;
                            objUsrTranForBooking.ModifiedDate = System.DateTime.Now;
                            _repositoryFromBS.TblUserTransaction.Update(objUsrTranForBooking);

                        }
                        obj.Status = 1;//change to cancel status
                        obj.ModifiedDate = System.DateTime.Now;
                        _repositoryFromBS.TblBookingTransaction.Update(obj);
                    }
                    else
                    {
                        int BookedUserCount = _repositoryFromBS.TblBookingTransaction.FindByCondition(x => x.ClassID == ClassID && x.Type == 0 && x.Status == 2).Count();
                        if (BookedUserCount < MaxBookingCount)
                        {
                            var objUserTransactionForBooking = _repositoryFromBS.TblUserTransaction.FindByCondition(x => x.UserID == UserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                            var objUserTransactionForBuyPackage = _repositoryFromBS.TblUserTransaction.FindByCondition(x => x.UserID == UserID && x.PackageID == PackageID && x.Type == 0).FirstOrDefault();

                            int Credit = 0;
                            int Debit = 0;
                            int PackageCreditBalance = 0;
                            int CreditofBookedClass = objClass.Credit;
                            if (objUserTransactionForBuyPackage != null) Credit = objUserTransactionForBuyPackage.Credit;
                            if (objUserTransactionForBooking != null) Debit = objUserTransactionForBooking.Debit;
                            PackageCreditBalance = Credit - Debit;

                            if (PackageCreditBalance >= CreditofBookedClass)
                            {
                                if (System.DateTime.Now.Date >= ClassEndDate.Date) //When class ends, waitlist’s users' credits need to be refunded
                                {
                                    var objUsrTranForBooking = _repositoryFromBS.TblUserTransaction.FindByCondition(x => x.UserID == UserID && x.PackageID == PackageID && x.Type == 1).FirstOrDefault();
                                    if (objUsrTranForBooking != null)
                                    {
                                        int tmpDebit = objUsrTranForBooking.Debit;
                                        objUsrTranForBooking.Debit = tmpDebit - objClass.Credit;
                                        objUsrTranForBooking.ModifiedDate = System.DateTime.Now;
                                        _repositoryFromBS.TblUserTransaction.Update(objUsrTranForBooking);

                                    }
                                    obj.Status = 1;//change to cancel status
                                    obj.ModifiedDate = System.DateTime.Now;
                                    _repositoryFromBS.TblBookingTransaction.Update(obj);
                                }
                                else
                                {

                                    obj.Type = 0; //Change from Waiting type to Booking Type
                                    obj.Status = 2; //change status from pending to booked
                                    obj.BookingDate = System.DateTime.Now;
                                    obj.ModifiedDate = System.DateTime.Now;
                                    _repositoryFromBS.TblBookingTransaction.Update(obj);

                                    if (objUserTransactionForBooking != null) //Increase Debit 
                                    {
                                        int tmpDebit = objUserTransactionForBooking.Debit;
                                        objUserTransactionForBooking.Debit = tmpDebit + objClass.Credit;
                                        objUserTransactionForBooking.ModifiedDate = System.DateTime.Now;
                                        _repositoryFromBS.TblUserTransaction.Update(objUserTransactionForBooking);
                                    }
                                    else
                                    {
                                        TblUserTransaction newUserTransaction = new TblUserTransaction();
                                        newUserTransaction.UserID = UserID;
                                        newUserTransaction.PackageID = PackageID;
                                        newUserTransaction.Type = 1;//For BookingOrWating
                                        newUserTransaction.Credit = 0;
                                        newUserTransaction.Debit = objClass.Credit;
                                        newUserTransaction.CreatedDate = System.DateTime.Now;
                                        newUserTransaction.ModifiedDate = System.DateTime.Now;
                                        _repositoryFromBS.TblUserTransaction.Create(newUserTransaction);
                                    }
                                }
                            }

                        }
                    }
                }
                Console.WriteLine(" Scheduler Service End! " + DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + ex.Message + ex.StackTrace);
                Log.Error(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}
