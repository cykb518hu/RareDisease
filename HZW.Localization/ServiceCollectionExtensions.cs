using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace HZW.Localization
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNetCoreStackLocalization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<LocalizationInMemoryCacheProvider>();
            services.AddScoped<IStringLocalizer, JsonStringLocalizer>((ctx) =>
            {
                var cacheProvider = ctx.GetService<LocalizationInMemoryCacheProvider>();
                return new JsonStringLocalizer(cacheProvider);
            });

        }

        public static void UseNetCoreStackLocalization(this IApplicationBuilder app)
        {

            //var cacheProvider = app.ApplicationServices.GetService<LocalizationInMemoryCacheProvider>();
            //var supportedCultures = new List<CultureInfo>();
            //RequestCulture defaultCulture = null;
            //var languageRepo = cacheProvider.GetAllLanguage();
            //foreach (var language in languageRepo)
            //{
            //    if (defaultCulture == null && language.IsDefaultLanguage)
            //        defaultCulture = new RequestCulture(language.CultureName);

            //    supportedCultures.Add(new CultureInfo(language.CultureName));
            //}

            //var requestLocalizationOptions = new RequestLocalizationOptions();
            //requestLocalizationOptions.DefaultRequestCulture = defaultCulture;
            //requestLocalizationOptions.SupportedCultures = supportedCultures;
            //requestLocalizationOptions.SupportedUICultures = supportedCultures;

            //app.UseRequestLocalization(requestLocalizationOptions);
        }

    }
}
