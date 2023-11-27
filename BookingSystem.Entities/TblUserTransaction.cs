using System;

namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_user_transaction")]
    public class TblUserTransaction : BaseModel
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public int PackageID { get; set; }
        public int Type { get; set; } //0= BuyPacakge, 1 = BookingOrWaiting
        public int Credit { get; set; }
        public int Debit { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}