using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess;
using OpenBots.Server.Infrastructure;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Security;
using OpenBots.Server.Security.DataAccess;
using OpenBots.Server.WebAPI.Controllers;
using OpenBots.Server.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using OpenBots.Server.Web.Hubs;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using OpenBots.Server.WebAPI.Hangfire;
using RouteCollection = Hangfire.Dashboard.RouteCollection;
using OpenBots.Server.Web.Hangfire;
using System.Text.Json.Serialization;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Options;
using Microsoft.AspNetCore.HttpOverrides;

namespace OpenBots.Server.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var corsPolicyOptions = Configuration.GetSection(CorsPolicyOptions.Origins).Get<CorsPolicyOptions>();
            var appInsightOptions = Configuration.GetSection(AppInsightOptions.ApplicationInsights).Get<AppInsightOptions>();
            var webAppUrlOptions = Configuration.GetSection(WebAppUrlOptions.WebAppUrl).Get<WebAppUrlOptions>();

            ConfigureKestrel(services);

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = 209715200;
                x.MultipartBodyLengthLimit = 209715200;
                x.MultipartHeadersLengthLimit = 209715200;
            });

            DependencyManager.ConfigureServices(services);           

            string baseOrigin = webAppUrlOptions.Url;
            string allowedOrigins = corsPolicyOptions.AllowedOrigins ;
            string exposedHeaders = corsPolicyOptions.ExposedHeaders;

            var origins = allowedOrigins.Split(";");
            var headers = exposedHeaders.Split(";");

            Array.Resize(ref origins, origins.Length + 1);
            origins[origins.Length - 1] = baseOrigin;

            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyOptions.PolicyName, builder =>
                {
                    if (!string.IsNullOrEmpty(allowedOrigins))
                    {
                        builder
                        .WithOrigins(origins)
                        .AllowCredentials();
                    }
                    else
                    {
                        builder.AllowAnyOrigin();
                    }

                    builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    
                    .WithExposedHeaders(headers);
                });
            });

            services
                    .AddControllers()
                    .AddNewtonsoftJson()
                    .AddJsonOptions(options =>
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            ConfigureLogging(services, appInsightOptions);

            ConfigureMVCWithSPA(services);

            ConfigureSignalR(services);

            services.Configure<ConfigurationValue>(Configuration.GetSection(
                                        ConfigurationValue.Values));

            services.AddApplicationInsightsTelemetry();

            services.AddLogging(
            logging =>
            {
                if (appInsightOptions.IsEnabled)
                {
                    logging.AddApplicationInsights();
                }
                logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace); //Set the log level here
            });

            services.AddMvcCore();

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("Sql"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();

            ConfigureDbContext(services);
            ConfigureIdentity(services);
            ConfigureSwaggerDocumentation(services);

            AddHealthCheck(services);

            // Add API versioning and report supported/deprecated API versions in http request headers
            services.AddMvcCore();
            services.AddApiVersioning( options => 
                options.ReportApiVersions = true);
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            //Security configuration
            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.Password.RequireDigit = false;
                config.Password.RequiredLength = 1;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
                .AddUserManager<ApplicationIdentityUserManager>()
                .AddEntityFrameworkStores<SecurityDbContext>()
                .AddDefaultTokenProviders();

            #region Add Authentication
            var tokensOptions = Configuration.GetSection(TokensOptions.Tokens).Get<TokensOptions>();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokensOptions.Key));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = String.IsNullOrEmpty(tokensOptions.Audience) ? false : true,
                    ValidAudience = tokensOptions.Audience,
                    ValidateIssuer = String.IsNullOrEmpty(tokensOptions.Issuer) ? false : true,
                    ValidIssuer = tokensOptions.Issuer,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
                config.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddIdentityCore<ApplicationUser>(options => { }).AddEntityFrameworkStores<SecurityDbContext>();
            services.AddScoped<IUserStore<ApplicationUser>, UserOnlyStore<ApplicationUser, SecurityDbContext>>();
            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = false;
            });

            #endregion
        }

        private static void ConfigureSignalR(IServiceCollection services)
        {
            services.AddSignalR();
        }

        private static void ConfigureLogging(IServiceCollection services, AppInsightOptions appInsightOptions)
        {
            services.AddLogging(
                logging =>
                {
                    if (appInsightOptions.IsEnabled)
                    {
                        logging.AddApplicationInsights();
                    }
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace); //you can set the logLevel here
                });
        }

        private static void ConfigureMVCWithSPA(IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(typeof(AccessActionFilter))).AddMvcOptions(o => o.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
              .ConfigureApiBehaviorOptions(options =>
              {
                  options.InvalidModelStateResponseFactory = context =>
                  {
                      var problems = new ServiceBadRequest(context, null);
                      return new BadRequestObjectResult(problems);
                  };
              });

            services.AddControllersWithViews();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        protected virtual IServiceCollection ConfigureDbContext(IServiceCollection services)
        {
            //var isSqlServerConnection = Configuration.GetValue<bool>("DbOption:UseSqlServer");
            var isSqlServerConnection = Configuration.GetSection("DbOption")?.GetSection("UseSqlServer").Value;

            if (Boolean.TryParse(isSqlServerConnection, out var isSqlServer))
            {
                services.AddDbContext<SecurityDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Sql")).UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll),
                ServiceLifetime.Transient);

                services.AddDbContext<StorageContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Sql")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
                ServiceLifetime.Transient);
            }
            else
            {
                services.AddDbContext<SecurityDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Sql")).UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll),
                ServiceLifetime.Transient);

                services.AddDbContext<StorageContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("Sql")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
             , ServiceLifetime.Transient);
            }
            return services;
        }

        private void ConfigureSwaggerDocumentation(IServiceCollection services)
        {
            if (bool.Parse(Configuration["App:EnableSwagger"]))
            {
                services.AddSwaggerExamplesFromAssemblyOf<PersonEmailsController>();
                services.AddSwaggerExamplesFromAssemblyOf<Organization>();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenBots Server API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });
                    c.OperationFilter<SecurityRequirementsOperationFilter>();

                    c.ExampleFilters();
                    var apiCommentPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "OpenBots.Server.Web.xml");
                    var modelCommentPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "OpenBots.Server.Model.xml");
                    c.IncludeXmlComments(apiCommentPath, true);
                    c.IncludeXmlComments(modelCommentPath, true);
                });
            }
        }

        private void AddHealthCheck(IServiceCollection services)
        {
            var healthCheckOptions = Configuration.GetSection(HealthCheckSetupOptions.HealthChecks).Get<HealthCheckSetupOptions>();

            if (healthCheckOptions.isEnabled)
            {
                services.AddHealthChecks()
                    .AddCheck<AppVersionHealthCheck>("Version")
                    .AddDbContextCheck<StorageContext>("Database")
                    .AddApplicationInsightsPublisher();

                var healthCheckUISection = Configuration.GetSection(HealthCheckSetupOptions.HealthChecks).GetSection(HealthChecksUIOptions.HealthChecksUI);
                services.Configure<HealthChecksUISettings>(settings =>
                {
                    healthCheckUISection.Bind(settings, c => { c.BindNonPublicProperties = true; });
                });
                services.AddHealthChecksUI().AddInMemoryStorage();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            var iPFencingOptions = Configuration.GetSection(IPFencingOptions.IPFencing).Get<IPFencingOptions>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            var logger = loggerFactory.CreateLogger("Logging");
            app.ConfigureExceptionHandler(logger);
            var corsPolicyOptions = Configuration.GetSection(CorsPolicyOptions.Origins).Get<CorsPolicyOptions>();
            app.UseCors(corsPolicyOptions.PolicyName);
            app.UseDefaultFiles();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.UseAuthentication();
            if (iPFencingOptions.IPFencingCheck.Equals("EveryRequest"))
            {
                app.UseIPFilter();
            }
            app.UseMvc();
            app.UseRouting();
            app.UseAuthorization();

            //UpdateDatabase(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notification");
                ConfigureHealthcheck(endpoints, app);
            });

            if (bool.Parse(Configuration["App:EnableSwagger"]))
            {
                //Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                //Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                //Specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenBots API v1");
                });
            }

            var tokensOptions = Configuration.GetSection(TokensOptions.Tokens).Get<TokensOptions>();
            var tokenValidationParameters = new TokenValidationParameters()//////
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokensOptions.Key)),
                ValidateAudience = true,
                ValidAudience = tokensOptions.Audience,
                ValidateIssuer = true,
                ValidIssuer = tokensOptions.Issuer,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            var webAppUrlOptions = Configuration.GetSection(WebAppUrlOptions.WebAppUrl).Get<WebAppUrlOptions>();
            var options = new DashboardOptions
            {
                AppPath = webAppUrlOptions.Url, 
                DashboardTitle = "OpenBots Server",
                Authorization = new IDashboardAuthorizationFilter[]
                  {
                    new HangfireAuthorizationFilterByQuerry(tokenValidationParameters, configuration)
                  }
            };

            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();

            app.UseForwardedHeaders();

            ////////////////////////////
            // Added Service Bus functionality for Message Processing from within Service 
            // or Other Microservices
            var bus = app.ApplicationServices.GetService<IQueueSubscriber>();
            if (bus != null)
                bus.Register();

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        private void ConfigureHealthcheck(IEndpointRouteBuilder endpoints, IApplicationBuilder app)
        {
            var healthCheckOptions = Configuration.GetSection(HealthCheckSetupOptions.HealthChecks).Get<HealthCheckSetupOptions>();

            if (healthCheckOptions.isEnabled)
            {
                string healthcheckEndpoint = healthCheckOptions.Endpoint;
                if (string.IsNullOrWhiteSpace(healthcheckEndpoint))
                    healthcheckEndpoint = "/health";
                endpoints.MapHealthChecks(healthcheckEndpoint, new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }

            if (healthCheckOptions.HealthChecksUI.HealthChecksUIEnabled)
            {
                string healthcheckUI = healthCheckOptions.HealthChecksUI.UIRelativePath;
                string healthcheckAPI = healthCheckOptions.HealthChecksUI.ApiRelativePath;
                if (string.IsNullOrWhiteSpace(healthcheckUI))
                    healthcheckUI = "/healthcheck-ui";

                if (string.IsNullOrWhiteSpace(healthcheckAPI))
                    healthcheckAPI = "/healthcheck-api";

                endpoints.MapHealthChecksUI(a => { a.UIPath = healthcheckUI; a.UseRelativeApiPath = false; a.UseRelativeResourcesPath = false;  a.ApiPath = healthcheckAPI; });
            }
        }

        private void UpdateDatabase(IApplicationBuilder app)
        {
            string migration = Configuration["DbOption:Migrate"].ToString();
            bool switchMigration = bool.Parse(migration);
            if (switchMigration)
            {
                using (var serviceScope = app.ApplicationServices
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<SecurityDbContext>())
                    {
                        context.Database.Migrate();
                    }
                    using (var context = serviceScope.ServiceProvider.GetService<StorageContext>())
                    {
                        context.Database.Migrate();
                    }
                }
            }
        }

        private void ConfigureKestrel(IServiceCollection services)
        {
            var kestrelOptions = Configuration.GetSection(KestrelOptions.Kestrel).Get<KestrelOptions>();
            if (kestrelOptions.IsEnabled)
            {
                services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
                {
                    options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, kestrelOptions.Port),
                        listenOptions =>
                        {
                            var certPassword = kestrelOptions.Certificates.Password;
                            var certPath = kestrelOptions.Certificates.Path;
                            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, certPassword);
                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                            {
                                ServerCertificate = cert
                            });
                        });

                    options.AddServerHeader = false;
                    options.Limits.MaxRequestLineSize = 16 * 1024; // 16KB Limit
                });
            }
            else
            {
                //Microsoft.AspNetCore.Server.IISIntegration.IISMiddleware
                if (kestrelOptions.UseIISIntegration)
                {
                    services.Configure<Microsoft.AspNetCore.Server.IISIntegration.IISDefaults>(Configuration);
                }
            }

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        private IApplicationBuilder UseHangfireDashboard(IApplicationBuilder app, string pathMatch = "/hangfire", DashboardOptions options = null, JobStorage storage = null)
        {
            var services = app.ApplicationServices;
            storage = storage ?? services.GetRequiredService<JobStorage>();
            options = options ?? services.GetService<DashboardOptions>() ?? new DashboardOptions();
            var routes = app.ApplicationServices.GetRequiredService<RouteCollection>();

            // Use our custom middleware.
            app.Map(new PathString(pathMatch), x => x.UseMiddleware<CustomHangfireDashboardMiddleware>(storage, options, routes));
            return app;
        }
    }
}
