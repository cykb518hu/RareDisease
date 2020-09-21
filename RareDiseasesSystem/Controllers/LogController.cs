using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{
    [Authorize]
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

        public JsonResult SearchLogList(List<string> logDateRange,int pageIndex = 1, int pageSize = int.MaxValue)
        {
            try
            {
                var total = 0;
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30);
                if (logDateRange != null && logDateRange.Count > 1)
                {
                    DateTime.TryParse(logDateRange[0].Substring(0, 24), out startDate);
                    DateTime.TryParse(logDateRange[1].Substring(0, 24), out endDate);
                }
                var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
                var userName = HttpContext.User.Identity.Name;
                var data = _logRepository.Search(pageIndex, pageSize, startDate, endDate, role, userName, ref total);
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