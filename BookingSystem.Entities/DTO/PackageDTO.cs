using System;

namespace BookingSystem.Entities.DTO
{
    public class GetPackageListRequest
    {
        public int CountryID { get; set; }
    }
    public class GetPackageListResponse
    {
        public dynamic data { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public class BuyPackageRequest
    {
        public int CountryID { get; set; }
        public int PackageID { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardholderName { get; set; }
        public string CVV { get; set; }
    }
    public class GetUserPurchasedPackageListResponse
    {
        public dynamic data { get; set; }
        public int total { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public class GetUserPurchasedPackageListRequest
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }

}
