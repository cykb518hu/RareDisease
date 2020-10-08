using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            _logRepository.Add("查看罕见病系统首页");
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
        public JsonResult SearchPatientData(string number = "")
        {
            try
            {
                _logRepository.Add("查询患者就诊记录");
                var patientOverview = _rdrDataRepository.GetPatientOverview(number);
                var patientVisitList = _rdrDataRepository.GetPatientVisitList(number);
                return Json(new { success = true, patientOverview, patientVisitList, total = patientVisitList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询患者就诊记录：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetPatientEMRDetail(string patientEmpiId = "")
        {
            try
            {
                var data = _rdrDataRepository.GetPatientEMRDetail(patientEmpiId);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError("获取患者电子病历文本：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetPatientHPOResult(string nlpEngine, string patientEMRDetail = "", string patientEmpiId = "")
        {
            try
            {
                var hpoList = new List<HPODataModel>();
                hpoList = _nLPSystemRepository.GetPatientHPOResult(nlpEngine, patientEMRDetail, patientEmpiId);             
                _logRepository.Add("获取病人HPO", "", JsonConvert.SerializeObject(hpoList));
                return Json(new { success = true, data = hpoList, });
            }
            catch (Exception ex)
            {
                _logger.LogError("获取病人HPO：" + ex.ToString());
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

        public JsonResult GetPatientRareDiseaseResult(string rareAnalyzeEngine, string rareDataBaseEngine,List<HPODataModel> hpoList = null)
        {
            try
            {
                var rareDiseaseList = new List<RareDiseaseResponseModel>();
                rareDiseaseList = _nLPSystemRepository.GetPatientRareDiseaseResult(hpoList, rareAnalyzeEngine, rareDataBaseEngine);
          
                _logRepository.Add("罕见病分析结果", "", JsonConvert.SerializeObject(rareDiseaseList));
                var normalDiseaseList = new List<DiseaseModel>();
                return Json(new { success = true, normalDiseaseList, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病分析结果：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }
    }
}
