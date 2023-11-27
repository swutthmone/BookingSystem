using System;


namespace BookingSystem.Entities
{
    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_user")]
    public class TblUser : BaseModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string OTP { get; set; }
        public DateTime? OTPExpireTime { get; set; }
        public int OTPFailCount { get; set; }

        public int LoginFailCount { get; set; }
        public int AccessStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}