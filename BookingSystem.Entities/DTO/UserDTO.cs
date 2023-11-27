using System;

namespace BookingSystem.Entities.DTO
{
    public class GetProfileListResponse
    {
        public dynamic data { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public class ChangePasswordRequest
    {
        public string currentpassword { get; set; }
        public string newpassword { get; set; }
    }
    public class RegisterRequest
    {
        public string name { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public bool gender { get; set; }
        public DateTime? dateofbirth { get; set; }
    }
    public class VerifyOTPRequest
    {
        public string email { get; set; }
        public string otp { get; set; }
    }
    public class ResendOTPRequest
    {
        public string email { get; set; }
    }
    public class SignUpRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
    public class UnlockRequest
    {
        public string email { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string email { get; set; }
    }
    public class VerifyOTPForForgotPasswordRequest
    {
        public string email { get; set; }
        public string newpassword { get; set; }
        public string otp { get; set; }
    }

}
