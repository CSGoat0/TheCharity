using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TheCharityBLL.Authorization.Filters;
using TheCharityBLL.Authorization.Handlers;
using TheCharityBLL.Authorization.Requirements;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Events.DonationEvents;
using TheCharityBLL.Events.EventHandlers.CampaignEventHandlers;
using TheCharityBLL.Events.EventHandlers.DonationEventHandlers;
using TheCharityBLL.Events.Implementation;
using TheCharityBLL.Jobs.Emails;
using TheCharityBLL.Jobs.Registry.Abstraction;
using TheCharityBLL.Jobs.Registry.Implementation;
using TheCharityBLL.Jobs.Services;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityBLL.Services.Abstraction.Payment;
using TheCharityBLL.Services.Implementation;
using TheCharityBLL.Services.Implementation.MoneyDonation;
using TheCharityBLL.Services.Implementation.PaymentGateway;
using TheCharityBLL.Services.Repository;
using TheCharityBLL.Settings;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;
using TheCharityDAL.Repositories.Implementation;
using IAuthorizationService = TheCharityBLL.Services.Abstraction.IAuthorizationService;

namespace TheCharityBLL.Helpers
{
    public static class ServiceExtensions
    {
        public static void TheCharityIdentity(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDataProtection();
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TheCharityDbContext>()
            .AddDefaultTokenProviders();
        }
        public static void TheCharityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        }
        public static void TheCharityEnhancedConnectionString(this IServiceCollection services, IConfiguration configuration, string stringName = "defaultConnection")
        {
            var connectionString = configuration.GetConnectionString(stringName);
            services.AddDbContext<TheCharityDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly("TheCharityDAL")
                    ));
            services.AddHealthChecks()
        .AddSqlServer(
            connectionString: connectionString,
            name: "TheCharity-DB",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "charity" }
        );
        }
        public static void TheCharityDependencyInjection(this IServiceCollection services)
        {
            // Repository Injection
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<IDonatedItemsRepository, DonatedItemsRepository>();
            services.AddScoped<IDonationRepository, DonationRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            // Services Injection
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICampaignNotificationService, CampaignNotificationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IDonatedItemService, DonatedItemService>();
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IPaymobService,PaymobService>();
            services.AddScoped<IPaymentInfoService, PaymentInfoService>();
            services.AddScoped<IUserService, UserService>();
            // Email Job Services
            services.AddScoped<AutoExpireCampaignsJob>();
            services.AddScoped<CampaignDeadlineReminderJob>();
            services.AddScoped<DeadlineExtensionConfirmationJob>();
            services.AddScoped<NewCampaignNotificationJob>();
            services.AddScoped<SendMilestoneEmailJob>();
            services.AddScoped<WeeklyCampaignDigestJob>();
            // Event Handlers
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IEventHandler<CampaignCompletedEvent>, CampaignCompletedEventHandler>();
            services.AddScoped<IEventHandler<CampaignCreatedEvent>, CampaignCreatedEventHandler>();
            services.AddScoped<IEventHandler<CampaignDeadlineExtendedEvent>, CampaignDeadlineExtendedEventHandler>();
            services.AddScoped<IEventHandler<CampaignDismissedEvent>, CampaignDismissedEventHandler>();
            services.AddScoped<IEventHandler<CampaignDonationReceivedEvent>, CampaignDonationEventHandler>();
            services.AddScoped<IEventHandler<CampaignDonationReceivedEvent>, IncrementCampaignMoneyEventHandler>();
            services.AddScoped<IEventHandler<CampaignExpiredEvent>, CampaignExpiredEventHandler>();
            services.AddScoped<IEventHandler<CampaignPostponedEvent>, CampaignPostponedEventHandler>();
            services.AddScoped<IEventHandler<CampaignStatusChangedEvent>, CampaignStatusChangedEventHandler>();
            // Register IHttpContextAccessor (for handlers)
            services.AddHttpContextAccessor();
            // Register Authorization Handlers
            services.AddScoped<IAuthorizationHandler, CanManageCampaignHandler>();
            services.AddScoped<IAuthorizationHandler, CanManageOrganizationHandler>();
            services.AddScoped<IAuthorizationHandler, CanManageSubAdminsHandler>();
            services.AddScoped<IAuthorizationHandler, CanPerformBulkOperationsHandler>();
            services.AddScoped<IAuthorizationHandler, CanUpdatePaymentInfoHandler>();
            services.AddScoped<IAuthorizationHandler, IsSharedCampaignCreatorHandler>();
            services.AddScoped<IAuthorizationHandler, IsSuperAdminHandler>();
            // Register Authorization Filters
            services.AddScoped<CanCreateCampaignFilter>();
            // Add Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanManageCampaign", policy =>
                    policy.Requirements.Add(new CanManageCampaignRequirement()));

                options.AddPolicy("CanManageOrganization", policy =>
                    policy.Requirements.Add(new CanManageOrganizationRequirement()));

                options.AddPolicy("CanManageSubAdmins", policy =>
                    policy.Requirements.Add(new CanManageSubAdminsRequirement()));

                options.AddPolicy("CanPerformBulkOperations", policy =>
                    policy.Requirements.Add(new CanPerformBulkOperationsRequirement()));

                options.AddPolicy("CanUpdatePaymentInfo", policy =>
                    policy.Requirements.Add(new CanUpdatePaymentInfoRequirement()));

                options.AddPolicy("IsSharedCampaignCreator", policy =>
                    policy.Requirements.Add(new IsSharedCampaignCreatorRequirement()));

                options.AddPolicy("IsSuperAdmin", policy =>
                    policy.Requirements.Add(new IsSuperAdminRequirement()));
            });
            // mapper Injection
            services.AddAutoMapper(cfg => {
                cfg.AddProfile<UserMapperProfile>();
                cfg.AddProfile<PaymentInfoMappingProfile>();
            });
            services.AddScoped<DonationMapper>();

        }
        public static void ThirdPartyAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
            };
        })
        .AddCookie("ExternalCookie"); // dedicated scheme for OAuth handshake only
        //.AddGoogle(options =>
        //{
        //    options.SignInScheme = "ExternalCookie"; // scoped here only
        //    options.ClientId = Configuration["Authentication:Google:ClientID"];
        //    options.ClientSecret = Configuration["Authentication:Google:SecretKey"];
        //})
        //.AddFacebook(options =>
        //{
        //    options.SignInScheme = "ExternalCookie"; // scoped here only
        //    options.AppId = Configuration["Authentication:Facebook:ClientID"];
        //    options.AppSecret = Configuration["Authentication:Facebook:SecretKey"];
        //});
        }
        public static void AddHangfireServices(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IJobSchedulerService, HangfireJobSchedulerService>();
            services.AddScoped<IJobRegistry, JobRegistry>();

            // Register JobExecutor
            services.AddSingleton<JobExecutor>();
        }
    }
}       
