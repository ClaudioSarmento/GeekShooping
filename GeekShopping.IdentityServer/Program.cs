

using Duende.IdentityServer.Services;
using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Initializer;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Services;
using GeekShopping.ProductAPI.Model.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace GeekShopping.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];
            builder.Services.AddDbContext<MySQLContext>(options => options
                .UseMySql(connection,
                    new MySqlServerVersion(new Version(8, 1, 0))));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MySQLContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddScoped<IProfileService, ProfileService>();

            builder.Services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;

                }
            ).AddInMemoryIdentityResources(
                IdentityConfiguration.IdentityResource)
             .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
             .AddInMemoryClients(IdentityConfiguration.Clients)
             .AddAspNetIdentity<ApplicationUser>()
             .AddProfileService<ProfileService>()
             .AddDeveloperSigningCredential();

            var app = builder.Build();

            // Obtenha o serviço IDbInitializer do provedor de serviços
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var dbInitializer = serviceProvider.GetRequiredService<IDbInitializer>();

                // Agora você pode usar o objeto dbInitializer para inicialização de banco de dados
                dbInitializer.Initialize();
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

         
           
        
            app.Run();
        }
    }
}