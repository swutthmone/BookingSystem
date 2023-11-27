using System;

namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_country")]
    public class TblCountry : BaseModel
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}