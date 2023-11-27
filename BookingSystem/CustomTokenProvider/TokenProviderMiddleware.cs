using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BookingSystem.Entities;
using BookingSystem.Repositories;
using BookingSystem.Operational;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;


namespace CustomTokenAuthProvider
{
    public class TokenProviderMiddleware : IMiddleware
    {       
        private IRepositoryWrapper _repository;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;
        public static IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        private double _tokenExpireMinute = 1;
        //private string _connectionString="";
        private string _EncryptionSalt = "";
        private Byte[] _signingKey;
        private Byte[] _tokenencKey;
        public TokenProviderMiddleware(IHttpContextAccessor httpContextAccessor, ILoggerFactory DepLoggerFactory, IRepositoryWrapper repository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repository;

            var requestPath = _httpContextAccessor.HttpContext.Request.Path.ToString();
            Log._logger = DepLoggerFactory.CreateLogger(requestPath);

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            string clienturl = _httpContextAccessor.HttpContext.Request.Headers["myOrigin"].ToString();
            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            double expiretimespan = Convert.ToDouble(Configuration.GetSection("TokenAuthentication:TokenExpire").Value);


            _EncryptionSalt = Configuration.GetSection("Encryption:EncryptionSalt").Value;

            TimeSpan expiration = TimeSpan.FromMinutes(expiretimespan);
            _tokenExpireMinute = expiration.TotalMinutes;
            _signingKey = Encoding.ASCII.GetBytes(Configuration.GetSection("TokenAuthentication:SecretKey").Value);

            _options = new TokenProviderOptions
            {
                Path = Configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = Configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = Configuration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_signingKey), SecurityAlgorithms.HmacSha256),
                Expiration = expiration

            };
        }

        public async Task ResponseMessage(dynamic data, HttpContext context, int code = 400)
        {
            string returnType = "0";
            try
            {
                if (!string.IsNullOrEmpty(data.returnType))
                {
                    returnType = data.returnType;//-5 for Password Expire
                }
            }
            catch (Exception)
            {
                returnType = "0";
            }
            var response = new
            {
                status = data.status,
                message = data.message,
                returnType = returnType
            };
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            //return;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.Headers.Add("server", "");
            // MKK : If public in url no need to have token and authorization
            string[] pathArr = context.Request.Path.ToString().Split('/');
            string languageStatusByInput = Convert.ToString(context.Request.Headers["LanguageStatus"]);//Get LanguageStatus From Mobile
            if (!string.IsNullOrEmpty(languageStatusByInput)) languageStatusByInput = languageStatusByInput.Replace("\"", "");

            if (
            context.Request.Path.ToString().ToLower().Contains("publicrequest/register") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/verifyotp") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/signup") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/resendotp") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/forgotpassword") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/verifyotpforforgotpassword") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/resendotpforforgotpassword") ||
            context.Request.Path.ToString().ToLower().Contains("publicrequest/unlock")
            
            
            )
            {
                await next(context);
                return;
            }

            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build(); string key = Configuration.GetSection("TokenAuthentication:Key").Value;
            if (key.Length < 32)
                _tokenencKey = Encoding.ASCII.GetBytes(key.PadRight(32, '-'));
            else
                _tokenencKey = Encoding.ASCII.GetBytes(key);
            if (!context.Request.Path.ToString().Contains(_options.Path, StringComparison.Ordinal))
            {
                #region Generate Token

                dynamic newToken = await ReGenerateToken(context);
                if (newToken == "-3")
                {
                    await ResponseMessage(new { status = "fail", message = "Access Token Invalid" }, context, 401);
                }
                else if (newToken == "-1")
                {
                    await ResponseMessage(new { status = "fail", message = "Access Denied" }, context, 400);
                }
                else if (newToken == "-2")
                {
                    await ResponseMessage(new { status = "fail", message = "The Token has expired" }, context, 401);
                }
                else if (newToken != "")
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers", "newToken");
                    context.Response.Headers.Add("newToken", newToken);
                    await next(context);
                }
                else
                {
                    await ResponseMessage(new { status = "fail", message = "Unauthorized Access" }, context, 401);
                }
                #endregion
            }
            else if (context.Request.Path.ToString().Contains(_options.Path, StringComparison.Ordinal) && context.Request.Method == HttpMethods.Post)
            {
                await GenerateToken(context);
            }
            else
            {
                await ResponseMessage(new { status = "fail", message = "Method Not Allowed." }, context, 405);
            }

        }

        private async Task GenerateToken(HttpContext context)
        {
            LoginDataModel loginData = new LoginDataModel();
            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            try
            {
                using (var reader = new System.IO.StreamReader(context.Request.Body))
                {
                    var request_body = reader.ReadToEnd();
                    loginData = JsonConvert.DeserializeObject<LoginDataModel>(request_body, _serializerSettings);
                    if (loginData.UserName == null) loginData.UserName = "";
                    if (loginData.Password == null)
                    {
                        loginData.Password = "";
                    }
                    if (loginData.LoginEmail == null) loginData.LoginEmail = "";
                }
            }
            catch
            {
                var objResponseFormat = new
                {
                    error = "invalid_grant",
                    error_description = "The user name or password is incorrect.",
                };

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(objResponseFormat, _serializerSettings));
                return;
            }

            // Check Login Validation
            var userInfo = LoginTypeValidate(loginData);
            var Loginresponseformat = new
            {
                error = "invalid_grant",
                error_description = "The user name or password is incorrect."
            };
            if (userInfo == null || userInfo.UserID == 0)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(Loginresponseformat, _serializerSettings));
                return;
            }
            if (userInfo.AccessStatus == 2)
            {
                var objResponseFormat = new
                {
                    error = "invalid_grant",
                    error_description = "This user account is locked."
                };

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(objResponseFormat, _serializerSettings));
                return;
            }

            string userID = userInfo.UserID.ToString();
            var now = DateTime.UtcNow;
            var _tokenData = new TokenData();
            _tokenData.Sub = userInfo.UserName;
            _tokenData.Jti = await _options.NonceGenerator();
            _tokenData.Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString();
            _tokenData.UserID = userID;
            _tokenData.TicketExpireDate = now.Add(_options.Expiration);
            _tokenData.LoginUserID = userID;
            _tokenData.isMobile = "1";
            _tokenData.Email = !string.IsNullOrEmpty(userInfo.Email) ? userInfo.Email : "";
            var claims = BookingSystem.Operational.Globalfunction.GetClaims(_tokenData);

            var appIdentity = new ClaimsIdentity(claims);
            context.User.AddIdentity(appIdentity);

            string encodedJwt = CreateEncryptedJWTToken(claims);
            dynamic result = null;
            result = new
            {
                access_token = encodedJwt,
                expires_in = (int)_options.Expiration.TotalSeconds,
                UserID = userID,
                userName = userInfo.UserName,
                IsNewToken = true
            };
            var response = new
            {
                data = result
            };

            // Serialize and return the token.
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));

        }
        public async Task<dynamic> ReGenerateToken(HttpContext context)
        {
            try
            {
                string access_token = "";
                TokenData _tokenData = null;
                var pathstr = context.Request.Path.ToString();
                string[] patharr = pathstr.Split('/');

                int prequest = Array.IndexOf(patharr, "PublicRequest");
                int wrequest = Array.IndexOf(patharr, "WindowServiceRequest");
                string key = context.Session.GetString("SessionToken");

                if (prequest < 1 && wrequest < 1)
                {
                    var hdtoken = context.Request.Headers["Authorization"];
                    if (hdtoken.Count > 0)
                    {
                        access_token = hdtoken[0];
                        access_token = access_token.Replace("Bearer ", "");
                        try
                        {
                            var handler = new JwtSecurityTokenHandler();
                            handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                                new TokenValidationParameters  //this is necessary in both startup and here.
                                {

                                    // The signing key must match!
                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                                    RequireSignedTokens = true,
                                    // Validate the JWT Issuer (iss) claim
                                    ValidateIssuer = true,
                                    ValidIssuer = _options.Issuer,
                                    // Validate the JWT Audience (aud) claim
                                    ValidateAudience = false,
                                    ValidAudience = _options.Audience,
                                    // Validate the token expiry
                                    ValidateLifetime = true,
                                    // If you want to allow a certain amount of clock drift, set that here:
                                    ClockSkew = TimeSpan.Zero,
                                    TokenDecryptionKey = new SymmetricSecurityKey(_tokenencKey)
                                }, out SecurityToken tokenS);

                            var tokenJS = (JwtSecurityToken)tokenS;
                            if (tokenJS.SignatureAlgorithm != "A256KW")   //only allow HS256 alg --> change to new encryption alg A256KW
                                throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                            _tokenData = Globalfunction.GetTokenData(tokenJS);
                        }
                        catch (SecurityTokenExpiredException)
                        {
                            //return "-2";
                            // this code block is only for tmporatory usage. untail mobile remember password flow not fix yet.

                            var currentContext = _httpContextAccessor.HttpContext.Request;
                            string userAgent = currentContext.Headers["User-Agent"].ToString();
                            // string userAgent = "Unknown";
                            if (userAgent == "Unknown")
                            {
                                try
                                {
                                    // string IsNewToken = Convert.ToString(context.Request.Headers["IsNewToken"]);
                                    string IsNewToken = "false";
                                    var handler = new JwtSecurityTokenHandler();
                                    handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                                    new TokenValidationParameters  //this is necessary in both startup and here.
                                    {

                                        // The signing key must match!
                                        ValidateIssuerSigningKey = true,
                                        IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                                        RequireSignedTokens = true,
                                        // Validate the JWT Issuer (iss) claim
                                        ValidateIssuer = true,
                                        ValidIssuer = _options.Issuer,
                                        // Validate the JWT Audience (aud) claim
                                        ValidateAudience = false,
                                        ValidAudience = _options.Audience,
                                        // Validate the token expiry
                                        ValidateLifetime = false,
                                        // If you want to allow a certain amount of clock drift, set that here:
                                        ClockSkew = TimeSpan.Zero,
                                        TokenDecryptionKey = new SymmetricSecurityKey(_tokenencKey)
                                    }, out SecurityToken tokenS);

                                    var tokenJS = (JwtSecurityToken)tokenS;
                                    if (tokenJS.SignatureAlgorithm != "A256KW")   //only allow HS256 alg --> change to new encryption alg A256KW
                                        throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                                    _tokenData = Globalfunction.GetTokenData(tokenJS);

                                    if (IsNewToken == "true")
                                    {
                                        double expireTime1 = _tokenExpireMinute;                //Convert.ToDouble(_options.Expiration.TotalMinutes);
                                        DateTime issueDate1 = _tokenData.TicketExpireDate.AddMinutes(-expireTime1);
                                        DateTime NowDate1 = DateTime.UtcNow;

                                        if ((issueDate1 > NowDate1 || _tokenData.TicketExpireDate < NowDate1))
                                        {
                                            return "-2";
                                        }
                                    }
                                }
                                catch (Exception exec)
                                {
                                    string msg = exec.Message;
                                    var handler = new JwtSecurityTokenHandler();
                                    handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                                    new TokenValidationParameters  //this is necessary in both startup and here.
                                    {
                                        // The signing key must match!
                                        ValidateIssuerSigningKey = true,
                                        IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                                        RequireSignedTokens = true,
                                        // Validate the JWT Issuer (iss) claim
                                        ValidateIssuer = true,
                                        ValidIssuer = _options.Issuer,
                                        // Validate the JWT Audience (aud) claim
                                        ValidateAudience = false,
                                        ValidAudience = _options.Audience,
                                        // Validate the token expiry
                                        ValidateLifetime = false,
                                        // If you want to allow a certain amount of clock drift, set that here:
                                        ClockSkew = TimeSpan.Zero
                                    }, out SecurityToken tokenS);

                                    var tokenJS = (JwtSecurityToken)tokenS;
                                    if (tokenJS.SignatureAlgorithm != "HS256")   //only allow HS256 alg 
                                        throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                                    _tokenData = Globalfunction.GetTokenData(tokenJS);
                                }

                            }

                            else
                            {
                                return "-2";
                            }
                        }
                        catch (Exception iex)
                        {
                            string exmsge = iex.Message;
                            var handler = new JwtSecurityTokenHandler();
                            handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                                new TokenValidationParameters  //this is necessary in both startup and here.
                                {
                                    // The signing key must match!
                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                                    RequireSignedTokens = true,
                                    // Validate the JWT Issuer (iss) claim
                                    ValidateIssuer = true,
                                    ValidIssuer = _options.Issuer,
                                    // Validate the JWT Audience (aud) claim
                                    ValidateAudience = false,
                                    ValidAudience = _options.Audience,
                                    // Validate the token expiry
                                    ValidateLifetime = false,
                                    // If you want to allow a certain amount of clock drift, set that here:
                                    ClockSkew = TimeSpan.Zero
                                }, out SecurityToken tokenS);

                            var tokenJS = (JwtSecurityToken)tokenS;
                            if (tokenJS.SignatureAlgorithm != "HS256")   //only allow HS256 alg 
                                throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                            _tokenData = Globalfunction.GetTokenData(tokenJS);
                        }

                    }
                    else
                    {
                        return "";
                    }
                }
                _session.SetString("LoginUserID", _tokenData.LoginUserID);
                _session.SetString("LoginRemoteIpAddress", _tokenData.IPAddress);

                double expireTime = _tokenExpireMinute;
                DateTime issueDate = _tokenData.TicketExpireDate.AddMinutes(-expireTime);
                DateTime NowDate = DateTime.UtcNow;
                string functionName = patharr[4].ToLower();

                TblUser usrObj = _repository.TblUser.FindByCondition(x => x.UserID == Convert.ToInt32(_tokenData.LoginUserID)).FirstOrDefault();

                var exp = DateTime.UtcNow;
                var expires_in = exp.AddMinutes(expireTime).ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'");
                string userID = usrObj.UserID.ToString();
                var now = DateTime.UtcNow;
                var _newtokenData = new TokenData();
                _newtokenData.Sub = usrObj.UserName;
                _newtokenData.Jti = await _options.NonceGenerator();
                _newtokenData.Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString();
                _newtokenData.UserID = userID;
                _newtokenData.TicketExpireDate = now.Add(_options.Expiration);
                _newtokenData.LoginUserID = userID;
                _newtokenData.IPAddress = _tokenData.IPAddress;
                var claims = BookingSystem.Operational.Globalfunction.GetClaims(_newtokenData);
                var appIdentity = new ClaimsIdentity(claims);
                context.User.AddIdentity(appIdentity); //add custom identity because default identity has delay to get data in EventLogRepository
                string newToken = CreateEncryptedJWTToken(claims);
                return newToken;
            }
            catch (Exception ex)
            {
                string exmsge = ex.Message;
                return "-3";
            }
        }
        private dynamic LoginTypeValidate(LoginDataModel loginDataModel)
        {
            var userInfo = _repository.TblUser.GetLoginInfo(loginDataModel.UserName, loginDataModel.Password, loginDataModel.LoginEmail);
            if (userInfo.Count == 0)
            {
                return null;
            }
            var objUser = userInfo[0];
            string oldhash = objUser.Password;
            string oldsalt = objUser.Salt;
            var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var Configuration = appsettingbuilder.Build();
            int SettingFailCount = Convert.ToInt32(Configuration.GetSection("appsettings:LoginFailCount").Value);
            int UserLoginFailCount = 0;
            string settingfailcount = SettingFailCount.ToString();
            if (objUser != null)
            {
                UserLoginFailCount = objUser.LoginFailCount;
                if (objUser.AccessStatus == 2 || objUser.AccessStatus == 1)
                {
                    return objUser;
                }
                else if (UserLoginFailCount >= SettingFailCount)
                {
                    TblUser emp = _repository.TblUser.FindByCondition(x => x.UserID == objUser.UserID).FirstOrDefault();
                    emp.AccessStatus = 2;
                    _repository.TblUser.Update(emp);
                    return objUser;
                }
                else
                {
                    if (!string.IsNullOrEmpty(oldhash) && !string.IsNullOrEmpty(oldsalt))
                    {
                        bool flag = BookingSystem.Operational.Encrypt.SaltedHash.Verify(oldsalt, oldhash, loginDataModel.Password);
                        // Increase login_failure count  

                        if (objUser != null && objUser.UserID > 0)
                        {
                            var obj = _repository.TblUser.FindByCondition(x => x.UserID == objUser.UserID).FirstOrDefault();
                            if (flag == false)
                            {
                                var newfailcount = userInfo[0].LoginFailCount + 1;
                                objUser.LoginFailCount = newfailcount;
                                obj.LoginFailCount = newfailcount;
                                if (newfailcount.ToString() == settingfailcount && obj != null)
                                {
                                    obj.AccessStatus = 2;
                                    objUser.AccessStatus = 2;
                                    _repository.TblUser.Update(obj);
                                }
                                else if (newfailcount.ToString() != settingfailcount && obj != null)
                                {
                                    _repository.TblUser.Update(obj);
                                }
                                if (obj.AccessStatus == 0)
                                {
                                    return null;
                                }

                            }
                            else
                            {
                                //reset login_failure count               
                                if (obj != null)
                                {
                                    obj.LoginFailCount = 0;
                                    _repository.TblUser.Update(obj);
                                }
                            }
                        }
                    }
                }
            }
            return objUser;
        }

        private string CreateEncryptedJWTToken(Claim[] claims)
        {
            string encodedJwt = "";
            try
            {
                var now = DateTime.UtcNow;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Audience = _options.Audience,
                    Issuer = _options.Issuer,
                    Subject = new ClaimsIdentity(claims),
                    NotBefore = now,
                    IssuedAt = Globalfunction.UnixTimeStampToDateTime(Int32.Parse(claims.First(claim => claim.Type == "iat").Value)),
                    Expires = now.Add(_options.Expiration),
                    SigningCredentials = _options.SigningCredentials,
                    EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(_tokenencKey), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512)
                };
                var handler = new JwtSecurityTokenHandler();
                encodedJwt = handler.CreateEncodedJwt(tokenDescriptor);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return encodedJwt;
        }
    }
}