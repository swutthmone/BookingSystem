using System;

namespace BookingSystem.Entities.DTO
{
    public class GetClassListRequest
    {
        public int PackageID { get; set; }
    }
    public class GetClassListResponse
    {
        public dynamic data { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public class ClassBookingRequest
    {
        public int ClassID { get; set; }
        public int PackageID { get; set; }
        public int CountryID { get; set; }

    }
    public class CancelClassBookingRequest
    {
        public int ClassID { get; set; }
        public int PackageID { get; set; }
        public int CountryID { get; set; }

    }

}
