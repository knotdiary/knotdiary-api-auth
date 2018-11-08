using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using KnotDiary.AuthApi.Extensions;
using KnotDiary.AuthApi.Http;
using KnotDiary.AuthApi.Infrastructure;
using KnotDiary.AuthApi.Services;
using KnotDiary.Common;
using KnotDiary.Common.Web.Extensions;
using KnotDiary.Common.Web.Infrastructure;
using KnotDiary.Common.Web.Infrastructure.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Splunk;
using System;
using System.Linq;
using System.Net;

namespace KnotDiary.AuthApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        const string ServiceName = "KnotDiary Auth API";

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;

            var splunkToken = Configuration.GetValue<string>("Logging:SplunkToken");
            var splunkUrl = Configuration.GetValue<string>("Logging:SplunkCollectorUrl");
            var splunkFormatter = new CompactSplunkJsonFormatter(true, environment.EnvironmentName, "api_log", Environment.MachineName);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .WriteTo.EventCollector(splunkUrl, splunkToken, splunkFormatter)
                .WriteTo.Console()
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<StorageConfiguration>(Configuration);

            services.AddSingleton(Log.Logger);
            services.AddSingleton(typeof(HttpUserServiceSettings), Configuration.GetSection("Services:UsersApi").Get<HttpUserServiceSettings>());
            services.AddSingleton<IHttpUserService, HttpUserService>();
            services.AddSingleton<IUserService, UserService>();

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            services.AddSingleton(serializerSettings);

            services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)))
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSwaggerDocumentation(ServiceName);

            // cors policy registration
            var icps = services.FirstOrDefault(p => p.ServiceType == typeof(ICorsPolicyService));
            if (icps != null) services.Remove(icps);
            services.AddTransient<ICorsPolicyService, CorsPolicyService>();

            // cors setup
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            // Add framework services.
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            // add identity server
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(new Clients(Configuration).Get())
                .AddResourceOwnerPasswordValidator()
                .AddUserProfileService();

            // add authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = Configuration["App:Web:Url"];
                    options.Audience = "api1";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            app.UseMiddleware<HeaderLogger>();
            app.UseMiddleware<RequestLogger>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // cors config
            app.UseCors(c =>
            {
                c.AllowAnyHeader();
                c.AllowAnyMethod();
                c.AllowAnyOrigin();
                c.AllowCredentials();
            });

            // swagger configuration
            app.UseSwaggerDocumentation(ServiceName);

            // global exception handler
            app.UseExceptionHandler(options => {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        await context.Response.WriteAsync(ex.Error.Message).ConfigureAwait(false);
                    }
                });
            });

            // identity server config
            app.UseIdentityServer();
            app.UseMvc();
        }
    }
}
