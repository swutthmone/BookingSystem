using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Linq;
using System;
using BookingSystem.Entities;

namespace BookingSystem
{

    [Route("v2_2api/{api:regex(api|mobile)}/[controller]")]
    public class BaseController : Controller
    {
        public TokenData _tokenData = new TokenData();
        public string _ipaddress = "";
        public string _clienturl = "";
        public ActionExecutingContext _actionExecutionContext;
        
        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            _actionExecutionContext = context;
            string Source = context.HttpContext.Request.Path.ToString().Split(new char[] { '/' })[1].ToString();
            string ControllerAction = context.ActionDescriptor.DisplayName.ToString().Replace(" (ESS)", "");

            Request.HttpContext.Session.Set("ApiSource", System.Text.Encoding.ASCII.GetBytes(Source));
            Request.HttpContext.Session.Set("ControllerAction", System.Text.Encoding.ASCII.GetBytes(ControllerAction));

            _ipaddress = "127.0.0.1";
            _ipaddress = Convert.ToString(BookingSystem.Operational.HttpContextExtensions.GetRemoteIPAddress(context));
            _clienturl = context.HttpContext.Request.Headers["Referer"].ToString() == "" ? context.HttpContext.Request.Headers["myOrigin"].ToString() : context.HttpContext.Request.Headers["Referer"].ToString();
            try
            {
                ClaimsIdentity objclaim = context.HttpContext.User.Identities.Last();
                if (objclaim.Claims.Count() >= 7)
                {
                    _tokenData.Sub = objclaim.FindFirst("Sub").Value;
                    _tokenData.LoginUserID = objclaim.FindFirst("LoginUserID").Value;
                    _tokenData.TicketExpireDate = DateTime.Parse(objclaim.FindFirst("TicketExpireDate").Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("setDefaultDataFromToken Base Controller" + ex.Message);
            }
        }

        protected string getTokenData(string claimname)
        {
            try
            {
                ClaimsIdentity objclaim = _actionExecutionContext.HttpContext.User.Identities.Last();
                if (objclaim != null)
                {
                    if (objclaim.FindFirst(claimname) != null)
                        return objclaim.FindFirst(claimname).Value;
                    else
                    {
                        Console.WriteLine("Get Token Data Not Found" + claimname);
                        return "";
                    }

                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Token Data Exception :" + ex.Message);
                return "";
            }

        }
        }
}