﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RareDisease.Data;
using RareDisease.Data.Handler;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_any";
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
            RareDiseaseGPDbContext.NLP_ConnectionString = Configuration.GetConnectionString("NLPConnection");   //为数据库连接字符串赋值
            services.AddHttpContextAccessor();

            services.AddHttpClient("DiseaseHost", c =>
            {
                c.BaseAddress = new Uri(Configuration.GetValue<string>("NLPAddress:DiseaseHost"));
            });
            services.AddHttpClient("HPOStringMatchHost", c =>
            {
                c.BaseAddress = new Uri(Configuration.GetValue<string>("NLPAddress:HPOStringMatchHost"));
            });
            services.AddHttpClient("HPOSpacyMatchHost", c =>
            {
                c.BaseAddress = new Uri(Configuration.GetValue<string>("NLPAddress:HPOSpacyMatchHost"));
            });
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
            });
            services.AddScoped<ILocalMemoryCache, LocalMemoryCache>();

            services.AddScoped<IRdrDataRepository, RdrDataRepository>();
            services.AddScoped<INLPSystemRepository, NLPSystemRepository>();
            services.AddScoped<IExcelRepository, ExcelRepository>();
            if (Configuration.GetValue<string>("EnvironmentName") == "Production")
            {
                services.AddScoped<ILogRepository, GPLogRepositry>();
            }
            else
            {
                services.AddScoped<ILogRepository, LogRepository>();
            }
            LogoHandler.IsHuaxi = Configuration.GetValue<bool>("GlobalSetting:HuaxiLogo");
            services.AddCors(options =>
             {
                 options.AddPolicy(MyAllowSpecificOrigins,
                     builder =>
                     {
                         builder.AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowAnyOrigin();
                     });
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            env.EnvironmentName = Configuration.GetValue<string>("EnvironmentName");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Index}/{id?}");
            });
           
        }
    }
}
