using System;

namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_booking_transaction")]
    public class TblBookingTransaction : BaseModel
    {
        public int BookingTransactionID { get; set; }
        public int UserID { get; set; }
        public int ClassID { get; set; }
        public int Type { get; set; } //0= Booking, 1 = Waiting
        public int Credit { get; set; }
        public int Status { get; set; }//0=Pending,1=Cancel,2= Booked
        public DateTime BookingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
