using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RareDiseasesSystem
{
    /// <summary>
    /// 这个是给第三方调用
    /// </summary>
    public class RareDiseaseDecisionController : Controller
    {
        private readonly ILogger<RareDiseaseDecisionController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly INLPSystemRepository _nLPSystemRepository;
        private readonly IRdrDataRepository _rdrDataRepository;
        public RareDiseaseDecisionController(ILogger<RareDiseaseDecisionController> logger, ILogRepository logRepository, INLPSystemRepository nLPSystemRepository, IRdrDataRepository rdrDataRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
            _nLPSystemRepository = nLPSystemRepository;
            _rdrDataRepository = rdrDataRepository;
        }
        [HttpPost]
        [EnableCors("_any")]
        public JsonResult PostEMR([FromBody] RareDiseaseRequestModel request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                request.IPAddress = ipAddress.ToString();
                var hpoList = _nLPSystemRepository.GetPatientHPOResult(request.NlpEngine, request.EMRDetail, "");
                var rareDiseaseList = _nLPSystemRepository.GetPatientRareDiseaseResult(hpoList, request.RareAnalyzeEngine, request.RareDataBaseEngine);
                _logRepository.Add("罕见病分析结果:", "API", JsonConvert.SerializeObject(request) + " " + JsonConvert.SerializeObject(rareDiseaseList));

                return Json(new { success = true, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("API 罕见病分析：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }
        [HttpPost]
        [EnableCors("_any")]
        public JsonResult PostNumber([FromBody] RareDiseaseRequestModel request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                request.IPAddress = ipAddress.ToString();
                if(request.NumberType== "card")
                {
                    var patientOverview = _rdrDataRepository.GetPatientOverview(request.Number, request.NumberType);
                    if (patientOverview.Any())
                    {
                        request.Number = patientOverview.FirstOrDefault().EMPINumber;
                    }
                }
                var patientVisitList = _rdrDataRepository.GetPatientVisitList(request.Number);
                var visitIdList = patientVisitList.Select(x => x.VisitId).ToList();
                var patientVisitIds = string.Join(",", visitIdList);
                var hpoList = _nLPSystemRepository.GetPatientHPOResult(request.NlpEngine, "", patientVisitIds);
                var rareDiseaseList = _nLPSystemRepository.GetPatientRareDiseaseResult(hpoList, request.RareAnalyzeEngine, request.RareDataBaseEngine);
                _logRepository.Add("罕见病分析结果:", "API", JsonConvert.SerializeObject(request)+ " " + JsonConvert.SerializeObject(rareDiseaseList));

                return Json(new { success = true, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("API 罕见病分析：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

    }
}
