using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication.Database.DatabaseAccessObjects;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;
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

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = Configuration["JWT:Audience"],
                    ValidIssuer = Configuration["JWT:Issuer"],
                    IssuerSigningKey = new
                        SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes
                            (Configuration["JWT:Key"]))
                };
            });

            services.AddTransient<IDatabaseAccessObject<DataTransferObjectBase>, UserDbAccess>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IMailer, Mailer>();
            services.AddSession();
            services.AddRazorPages();


            services.AddFlashes().AddMvc(options => { options.Filters.Add(new ValidateModelStateFilter()); });

            services.AddScoped<UserAuthorizationFilter>();


            services.AddControllersWithViews();
        }

        private static void CreateAllTables(MySqlContext mySqlContext)
        {
            var dbObjectTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                .Where(x =>
                    x.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDatabaseAccessObject<>)))
                .ToList();

            foreach (var type in dbObjectTypes)
            {
                var dao = Activator.CreateInstance(type, mySqlContext);
                if (dao == null) continue;
                var method = dao.GetType().GetMethod("CreateTableIfNotExists");
                method?.Invoke(dao, new object[] { });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.Use(async (context, next) =>
            {
                var token = context.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + token);
                }

                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}