using System;

namespace BookingSystem.Entities
{
    /* 
     * This class don't use for Table. It will use only for AuthToken Data as Object
     */
    public class TokenData
    {
        public string Sub = "";  //Required Field, Used for core JWT, The subject of the token 
        public string Jti = ""; //Required Field, Used for core JWT, Unique identifier for the JWT. Can be used to prevent the JWT from being replayed. This is helpful for a one time use token.
        public string Iat = ""; //Required Field, Used for core JWT, The time the JWT was issued. Can be used to determine the age of the JWT
        public string UserID = "";
        public string IPAddress = "";
        public string LoginUserID = "";
        public string Url = "";

        public string isMobile = "";

        public string customDateFormat="";
        public DateTime TicketExpireDate = DateTime.UtcNow;

        public string Email = "";
    }
}