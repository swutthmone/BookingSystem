using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using BookingSystem.Entities;
using BookingSystem.Repositories;
using BookingSystem.Operational;
using Microsoft.Extensions.Configuration;
using CustomTokenAuthProvider;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using BookingSystem.Entities.DTO;

namespace BookingSystem.Controllers
{
    public class PublicRequestController : BaseController
    {
        private IRepositoryWrapper _repositoryWrapper;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public static IHttpContextAccessor _httpContextAccessor;

        private AppDb _objdb;
        private string _EncryptionSalt = "";
        public PublicRequestController(IRepositoryWrapper RW, AppDb DB, IHttpContextAccessor httpContextAccessor)
        {
            _repositoryWrapper = RW;

            _objdb = DB;

            _httpContextAccessor = httpContextAccessor;

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();

            double expiretimespan = Convert.ToDouble(Configuration.GetSection("TokenAuthentication:TokenExpire").Value);
            _EncryptionSalt = Configuration.GetSection("Encryption:EncryptionSalt").Value;

            TimeSpan expiration = TimeSpan.FromMinutes(expiretimespan);
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("TokenAuthentication:SecretKey").Value));

            _options = new TokenProviderOptions
            {
                Path = Startup.StaticConfiguration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = Startup.StaticConfiguration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = Startup.StaticConfiguration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Expiration = expiration

            };
        }
        [HttpPost("Register", Name = "Register")]
        public dynamic Register([FromBody] RegisterRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            try
            {
                int status = 0;
                string message = "";
                string otpPrefix = "";

                string objName = obj.name;
                string objEmail = obj.email;
                string objFirstName = obj.firstname;
                string objLastName = obj.lastname;
                bool objGender = obj.gender;
                DateTime? objDOB = obj.dateofbirth;

                var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                var Configuration = appsettingbuilder.Build();
                int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);

                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    // register by email
                    TblTmpUser registerInfo = _repositoryWrapper.TblTmpUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (registerInfo != null)
                    {
                        status = 3;
                        message = "Email address already exist.";
                    }
                    else
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string[] AllowedCharacters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                        string RandomCharString = Globalfunction.GenerateRandomOTP(6, saAllowedCharacters);
                        string sRandomChar = Globalfunction.GenerateRandomChar(1, AllowedCharacters);
                        string sRandomOTP = sRandomChar + "-" + RandomCharString;
                        DateTime currentDateTime = System.DateTime.Now;

                        TblTmpUser objUser = new TblTmpUser();
                        objUser.UserName = objName;
                        objUser.FirstName = objFirstName;
                        objUser.LastName = objLastName;
                        objUser.Gender = objGender;
                        objUser.DOB = objDOB;
                        objUser.Email = objEmail;
                        objUser.CreatedDate = System.DateTime.Now;
                        objUser.ModifiedDate = System.DateTime.Now;
                        objUser.IsVerified = false;
                        objUser.OTP = sRandomOTP;
                        objUser.OTPExpireTime = currentDateTime.AddMinutes(OTPExpireMinutes);
                        objUser.OTPFailCount = 0;
                        _repositoryWrapper.TblTmpUser.Create(objUser);
                        int UserID = objUser.UserID;

                        // Send an email with OTP

                        status = 1;
                        message = "Register Successfully.";
                        otpPrefix = sRandomChar;
                    }
                }


                objresponse = new { status = status, message = message, otpPrefix = otpPrefix };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail register.", otpPrefix = "" };
                Log.Error("Error in Register " + ex.Message + " " + ex.StackTrace);
            }

            return objresponse;
        }

        [HttpPost("VerifyOTP", Name = "VerifyOTP")]
        public dynamic VerifyOTP([FromBody] VerifyOTPRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;

            string objotp = obj.otp;
            string objEmail = obj.email;

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);
            int OTPRetryCount = Convert.ToInt32(Configuration.GetSection("OTPSetting:OTPRetryCount").Value);

            try
            {
                int status = 0;
                string message = "";
                DateTime currentDateTime = System.DateTime.Now;
                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 6;
                    message = "Email is require";
                }
                else
                {
                    TblTmpUser objByEmail = _repositoryWrapper.TblTmpUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (objByEmail == null)
                    {
                        status = 7;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        DateTime otpExpireTime = Convert.ToDateTime(objByEmail.OTPExpireTime);
                        if (currentDateTime > otpExpireTime)
                        {
                            status = 3;
                            message = "Your OTP has expired. Please try to request new OTP.";
                        }
                        else
                        {

                            if (objByEmail.IsVerified == false)
                            {
                                if (objotp == objByEmail.OTP)
                                {

                                    objByEmail.IsVerified = true;
                                    objByEmail.ModifiedDate = System.DateTime.Now;
                                    _repositoryWrapper.TblTmpUser.Update(objByEmail);
                                    status = 1;
                                    message = "OTP verified Successfully.";
                                }
                                else
                                {
                                    if (objByEmail.OTPFailCount >= OTPRetryCount)
                                    {
                                        status = 4;
                                        message = "You have been reached maximum allowed OTP fail limit, please try to request new OTP.";
                                    }
                                    else
                                    {
                                        status = 2;
                                        message = "Wrong OTP. Please try again.";
                                        int TmpFailCount = objByEmail.OTPFailCount;
                                        objByEmail.OTPFailCount = TmpFailCount + 1;
                                        _repositoryWrapper.TblTmpUser.Update(objByEmail);
                                    }
                                }
                            }
                            else
                            {
                                status = 5;
                                message = "Already Verified.";
                            }
                        }


                    }
                }
                objresponse = new { status = status, message = message };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail VerifyOTP." };
                Log.Error("Error in VerifyOTP " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }
        [HttpPost("ResendOTP", Name = "ResendOTP")]
        public dynamic ResendOTP([FromBody] ResendOTPRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            int status = 0;
            string message = "";
            string otpPrefix = "";
            DateTime currentDateTime = System.DateTime.Now;

            string objEmail = obj.email;

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);


            try
            {
                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    TblTmpUser tmpuserobj = _repositoryWrapper.TblTmpUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (tmpuserobj == null)
                    {
                        status = 3;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string[] AllowedCharacters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                        string RandomCharString = Globalfunction.GenerateRandomOTP(6, saAllowedCharacters);
                        string sRandomChar = Globalfunction.GenerateRandomChar(1, AllowedCharacters);
                        string sRandomOTP = sRandomChar + "-" + RandomCharString;
                        if (tmpuserobj.IsVerified == false)
                        {

                            tmpuserobj.OTP = sRandomOTP;
                            tmpuserobj.OTPExpireTime = currentDateTime.AddMinutes(OTPExpireMinutes);
                            tmpuserobj.OTPFailCount = 0;
                            tmpuserobj.ModifiedDate = System.DateTime.Now;
                            _repositoryWrapper.TblTmpUser.Update(tmpuserobj);

                            // Send an email with OTP

                            status = 1;
                            message = "OTP resend Successfully.";
                            otpPrefix = sRandomChar;
                        }
                        else
                        {
                            status = 4;
                            message = "Already Verified.";
                            otpPrefix = "";
                        }
                    }
                }
                objresponse = new { status = status, message = message, otpPrefix = otpPrefix };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail ResendOTP.", otpPrefix = "" };
                Log.Error("Error in ResendOTP " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }

        [HttpPost("SignUp", Name = "SignUp")]
        public dynamic SignUp([FromBody] SignUpRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            string objemail = obj.email;
            string objpassword = obj.password;
            try
            {
                int status = 0;
                string message = "";
                if (string.IsNullOrEmpty(objemail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    TblTmpUser objbyemail = _repositoryWrapper.TblTmpUser.FindByCondition(x => x.Email == objemail).FirstOrDefault();
                    if (objbyemail == null)
                    {
                        status = 3;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        if (objbyemail.IsVerified == true)
                        {
                            TblUser objUser = _repositoryWrapper.TblUser.FindByCondition(x => x.Email == objbyemail.Email).FirstOrDefault();
                            if (objUser == null)
                            {
                                var newobj = new TblUser();
                                newobj.UserName = objbyemail.UserName;
                                newobj.Email = objbyemail.Email;
                                newobj.FirstName = objbyemail.FirstName;
                                newobj.LastName = objbyemail.LastName;
                                newobj.DOB = objbyemail.DOB;
                                newobj.Gender = objbyemail.Gender;
                                string salt = Operational.Encrypt.SaltedHash.GenerateSalt();
                                objpassword = Operational.Encrypt.SaltedHash.ComputeHash(salt, objpassword.ToString());
                                newobj.Password = objpassword;
                                newobj.Salt = salt;
                                newobj.CreatedDate = System.DateTime.Now;
                                newobj.ModifiedDate = System.DateTime.Now;
                                newobj.LoginFailCount = 0;
                                newobj.AccessStatus = 0;
                                _repositoryWrapper.TblUser.Create(newobj);
                                status = 1;
                                message = "Sign Up Successfully.";

                            }
                            else
                            {
                                status = 5;
                                message = "Email address already exist.";
                            }

                        }
                        else
                        {
                            status = 4;
                            message = "Please make verify Email firsly.";
                        }
                    }
                }
                objresponse = new { status = status, message = message };

            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail SignUp." };
                Log.Error("Error in SignUp " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }
        [HttpPost("Unlock", Name = "Unlock")]
        public dynamic Unlock([FromBody] UnlockRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            string objemail = obj.email;
            try
            {
                int status = 0;
                string message = "";
                if (string.IsNullOrEmpty(objemail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    TblUser objbyemail = _repositoryWrapper.TblUser.FindByCondition(x => x.Email == objemail).FirstOrDefault();
                    if (objbyemail == null)
                    {
                        status = 3;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        if (objbyemail.AccessStatus == 2)
                        {
                            objbyemail.AccessStatus = 0;
                            objbyemail.LoginFailCount = 0;
                            _repositoryWrapper.TblUser.Update(objbyemail);
                            status = 1;
                            message = "User Account Unlock Successfully";
                        }
                        else
                        {
                            status = 5;
                            message = "User Account is not locked.";
                        }
                    }
                }
                objresponse = new { status = status, message = message };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail Unlock." };
                Log.Error("Error in Unlock " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }
        [HttpPost("ForgotPassword", Name = "ForgotPassword")]
        public dynamic ForgotPassword([FromBody] ForgotPasswordRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            try
            {
                int status = 0;
                string message = "";
                string otpPrefix = "";

                string objEmail = obj.email;
                var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                var Configuration = appsettingbuilder.Build();
                int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);

                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    TblUser userinfo = _repositoryWrapper.TblUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (userinfo == null)
                    {
                        status = 3;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string[] AllowedCharacters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                        string RandomCharString = Globalfunction.GenerateRandomOTP(6, saAllowedCharacters);
                        string sRandomChar = Globalfunction.GenerateRandomChar(1, AllowedCharacters);
                        string sRandomOTP = sRandomChar + "-" + RandomCharString;
                        DateTime currentDateTime = System.DateTime.Now;

                        userinfo.CreatedDate = System.DateTime.Now;
                        userinfo.ModifiedDate = System.DateTime.Now;
                        userinfo.OTP = sRandomOTP;
                        userinfo.OTPExpireTime = currentDateTime.AddMinutes(OTPExpireMinutes);
                        userinfo.OTPFailCount = 0;
                        _repositoryWrapper.TblUser.Update(userinfo);

                        // Send an email with OTP
                        status = 1;
                        message = "ForgotPassword Successfully.";
                        otpPrefix = sRandomChar;
                    }
                }
                objresponse = new { status = status, message = message, otpPrefix = otpPrefix };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail ForgotPassword.", otpPrefix = "" };
                Log.Error("Error in ForgotPassword " + ex.Message + " " + ex.StackTrace);
            }

            return objresponse;

        }
        [HttpPost("VerifyOTPForForgotPassword", Name = "VerifyOTPForForgotPassword")]
        public dynamic VerifyOTPForForgotPassword([FromBody] VerifyOTPForForgotPasswordRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;

            string objotp = obj.otp;
            string objEmail = obj.email;
            string objnewpassword = obj.newpassword;

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);
            int OTPRetryCount = Convert.ToInt32(Configuration.GetSection("OTPSetting:OTPRetryCount").Value);

            try
            {
                int status = 0;
                string message = "";
                DateTime currentDateTime = System.DateTime.Now;
                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 5;
                    message = "Email is require";
                    return objresponse = new { status = status, message = message };
                }
                else if (string.IsNullOrEmpty(objnewpassword))
                {
                    status = 5;
                    message = "New Password is require";
                    return objresponse = new { status = status, message = message };
                }
                else if (string.IsNullOrEmpty(objotp))
                {
                    status = 5;
                    message = "OTP is require";
                    return objresponse = new { status = status, message = message };
                }
                else
                {
                    TblUser objByEmail = _repositoryWrapper.TblUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (objByEmail == null)
                    {
                        status = 6;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        DateTime otpExpireTime = Convert.ToDateTime(objByEmail.OTPExpireTime);
                        if (currentDateTime > otpExpireTime)
                        {
                            status = 3;
                            message = "Your OTP has expired. Please try to request new OTP.";
                        }
                        else
                        {
                            if (objotp == objByEmail.OTP)
                            {
                                string salt = Operational.Encrypt.SaltedHash.GenerateSalt();
                                string PWD = Operational.Encrypt.SaltedHash.ComputeHash(salt, objnewpassword);
                                objByEmail.Salt = salt;
                                objByEmail.Password = PWD;
                                objByEmail.ModifiedDate = System.DateTime.Now;
                                _repositoryWrapper.TblUser.Update(objByEmail);
                                status = 1;
                                message = "OTP verified Successfully.";
                            }
                            else
                            {
                                if (objByEmail.OTPFailCount >= OTPRetryCount)
                                {
                                    status = 4;
                                    message = "You have been reached maximum allowed OTP fail limit, please try to request new OTP.";
                                }
                                else
                                {
                                    status = 2;
                                    message = "Wrong OTP. Please try again.";
                                    int TmpFailCount = objByEmail.OTPFailCount;
                                    objByEmail.OTPFailCount = TmpFailCount + 1;
                                    _repositoryWrapper.TblUser.Update(objByEmail);
                                }
                            }

                        }


                    }
                }
                objresponse = new { status = status, message = message };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail VerifyOTP." };
                Log.Error("Error in VerifyOTP " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }
    
    [HttpPost("ResendOTPForForgotPassword", Name = "ResendOTPForForgotPassword")]
        public dynamic ResendOTPForForgotPassword([FromBody] ResendOTPRequest param)
        {
            dynamic objresponse = null;
            dynamic obj = param;
            int status = 0;
            string message = "";
            string otpPrefix = "";
            DateTime currentDateTime = System.DateTime.Now;

            string objEmail = obj.email;

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int OTPExpireMinutes = Convert.ToInt32(Configuration.GetSection("TokenAuthentication:OTPTokenExpire").Value);


            try
            {
                if (string.IsNullOrEmpty(objEmail))
                {
                    status = 2;
                    message = "Email is require";
                }
                else
                {
                    TblUser tmpuserobj = _repositoryWrapper.TblUser.FindByCondition(x => x.Email == objEmail).FirstOrDefault();
                    if (tmpuserobj == null)
                    {
                        status = 3;
                        message = "Email address does not exist.";
                    }
                    else
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string[] AllowedCharacters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                        string RandomCharString = Globalfunction.GenerateRandomOTP(6, saAllowedCharacters);
                        string sRandomChar = Globalfunction.GenerateRandomChar(1, AllowedCharacters);
                        string sRandomOTP = sRandomChar + "-" + RandomCharString;

                            tmpuserobj.OTP = sRandomOTP;
                            tmpuserobj.OTPExpireTime = currentDateTime.AddMinutes(OTPExpireMinutes);
                            tmpuserobj.OTPFailCount = 0;
                            tmpuserobj.ModifiedDate = System.DateTime.Now;
                            _repositoryWrapper.TblUser.Update(tmpuserobj);

                            // Send an email with OTP

                            status = 1;
                            message = "OTP Resend For Forgot Password Successfully.";
                            otpPrefix = sRandomChar;
                    }
                }
                objresponse = new { status = status, message = message, otpPrefix = otpPrefix };
            }
            catch (Exception ex)
            {
                objresponse = new { status = 0, message = "Fail ResendOTPForForgotPassword.", otpPrefix = "" };
                Log.Error("Error in ResendOTPForForgotPassword " + ex.Message + " " + ex.StackTrace);
            }
            return objresponse;
        }
    }
}


