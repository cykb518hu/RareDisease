using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
        public JsonResult SearchPatientData(string number,string numberType)
        {
            try
            {
                _logRepository.Add("查询患者就诊记录");
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
                var data = _rdrDataRepository.GetPatientEMRDetail(patientVisitIds);
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
                var rareDiseaseList = new List<NlpRareDiseaseResponseModel>();
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

        public string GetNLPRareDiseaseResultMockUp(string texts)
        {
            try
            {
                var item = new List<NlpRareDiseaseResponseModel>();
                item.Add(new NlpRareDiseaseResponseModel { Name = "anemiadlsk;jjjjjjjjjjjsdfsdfadljdslkjflkasjdlfakjsdlkjflaksjdl;fkjasldkjfsalkjdflkjads;lkfjskaljdkljdslfjlskajdfl;kjasdlkjflasdjfljadsjflsdjf;lsajljfasjdlf;jadsljfl;asdjf;ladjslfj;saj;flkdsjflkdsjjsa", Ratio = 1, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                item[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                item[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });

                item.Add(new NlpRareDiseaseResponseModel { Name = "anemiadlsk;jjjjjjjjjjjsdfsdfadljdslkjflkasjdlfakjsdlkjflaksjdl;fkjasldkjfsalkjdflkjads;lkfjskaljdkljdslfjlskajdfl;kjasdlkjflasdjfljadsjflsdjf;lsajljfasjdlf;jadsljfl;asdjf;ladjslfj;saj;flkdsjflkdsjjsaanemiadlsk;jjjjjjjjjjjsdfsdfadljdslkjflkasjdlfakjsdlkjflaksjdl;fkjasldkjfsalkjdflkjads;lkfjskaljdkljdslfjlskajdfl;kjasdlkjflasdjfljadsjflsdjf;lsajljfasjdlf;jadsljfl;asdjf;ladjslfj;saj;flkdsjflkdsjjsaanemiadlsk;jjjjjjjjjjjsdfsdfadljdslkjflkasjdlfakjsdlkjflaksjdl;fkjasldkjfsalkjdflkjads;lkfjskaljdkljdslfjlskajdfl;kjasdlkjflasdjfljadsjflsdjf;lsajljfasjdlf;jadsljfl;asdjf;ladjslfj;saj;flkdsjflkdsjjsa", Ratio = 0.9, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                item[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001644", HpoName = "痴呆", Match = 1 });
                item[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001345", HpoName = "行动不便", Match = 0 });
                var str = "[{'name':'帕金森','ratio':0.2,'Hpolist':[{'HpoId':'HP0001644','hpoName':'痴呆','match':0}, {'HpoId':'HP0001545','hpoName':'行动不便','match':1}]},{'name':'白化病','ratio':0.3,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                var str1 = "[{\"name\":\"anemia11\",\"ratio\":0.2,\"Hpolist\":[{\"hpoId\":\"HP0001744\",\"hpoName\":\"\\u6d88\\u5316\\u7cfb\\u7edf\\u5f62\\u6001\\u5f02\\u5e38\",\"match\":1}, {\"hpoId\":\"HP0001744\",\"hpoName\":\"\\u6d88\\u5316\\u7cfb\\u7edf\\u5f62\\u6001\\u5f02\\u5e38\",\"match\":0}]},{\"name\":\"anemia\",\"ratio\":0.1,\"Hpolist\":[{\"hpoId\":\"HP0001744\",\"hpoName\":\"\\u6d88\\u5316\\u7cfb\\u7edf\\u5f62\\u6001\\u5f02\\u5e38\",\"match\":0}, {\"hpoId\":\"HP0001744\",\"hpoName\":\"\\u6d88\\u5316\\u7cfb\\u7edf\\u5f62\\u6001\\u5f02\\u5e38\",\"match\":0}]} ]";

                return str;// JsonConvert.SerializeObject(item);
                //return str;
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病分析结果：" + ex.ToString());
                return "";
            }
        }
        public string GetNLHPOMockUp(string texts)
        {
            try
            {

                var str = "[ {'ratio': 1, 'emrword': '腹泻', 'similarchpoterm': '腹泻', 'similarchpoid': \"['hp:00101', 'hp:00102']\",'start': 0, 'end': 2 }, {'ratio': 1,'emrword': '腹泻','similarchpoterm': '腹泻CHPO','similarchpoid': \"['hp:00103','hp:00104']\",'start': 0, 'end': 2 }]";
                var str1 = "[ {'ratio': 1, 'emrword': '腹泻', 'similarchpoterm': '腹泻', 'similarchpoid': ['hp:00101', 'hp:00102'],'start': 0, 'end': 2 }, {'ratio': 1,'emrword': '腹泻','similarchpoterm': '腹泻CHPO','similarchpoid': ['hp:00103','hp:00104'],'start': 0, 'end': 2 }]";

                return str1;// JsonConvert.SerializeObject(item);
                //return str;
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病分析结果：" + ex.ToString());
                return "";
            }
        }
    }
}
