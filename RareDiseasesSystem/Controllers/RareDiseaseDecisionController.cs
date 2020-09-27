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
    public class RareDiseaseDecisionController : Controller
    {
        private readonly ILogger<RareDiseaseDecisionController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly INLPSystemRepository _nLPSystemRepository;
        public RareDiseaseDecisionController(ILogger<RareDiseaseDecisionController> logger, ILogRepository logRepository, INLPSystemRepository nLPSystemRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
            _nLPSystemRepository = nLPSystemRepository;
        }
        [HttpPost]
        [EnableCors("any")]
        public JsonResult PostEMR([FromBody] RareDiseaseRequestModel request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                request.IPAddress = ipAddress.ToString();
                var hpoList = _nLPSystemRepository.AnalyzePatientHPO(request.NlpEngine, request.EMRDetail, "");
                var rareDiseaseList = _nLPSystemRepository.GetDiseaseListByHPO(hpoList, request.RareAnalyzeEngine, request.RareDataBaseEngine);
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
        [EnableCors("any")]
        public JsonResult PostNumber([FromBody] RareDiseaseRequestModel request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                request.IPAddress = ipAddress.ToString();
                var hpoList = _nLPSystemRepository.AnalyzePatientHPO(request.NlpEngine, "", request.Number);
                var rareDiseaseList = _nLPSystemRepository.GetDiseaseListByHPO(hpoList, request.RareAnalyzeEngine, request.RareDataBaseEngine);
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
