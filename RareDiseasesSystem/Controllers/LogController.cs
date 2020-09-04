using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogger<LogController> _logger;
        private readonly ILogRepository _logRepository;
        public LogController(ILogger<LogController> logger, ILogRepository logRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult SearchLogList(int pageIndex = 1, int pageSize = int.MaxValue)
        {
            try
            {
                var total = 0;
                var data = _logRepository.Search(pageIndex, pageSize, ref total);
                return Json(new { success = true, data, total });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询操作日志错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }
    }
}