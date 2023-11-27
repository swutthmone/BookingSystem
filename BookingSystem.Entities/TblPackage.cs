using System;

namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_packages")]
    public class TblPackage : BaseModel
    {
        public int PackageID { get; set; }
        public int CountryID { get; set; }
        public string PackageName { get; set; }
        public int Credit { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiredDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}