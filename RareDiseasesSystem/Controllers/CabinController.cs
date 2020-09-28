using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Model;
using RareDisease.Data.Model.Cabin;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{
    [Authorize]
    public class CabinController : Controller
    {

        private readonly ILogger<CabinController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly ILocalMemoryCache _localMemoryCache;
        public CabinController(ILogger<CabinController> logger, ILogRepository logRepository, ILocalMemoryCache localMemoryCache)
        {
            _logger = logger;
            _logRepository = logRepository;
            _localMemoryCache = localMemoryCache;
        }
        public IActionResult Index()
        {
            _logRepository.Add("查看罕见病驾驶舱");
            return View();
        }

        /// <summary>
        /// 磁贴 
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRareDiseaseOverView()
        {
            try
            {
                var results = _localMemoryCache.GetCabinOverView();
                return new JsonResult(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询磁贴数据错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }


        /// <summary>
        /// 罕见病时间趋势图
        /// </summary>
        /// <returns></returns>
        public IActionResult GetDiseaseTimeLineDistribution()
        {
            try
            {
                var data = _localMemoryCache.GetCabinPatientGenderTimeLine();
                var entity = new PatientTimeLineDistributionModel
                {
                    LegendData = new List<string>( ),
                    xAxisData = new List<string>(),
                    LineData = new Dictionary<string, List<int>>()
                };
                entity.xAxisData = data.Select(x => x.Year).ToList();
                entity.LegendData.Add("男");
                entity.LegendData.Add("女");

                entity.xAxisData = data.Select(x => x.Year).ToList();
                entity.LineData.Add("男", data.Select(x=>x.Male).ToList());
                entity.LineData.Add("女", data.Select(x => x.Female).ToList());

                return new JsonResult(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病时间趋势图错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// 病种患者分布 - 患者年龄段占比
        /// </summary>
        /// <returns></returns>
        public IActionResult GetDiseasePatientAge()
        {
            try
            {
                var results = _localMemoryCache.GetCabinPatientAge(); 
                var entity = new PieChartModel
                {
                    legendData = results.Select(x => x.Name).ToList(),
                    seriesData = results
                };
                return new JsonResult(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError("患者年龄段占比错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// 病种患者分布 - 患者性别占比
        /// </summary>
        /// <returns></returns>
        public IActionResult GetDiseasePatientGender()
        {
            try
            {
                var results = _localMemoryCache.GetCabinGenderOverView();
                var entity = new PieChartModel
                {
                    legendData = results.Select(x => x.Name).ToList(),
                    seriesData = results
                };

                return new JsonResult(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError("患者性别占比错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// 罕见病据量排名
        /// </summary>
        /// <returns></returns>
        public IActionResult GetDiseasePatientRank()
        {
            try
            {
                var data = _localMemoryCache.GetCabinDiseaseRank();
                var entity = new BarChartModel();
                entity.AxisData = data.Select(x => x.Name).ToList();
                entity.SeriesData = data.Select(x => x.Value).ToList();

                return new JsonResult(new { success = true, data = entity });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病据量排名错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }



        /// <summary>
        /// 病种患者地域分布
        /// </summary>
        /// <returns></returns>
        public IActionResult GetPatientAreaDistribution()
        {
            try
            {
                var results = _localMemoryCache.GetCabinPatientArea();
                return new JsonResult(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError("病种患者地域分布错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

    }
}