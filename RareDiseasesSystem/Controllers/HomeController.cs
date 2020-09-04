using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        public HomeController(ILocalMemoryCache localMemoryCache, ILogger<HomeController> logger, ILogRepository logRepository)
        {
            _localMemoryCache = localMemoryCache;
            _logger = logger;
            _logRepository = logRepository;
        }
        public IActionResult Index()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    
        public JsonResult SearchPatientData(string patientCardNo = "")
        {
            try
            {
                var patientOverview = new List<PatientOverviewModel>();
                var patientVisitList = new List<PatientVisitInfoModel>();          
                if (!string.IsNullOrEmpty(patientCardNo))
                {
                    patientOverview.Add(new PatientOverviewModel { EMPINumber = "12312312", Address = "sdfsdfds", CardNo = "123333333333", Gender = "男", Name = "xiaowu", PhoneNumber = "123213123" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院1", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院2", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院3", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院4", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院5", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院6", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院7", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院8", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院9", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院10", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院11", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院12", Center = "华西医院", DiagDesc = "ok" });
                    patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院13", Center = "华西医院", DiagDesc = "ok" });
                }

                return Json(new { success = true, patientOverview, patientVisitList, total = patientVisitList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询病人病历错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult ConvertPatientEMRtoText(string patientCardNo = "")
        {
            try
            {
                _logRepository.Add("提取电子病历文本");
                return Json(new { success = true, data="我们再测试，不知道结果怎么，这个可能需要很多行数据，多来一点试试看"});
            }
            catch (Exception ex)
            {
                _logger.LogError("提取电子病历文本错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult AnalyzePatientEMRRetreiveHPO(string patientEMRDetail = "")
        {
            try
            {
                _logRepository.Add("电子病历分析");
                var hpoList = new List<HPODataModel>();
                hpoList.Add(new HPODataModel { Name = "心脏病", Editable = true });
                hpoList.Add(new HPODataModel { Name = "肺病", Editable = true });
                hpoList.Add(new HPODataModel { Name = "肝病", Editable = true });
                return Json(new { success = true, data = hpoList, });
            }
            catch (Exception ex)
            {
                _logger.LogError("分析电子病历错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult SearchHPOList(string searchHPOText = "")
        {
            try
            {
                var searchedHPOList = new List<HPODataModel>();
                searchedHPOList.Add(new HPODataModel { Name = "心脏病1", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肺病2", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肝病3", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "心脏病4", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肺病5", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肝病6", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "心脏病7", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肺病8", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肝病9", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "心脏病11", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肺病12", Editable = false });
                searchedHPOList.Add(new HPODataModel { Name = "肝病13", Editable = false });
                return Json(new { success = true, data=searchedHPOList, total = searchedHPOList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError("查询HPO错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult SubmitHPODataForAnalyze(List<HPODataModel> hpoList = null)
        {
            try
            {
                _logRepository.Add("罕见病分析");
                var normalDiseaseList = new List<DiseaseModel>();
                normalDiseaseList.Add(new DiseaseModel { Name = "心脏病", Likeness = "1" });
                normalDiseaseList.Add(new DiseaseModel { Name = "心脏病2", Likeness = "0.9" });
                normalDiseaseList.Add(new DiseaseModel { Name = "心脏病3", Likeness = "0.8" });

                var rareDiseaseList = new List<DiseaseModel>();
                rareDiseaseList.Add(new DiseaseModel { Name = "罕见病1", Likeness = "1" });
                rareDiseaseList.Add(new DiseaseModel { Name = "罕见病2", Likeness = "0.9" });
                rareDiseaseList.Add(new DiseaseModel { Name = "罕见病3", Likeness = "0.8" });
                return Json(new { success = true, normalDiseaseList, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("分析HPO错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }
    }
}
