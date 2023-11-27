using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingSystem.Operational
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get remote ip address, optionally allowing for x-forwarded-for header check
        /// </summary>
        /// <param name="context">Http context</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (context.Connection.RemoteIpAddress != null)
            {
                string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
            
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                return ip;
                }
            }
            return context.Connection.RemoteIpAddress;
        }
        public static IPAddress GetRemoteIPAddress(ActionExecutingContext context)
        {
           if (context.HttpContext.Connection.RemoteIpAddress != null)
            {
                string header = (context.HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault());
            
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                return ip;
                }
            }
            return context.HttpContext.Connection.RemoteIpAddress;
        } 
    }
}
