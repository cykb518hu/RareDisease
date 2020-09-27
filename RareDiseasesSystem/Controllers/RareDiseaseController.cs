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
        private readonly IRdrDataRepository _rdrDataRepository;
        public RareDiseaseController(ILocalMemoryCache localMemoryCache, ILogger<RareDiseaseController> logger,ILogRepository logRepository, IRdrDataRepository rdrDataRepository)
        {
            _localMemoryCache = localMemoryCache;
            _logger = logger;
            _logRepository = logRepository;
            _rdrDataRepository = rdrDataRepository;
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

                var globalList = _rdrDataRepository.SearchRareDiseaseList(search);
                var chinaList = _localMemoryCache.GetChinaRareDiseaseList(search);
                globalList.AddRange(chinaList);

                int count = globalList.Count;
                var data = globalList.Skip((pageIndex - 1) * 10).Take(pageSize).ToList();
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