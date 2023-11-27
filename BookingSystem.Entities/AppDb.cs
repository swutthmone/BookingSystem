using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookingSystem.Entities
{
    public class AppDb : DbContext
    {
        public string _connectionString = "";
        public AppDb(DbContextOptions<AppDb> options) : base(options)
        {
            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();

            _connectionString = Environment.GetEnvironmentVariable("DBSTRING") ?? Configuration.GetConnectionString("DefaultConnection");
            Database.GetDbConnection().StateChange += Connection_StateChange; // set Timezone for each query.
        }
        void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            if (e.CurrentState == System.Data.ConnectionState.Open)
            {
                if (sender is System.Data.Common.DbConnection)
                {
                    // Set the session time zone to UTC - we will handle any date/time conversions.
                    var command = (sender as System.Data.Common.DbConnection).CreateCommand();
                    command.CommandText = "SET time_zone = '+06:30'";
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        public virtual DbSet<TblTmpUser> TblTmpUser { get; set; }
        public virtual DbSet<TblUser> TblUser { get; set; }
        public virtual DbSet<TblCountry> TblCountry { get; set; }
        public virtual DbSet<TblPackage> TblPackage { get; set; }
        public virtual DbSet<TblCalss> TblCalss { get; set; }
        public virtual DbSet<TblUserTransaction> TblUserTransaction { get; set; }
        public virtual DbSet<TblBookingTransaction> TblBookingTransactions { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblTmpUser>()
              .HasKey(c => new { c.UserID });

            modelBuilder.Entity<TblUser>()
             .HasKey(c => new { c.UserID });

             modelBuilder.Entity<TblCountry>()
             .HasKey(c => new { c.CountryID });

             modelBuilder.Entity<TblPackage>()
             .HasKey(c => new { c.PackageID });

             modelBuilder.Entity<TblCalss>()
             .HasKey(c => new { c.ClassID });

             modelBuilder.Entity<TblUserTransaction>()
             .HasKey(c => new { c.TransactionID });

             modelBuilder.Entity<TblBookingTransaction>()
             .HasKey(c => new { c.BookingTransactionID });


        }
    }
}