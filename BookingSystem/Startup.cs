using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using CustomTokenAuthProvider;
using BookingSystem.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
namespace BookingSystem
{
    public partial class Startup
    {
       // public SymmetricSecurityKey signingKey;
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            StaticConfiguration = Configuration;
        }

        public IConfigurationRoot Configuration { get; }

        public static IConfigurationRoot StaticConfiguration { get; set;}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors(Configuration);

            services.ConfigureIISIntegration();

            services.ConfigureMySqlContext(Configuration);

            services.ConfigureRepositoryWrapper();

            services.AddTransient<TokenProviderMiddleware>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

         //   services.AddWkhtmltopdf();

            //#ST added to retrieve ip addreess
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

            // Add framework services.
            services.AddMvc()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddSession(options =>
            {
                // Set a short timeout for easy testdoting.
                options.IdleTimeout = TimeSpan.FromSeconds(30);
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookingSystem", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer Authentication with JWT Token",
                    Type = SecuritySchemeType.Http
                });
                OpenApiSecurityRequirement securityRequirement = new();
                OpenApiSecurityScheme secondSecurityDefinition = new()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                securityRequirement.Add(secondSecurityDefinition, Array.Empty<string>());
                c.AddSecurityRequirement(securityRequirement);
                c.OperationFilter<AddRequiredSwaggerHeaderParameter>();
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
              services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //Set for console log
	        //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
	        //Set for log4net
	        loggerFactory.AddLog4Net(Configuration.GetValue<string>("Log4NetConfigFile:Name"));
            // IMPORTANT: This session call MUST go before UseMvc()
            
            app.UseSession();

            //app.UseStaticFiles(); // For the wwwroot folder
            //app.UseS; // For the wwwroot folder
            // Serve my app-specific default file, if present.
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);
            app.UseStaticFiles();

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors("CorsAllowAllPolicy");
            
                app.UseSwagger();
                app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseTokenProviderMiddleware();

            //loggerFactory.AddConsole(LogLevel.Warning, true).AddDebug();
            LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}
