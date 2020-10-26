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
        [EnableCors("_any")]
        public JsonResult GetRareDiseaseByEMR(string emrDetail, string nlpEngine,string analyzeEngine,string database,string appName)
        {
            try
            {
                var request = new RareDiseaseRequestModel();
                request.EMRDetail = emrDetail;
                request.NlpEngine = nlpEngine;
                request.RareAnalyzeEngine = analyzeEngine;
                request.RareDataBaseEngine = database;
                request.AppName = appName;
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                request.IPAddress = ipAddress.ToString();
                var hpoList = _nLPSystemRepository.GetPatientHPOResult(request.NlpEngine, request.EMRDetail, "");
                var rareDiseaseList = _nLPSystemRepository.GetPatientRareDiseaseResult(hpoList, request.RareAnalyzeEngine, request.RareDataBaseEngine);
                _logRepository.Add(appName+ "：调用接口 GetRareDiseaseByEMR", "API", JsonConvert.SerializeObject(request) + " " + JsonConvert.SerializeObject(rareDiseaseList));

                return Json(new { result = "ok", Response= rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("API GetRareDiseaseByEMR：" + ex.ToString());
                return Json(new { result = "ok", Response = new { errorCode = "500", errorText =ex.ToString()} });
            }
        }
        [EnableCors("_any")]
        public JsonResult GetRareDiseaseByNumber(string number, string numberType, string nlpEngine, string analyzeEngine, string database, string appName)
        {
            try
            {
                var request = new RareDiseaseRequestModel();
                request.Number = number;
                request.NumberType = numberType;
                request.NlpEngine = nlpEngine;
                request.RareAnalyzeEngine = analyzeEngine;
                request.RareDataBaseEngine = database;
                request.AppName = appName;
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
                _logRepository.Add(appName + "：调用接口 GetRareDiseaseByEMR", "API", JsonConvert.SerializeObject(request)+ " " + JsonConvert.SerializeObject(rareDiseaseList));

                return Json(new { result = "ok", Response = rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("API GetRareDiseaseByNumber：" + ex.ToString());
                return Json(new { result = "ok", Response = new { errorCode = "500", errorText = ex.ToString() } });
            }
        }

    }
}
