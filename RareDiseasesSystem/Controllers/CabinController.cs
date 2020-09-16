using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data.Model;
using RareDisease.Data.Model.Cabin;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{
    public class CabinController : Controller
    {

        private readonly ILogger<CabinController> _logger;
        private readonly ILogRepository _logRepository;
        public CabinController(ILogger<CabinController> logger, ILogRepository logRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
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
                var results = new List<OverViewModel>
                {
                    new OverViewModel
                    {
                         Title = "罕见病总数（个）",
                        Result ="137"
                    },
                     new OverViewModel
                    {
                         Title = "罕见病总数（个）",
                        Result ="2000000"
                    },

                };
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
                var entity = new DiseasePatientTimeLineDistributionModel
                {
                    LegendData = new List<string>( ),
                    xAxisData = new List<string>(),
                    LineData = new Dictionary<string, List<double>>()
                };
                entity.LegendData.Add("男");
                entity.LegendData.Add("女");

                entity.xAxisData.Add("2011");
                entity.xAxisData.Add("2012");
                entity.xAxisData.Add("2013");
                entity.xAxisData.Add("2014");
                entity.xAxisData.Add("2015");
                entity.xAxisData.Add("2016");
                entity.xAxisData.Add("2017");
                entity.xAxisData.Add("2018");
                entity.xAxisData.Add("2019");
                entity.xAxisData.Add("2020");
                entity.LineData.Add("男", new List<double> { 5159, 7332, 15234, 6969, 200, 13757, 2977, 5573, 11587, 12876, 6224 });
                entity.LineData.Add("女", new List<double> { 8068, 8561, 329, 18632, 10545, 15721, 3938, 17489, 10992, 19602, 9352 });

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
                var ages = new[] { 65, 70, 75, 80, 85 };
                var results = new List<SeriesDataModel>();
                results.Add(new SeriesDataModel { Name = "65-69岁", Value = 100 });
                results.Add(new SeriesDataModel { Name = "70-74岁", Value = 200 });
                results.Add(new SeriesDataModel { Name = "75-79岁", Value = 300 });
                results.Add(new SeriesDataModel { Name = "80-85岁", Value = 150 });
                results.Add(new SeriesDataModel { Name = "> 85岁", Value = 5000 });
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
                var results = new List<SeriesDataModel>
                {
                    new SeriesDataModel
                    {
                        Name = "男",
                        Value =200
                    },
                    new SeriesDataModel
                    {
                        Name = "女",
                        Value = 250
                    }
                };
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
                var entity = new BarChartModel
                {
                    AxisData = new List<string> { "高血压", "恶性肿瘤", "糖尿病", "冠心病", "肺部感染", "老年性白内障", "肾囊肿", "前列腺肥大", "肝囊肿" },
                    SeriesData = new List<double> { 401920, 386875, 247180, 190110, 150890, 143875, 120735, 116945, 83090 }
                };

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
                var results = new List<SeriesDataModel>
                {
                    new SeriesDataModel
                    {
                        Name = "男",
                        Value =200
                    },
                    new SeriesDataModel
                    {
                        Name = "女",
                        Value = 250
                    }
                };
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