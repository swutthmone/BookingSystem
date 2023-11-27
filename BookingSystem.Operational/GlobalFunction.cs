using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using BookingSystem.Entities;

namespace BookingSystem.Operational
{
    public class Globalfunction
    {
        public static AppDb currentDBContext = null;
        
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static Claim[] GetClaims(TokenData obj)
        {
            var claims = new Claim[]
            {
                new Claim("UserID",obj.UserID),
                new Claim("LoginUserID", obj.LoginUserID),
                new Claim(JwtRegisteredClaimNames.Sub, obj.Sub),
                new Claim(JwtRegisteredClaimNames.Jti, obj.Jti),
                new Claim(JwtRegisteredClaimNames.Iat, obj.Iat, ClaimValueTypes.Integer64),
                new Claim("TicketExpireDate", obj.TicketExpireDate.ToString()),
                new Claim("Email",obj.Email)
            };
            return claims;
        }
        public static TokenData GetTokenData(JwtSecurityToken tokenS)
        {
            var obj = new TokenData();
            try
            {
                obj.UserID = tokenS.Claims.First(claim => claim.Type == "UserID").Value;
                obj.Sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;
                obj.LoginUserID = tokenS.Claims.First(claim => claim.Type == "LoginUserID").Value;
                obj.TicketExpireDate = DateTime.Parse(tokenS.Claims.First(claim => claim.Type == "TicketExpireDate").Value);
                obj.Email = tokenS.Claims.First(claim => claim.Type == "Email").Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTokenData " + DateTime.Now + ex.Message + " " + ex.StackTrace);
            }
            return obj;
        }



        public static string GenerateRandomChar(int iOTPLength, string[] AllowedCharacters)
        {
            string sOTP = string.Empty;
            Random rand = new Random();
            sOTP = AllowedCharacters[rand.Next(0, AllowedCharacters.Length)];
            return sOTP;
        }

        public static string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {

            string sOTP = String.Empty;

            string sTempChars = String.Empty;

            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)

            {
                //int p = rand.Next(0, saAllowedCharacters.Length);  

                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];

                sOTP += sTempChars;

            }
            return sOTP;

        }        
    }
}