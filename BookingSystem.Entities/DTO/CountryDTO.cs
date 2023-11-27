using System;

namespace BookingSystem.Entities.DTO
{
    public class GetCountryListResponse
    {
        public dynamic data { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }

}
