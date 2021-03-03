using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;
using RareDiseasesSystem.Models;

namespace RareDiseasesSystem.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILocalMemoryCache _localMemoryCache;
        private readonly ILogRepository _logRepository;
        private readonly IHostingEnvironment _env;
        private readonly IRdrDataRepository _rdrDataRepository;
        private readonly INLPSystemRepository _nLPSystemRepository;
        public HomeController(ILocalMemoryCache localMemoryCache, ILogger<HomeController> logger, ILogRepository logRepository,IHostingEnvironment env, IRdrDataRepository rdrDataRepository, INLPSystemRepository nLPSystemRepository)
        {
            _localMemoryCache = localMemoryCache;
            _logger = logger;
            _logRepository = logRepository;
            _env = env;
            _rdrDataRepository = rdrDataRepository;
            _nLPSystemRepository = nLPSystemRepository;
        }
        public IActionResult Index()
        {
            _logRepository.Add("罕见病系统首页");
            return View();
        }

        public IActionResult DiseaseCaculate()
        {
            _logRepository.Add("罕见病计算页面");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// number 可以是empiid 或者身份证号
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public JsonResult SearchPatientData(string number,string numberType)
        {
            try
            {
                _logRepository.Add("查询患者就诊记录", "", $"number:{number},numberType:{numberType}");
                var patientOverview = _rdrDataRepository.GetPatientOverview(number, numberType);              
                if (patientOverview.Any())
                {
                    number = patientOverview.FirstOrDefault().EMPINumber;
                }
                var patientVisitList = _rdrDataRepository.GetPatientVisitList(number);
                return Json(new { success = true, patientOverview, patientVisitList, total = patientVisitList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询患者就诊记录：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetPatientEMRDetail(string patientVisitIds)
        {
            try
            {
                _logRepository.Add("获取患者电子病历文本", "", $"patientVisitIds:{patientVisitIds}");
                var data = _rdrDataRepository.GetEmrForNLP(patientVisitIds);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError("获取患者电子病历文本：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetPatientHPOResult(string nlpEngine, string patientEMRDetail = "", string patientVisitIds = "")
        {
            try
            {
                var hpoList = new List<HPODataModel>();
                hpoList = _nLPSystemRepository.GetPatientHPOResult(nlpEngine, patientEMRDetail, patientVisitIds);
                _logRepository.Add("分析HPO");
                return Json(new { success = true, data = hpoList, });
            }
            catch (Exception ex)
            {
                _logger.LogError("分析HPO：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult SearchStandardHPOList(string searchHPOText = "")
        {
            try
            {
                _logRepository.Add("查询HPO", "", searchHPOText);
                var searchedHPOList = _rdrDataRepository.SearchStandardHPOList(searchHPOText);
                return Json(new { success = true, data = searchedHPOList, total = searchedHPOList.Count });

            }
            catch (Exception ex)
            {
                _logger.LogError("查询HPO错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult DeleteHPOTerm(string hpoId, string hpoTerm)
        {
            try
            {
                _logRepository.Add($"删除HPO Term","",$"HPO ID:{hpoId},HPO Term:{hpoTerm}");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("删除HPO Term：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }
    }
}
