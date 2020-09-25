using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RareDisease.Data;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
        {
            o.LoginPath = new PathString("/Login");
            o.AccessDeniedPath = new PathString("/Error");
        });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMemoryCache();
            
            services.AddDbContext<RareDiseaseDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LogConnection")));
            RareDiseaseGPDbContext.RDR_ConnectionString = Configuration.GetConnectionString("LogConnection");   //为数据库连接字符串赋值
            services.AddHttpContextAccessor();
            services.AddScoped<ILocalMemoryCache, LocalMemoryCache>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IRdrDataRepository, RdrDataRepository>();
            services.AddScoped<INLPSystemRepository, NLPSystemRepository>();
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("罕见病", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "罕见病 API",
                    Description = "API for 罕见病",
                    Contact = new OpenApiContact() { Name = "胡真武", Email = "huzhenwu1989312@outlook.com" }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseSwagger();
            //Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("v1/swagger.json", "罕见病 API");
            });

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
           
        }
    }
}
