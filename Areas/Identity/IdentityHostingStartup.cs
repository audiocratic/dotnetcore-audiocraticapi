using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AudiocraticAPI.Areas.Identity.Data;

[assembly: HostingStartup(typeof(AudiocraticAPI.Areas.Identity.IdentityHostingStartup))]
namespace AudiocraticAPI.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<IdentityDataContext>(options =>
                {
                    string connString = System.Diagnostics.Debugger.IsAttached ? "Debug" : "BudgetContext";
                                  
                    options.UseMySql(
                        context.Configuration.GetConnectionString(connString));
                });

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<IdentityDataContext>();
            });
        }
    }
}