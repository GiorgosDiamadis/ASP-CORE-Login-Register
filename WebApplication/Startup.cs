using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication.Database;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Filters;
using WebApplication.Services;
using WebApplication.Services.Interfaces;

namespace WebApplication
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
            services.AddDbContext<WebApplicationContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("WebApplicationContext")));

            MySqlContext mySqlContext = new MySqlContext(Configuration.GetConnectionString("DefaultConnection"));
            ServiceDescriptor serviceDescriptor = new ServiceDescriptor(typeof(MySqlContext), mySqlContext);
            services.Add(serviceDescriptor);
            CreateAllTables(mySqlContext);

            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IMailer, Mailer>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddSession();
            services.AddRazorPages();


            services.AddFlashes().AddMvc(options =>
            {
                options.Filters.Add(new ValidateModelStateFilter());
                options.Filters.Add(new HandleAntiForgeryToken());
            });

            services.AddScoped<UserAuthorizationFilter>();

            services.AddControllersWithViews();
        }

        private static void CreateAllTables(MySqlContext mySqlContext)
        {
            var dbObjectTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                .Where(x =>
                    x.GetInterfaces().Any(i => i == typeof(IDatabaseAccessObject)))
                .ToList();

            foreach (var type in dbObjectTypes)
            {
                var dao = Activator.CreateInstance(type, mySqlContext);
                var method = dao?.GetType().GetMethod("CreateTableIfNotExists");
                method?.Invoke(dao, new object[] { });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}