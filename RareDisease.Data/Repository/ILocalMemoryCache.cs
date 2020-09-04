using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface ILocalMemoryCache
    {
        List<ChinaRareDiseaseModel> GetChinaRareDiseaseList();
    }

    public class LocalMemoryCache : ILocalMemoryCache
    {
        private IMemoryCache _cache;
        private IHostingEnvironment _hostingEnvironment;

        public LocalMemoryCache(IMemoryCache memoryCache, IHostingEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }

        public List<ChinaRareDiseaseModel> GetChinaRareDiseaseList()
        {
            var result = new List<ChinaRareDiseaseModel>();
            var data = _cache.Get("ChinaRareDiseaseList");
            if (data == null)
            {
                var path = _hostingEnvironment.ContentRootPath + "//App_Data//ChinaRareDiseases.json";
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册简体中文的支持
                var diseaseStr = File.ReadAllText(path, Encoding.GetEncoding("gb2312"));
                if(!string.IsNullOrEmpty(diseaseStr))
                {
                    result = JsonConvert.DeserializeObject<List<ChinaRareDiseaseModel>>(diseaseStr);
                    _cache.Set("ChinaRareDiseaseList", result);
                }           
            }
            else
            {
                result = (List<ChinaRareDiseaseModel>)(data);
            }
            return result;
        }
    }
}
