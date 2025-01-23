using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Models;
using SocialMedia.Services;

namespace SocialMedia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            //get configuration
            var connectionString = configuration.GetConnectionString("DBContext");
            var cloudName = configuration.GetValue<string>("AccountSettings:CloudName");
            var apiKey = configuration.GetValue<string>("AccountSettings:ApiKey");
            var apiSecret = configuration.GetValue<string>("AccountSettings:ApiSecret");

            if (new[] { cloudName, apiKey, apiSecret }.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Please specify Cloudinary account details!");
            }

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            builder.Services.AddSingleton(new Cloudinary(new Account(cloudName, apiKey, apiSecret)));
            builder.Services.AddSingleton<CloudinaryServices>();
            builder.Services.AddDbContext<SocialNetworkContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddScoped<SocialNetworkContext>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromSeconds(60);
                options.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapHub<SignalRService>("/signalRService");
            app.Run();
        }
    }
}
