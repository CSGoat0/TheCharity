using Hangfire;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TheCharityBLL.Helpers;
using TheCharityBLL.Jobs.Registry.Abstraction;
using TheCharityPL.Middlewares;

namespace TheCharityPL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.TheCharityEnhancedConnectionString(builder.Configuration);
            builder.Services.TheCharityDependencyInjection();
            builder.Services.TheCharityIdentity(builder.Configuration);
            builder.Services.TheCharityConfiguration(builder.Configuration);
            builder.Services.ThirdPartyAuthentication(builder.Configuration);
            builder.Services.AddHangfireServices();
            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                // This prevents unknown properties from crashing deserialization
            }); ;
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "The Charity API",
                    Version = "v1"
                });

                // Load XML from TheCharityPL (controllers)
                var plXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var plXmlPath = Path.Combine(AppContext.BaseDirectory, plXmlFile);
                if (File.Exists(plXmlPath))
                {
                    c.IncludeXmlComments(plXmlPath);
                }

                // Load XML from TheCharityBLL (DTOs)
                var bllXmlFile = "TheCharityBLL.xml";
                var bllXmlPath = Path.Combine(AppContext.BaseDirectory, bllXmlFile);
                if (File.Exists(bllXmlPath))
                {
                    c.IncludeXmlComments(bllXmlPath);
                }
            });

            // IDK but i got an error and i couldnt figuer out why so i asked claud and it said add this - Mohamed Rashid
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Configure Hangfire
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("DefaultConnection connection string is missing");
            }

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2;
                options.Queues = new[] { "voting", "maintenance", "default" };
                options.ServerName = $"TheCharity-API-{Environment.MachineName}";
            });

            var app = builder.Build();

            // Configure Hangfire dashboard (secured - Super Admin only)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "The Charity Platform - Background Jobs",
                StatsPollingInterval = 5000
            });

            // Register recurring jobs at startup
            using (var scope = app.Services.CreateScope())
            {
                var jobRegistry = scope.ServiceProvider.GetRequiredService<IJobRegistry>();
                jobRegistry.RegisterAllRecurringJobs();
            }

            app.MapHealthChecks("/health");// check if the db connected or not

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //global exception handling middleware
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}