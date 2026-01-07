using Config.Net;
using Microsoft.EntityFrameworkCore;
using RaoVids.Components;

namespace RaoVids
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load configuration from environment variables/.env file.
            var appSettings = new ConfigurationBuilder<IAppSettings>()
                .UseEnvironmentVariables()
                .UseDotEnvFile()
                .Build();

            Console.WriteLine($"Database conn string: {appSettings.DatabaseConnString}");

            // Create web application.
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add Entity Framework.
            builder.Services.AddEntityFrameworkNpgsql()
                .AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(appSettings.DatabaseConnString));

            // Build app.
            var app = builder.Build();

            // Check connection to database, which isn't strictly necessary but should make
            // configuring the service on Kubernetes a bit more painless if it goes wrong for some
            // reason.
            using (var scope = app.Services.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    dbContext.Database.OpenConnection();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to open database connection: " + e.Message);
                }

                Console.WriteLine("Connected to database successfully, starting web app");
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
