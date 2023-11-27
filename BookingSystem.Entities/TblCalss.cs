using System;

namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_class")]
    public class TblCalss : BaseModel
    {
        public int ClassID { get; set; }
        public int  PackageID { get; set; }
        public string ClassName { get; set; }
        public int Credit { get; set; }
        public int MaxBookingCount { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}