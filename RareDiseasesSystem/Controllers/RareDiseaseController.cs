using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{
    [Authorize]
    public class RareDiseaseController : Controller
    {
        private readonly ILogger<RareDiseaseController> _logger;
        private ILocalMemoryCache _localMemoryCache;
        private readonly ILogRepository _logRepository;
        public RareDiseaseController(ILocalMemoryCache localMemoryCache, ILogger<RareDiseaseController> logger,ILogRepository logRepository)
        {
            _localMemoryCache = localMemoryCache;
            _logger = logger;
            _logRepository = logRepository;
        }
        public IActionResult Search()
        {
            return View();
        }

        public JsonResult SearchList(string search = "", int pageIndex = 1,int pageSize = int.MaxValue)
        {
            try
            {
                _logRepository.Add("查询罕见病详情", "", search);
                var data = new List<ChinaRareDiseaseModel>();
                int count = 0;

                var list = _localMemoryCache.GetChinaRareDiseaseList();
                data = list.Where(x => x.Name.ToLower().Contains(search.ToLower())).ToList();
                count = data.Count;
                data = data.Skip((pageIndex - 1) * 10).Take(pageSize).ToList();


                return Json(new { success = true, data, total = count });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病查询错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

    }
}