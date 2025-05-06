using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Services; // Ensure this using statement is present
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerceProcurementSystem
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
            // --- Database Context Configuration ---
            // Ensure a connection string named "DefaultConnection" exists in appsettings.json
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            services.AddDbContext<ProcurementContext>(options =>
                options.UseSqlServer(connectionString));

            // Provides detailed database error pages during development
            services.AddDatabaseDeveloperPageExceptionFilter();

            // --- MVC Controllers & Views ---
            services.AddControllersWithViews();

            // --- Typed HttpClient for Socrata Service ---
            services.AddHttpClient<ISocrataService, SocrataService>((sp, client) =>
            {
                // Get the "Socrata" section from configuration
                var socrataConfig = sp.GetRequiredService<IConfiguration>().GetSection("Socrata");

                // Validate and set BaseUri
                var baseUriString = socrataConfig["BaseUri"];
                if (string.IsNullOrEmpty(baseUriString))
                {
                    // Throw a clear exception if configuration is missing
                    throw new InvalidOperationException("Socrata:BaseUri configuration is missing or empty in appsettings.json");
                }
                client.BaseAddress = new Uri(baseUriString);

                // Validate and set AppToken header
                var appToken = socrataConfig["AppToken"];
                if (string.IsNullOrEmpty(appToken))
                {
                    // Throw a clear exception if configuration is missing
                    throw new InvalidOperationException("Socrata:AppToken configuration is missing or empty in appsettings.json");
                }
                client.DefaultRequestHeaders.Add("X-App-Token", appToken);
            });

            // --- Optional: Register ExternalDataImportService if needed ---
            // If you intend to use ExternalDataImportService, register it. Decide if it needs HttpClient too.
            // services.AddScoped<ExternalDataImportService>(); // Use AddScoped if it uses Scoped DbContext
            // OR
            // services.AddHttpClient<ExternalDataImportService>(); // If it needs its own HttpClient instance
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Optional: Use database error page middleware for development
                app.UseMigrationsEndPoint(); // Use this if using EF Core migrations
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // Use a generic error handler page
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); // Enforce HTTPS in production
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            app.UseStaticFiles(); // Serve static files like CSS, JS, images from wwwroot

            app.UseRouting(); // Marks the position in the middleware pipeline where routing decisions are made

            app.UseAuthorization(); // Marks the position where authorization is applied (if used)

            // Defines the endpoints (routes) for the application
            app.UseEndpoints(endpoints =>
            {
                
                // Map the default controller route: e.g., /Home/Index/OptionalId
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                // Add other endpoints like MapRazorPages() if needed
                
            }); 
        }
    }
} 