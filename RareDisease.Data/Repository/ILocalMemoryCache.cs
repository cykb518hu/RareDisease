using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using RareDisease.Data.Model.Cabin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface ILocalMemoryCache
    {
        List<RareDiseaseDetailModel> GetChinaRareDiseaseList(string search);
        List<LoginModel> GetUserList();

        List<OverViewModel> GetCabinOverView();

        List<SeriesDataModel> GetCabinGenderOverView();

        List<SeriesDataModel> GetCabinDiseaseRank();
        List<SeriesDataModel> GetCabinPatientAge();

        List<CabinPatientGenderTimeLine> GetCabinPatientGenderTimeLine();
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

        public List<RareDiseaseDetailModel> GetChinaRareDiseaseList(string search)
        {
            var result = GetList<List<RareDiseaseDetailModel>>("ChinaRareDiseaseList", "//App_Data//ChinaRareDiseases.json");

            if(!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                result = result.Where(x => x.Name.Trim().ToLower().Contains(search) || x.NameEnglish.Trim().ToLower().Contains(search)).ToList();
                result.ForEach(x => { x.Source = "2019名录"; x.Editable = true; });
            }
            return result;
        }
   
        public List<LoginModel> GetUserList()
        {
            var result = GetList<List<LoginModel>>("hjbUserList", "//App_Data//UserList.json");
            return result;
        }
        public List<OverViewModel> GetCabinOverView()
        {
            var result = GetList<List<OverViewModel>>("cabinOverView", "//App_Data//CabinOverView.json");
            return result;
        }

        public List<SeriesDataModel> GetCabinGenderOverView()
        {
            var result = GetList<List<SeriesDataModel>>("CabinGenderOverView", "//App_Data//CabinGenderOverView.json");
            return result;
        }
        public List<SeriesDataModel> GetCabinDiseaseRank()
        {
            var result = GetList<List<SeriesDataModel>>("CabinDiseaseRank", "//App_Data//CabinDiseaseRank.json");
            return result;
        }
        public List<SeriesDataModel> GetCabinPatientAge()
        {
            var result = GetList<List<SeriesDataModel>>("CabinPatientAge", "//App_Data//CabinPatientAge.json");
            return result;
        }

        public List<CabinPatientGenderTimeLine> GetCabinPatientGenderTimeLine()
        {
            var result = GetList<List<CabinPatientGenderTimeLine>>("CabinPatientGenderTimeLine", "//App_Data//CabinPatientGenderTimeLine.json");
            return result;
        }

        public T GetList<T>(string key, string filePath)
        {
            T result = default(T);
            var data = _cache.Get(key);
            if (data == null)
            {
                var path = _hostingEnvironment.ContentRootPath + filePath;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册简体中文的支持
                var userListStr = File.ReadAllText(path, Encoding.GetEncoding("gb2312"));
                if (!string.IsNullOrEmpty(userListStr))
                {
                    result = JsonConvert.DeserializeObject<T>(userListStr);
                    _cache.Set("key", result);
                }
            }
            else
            {
                result = (T)(data);
            }
            return result;
        }
    }
}
