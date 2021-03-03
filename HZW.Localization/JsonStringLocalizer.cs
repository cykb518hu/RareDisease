using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace HZW.Localization
{
    public class JsonStringLocalizer: IStringLocalizer
    {
        private readonly LocalizationInMemoryCacheProvider _cacheProvider;

        protected CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentCulture;

        public JsonStringLocalizer(LocalizationInMemoryCacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedString(name, value);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = GetString(name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            return this;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            return null;
        }

        private string GetString(string name)
        {
            var value = _cacheProvider.GetValueByLanguageCultureNameAndResourceKey(CurrentCulture.Name, name);
            return value;
        }
    }
}
