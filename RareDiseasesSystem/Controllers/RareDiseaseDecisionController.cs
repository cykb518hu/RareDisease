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
    [Route("api/[controller]")]
    public class RareDiseaseDecisionController : Controller
    {
        private readonly ILogger<RareDiseaseDecisionController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly IHostingEnvironment _env;
        public RareDiseaseDecisionController(ILogger<RareDiseaseDecisionController> logger, ILogRepository logRepository, IHostingEnvironment env)
        {
            _logger = logger;
            _logRepository = logRepository;
            _env = env;
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/<controller>
        [HttpPost]
        [EnableCors("any")]
        [Route("PostEMR")]
        public JsonResult PostEMR(string EMRDetail,string appName)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                //to do
                //API
                var rareDiseaseList = new List<DiseaseModel>();
                rareDiseaseList.Add(new DiseaseModel { Name = "常染色体显性帕金森病8型", Likeness = "1" });
                var hpoList1 = new List<HPODataModel>();
                hpoList1.Add(new HPODataModel { Name = "构音障碍", HpoId = "HP:0001260", Matched = "true" });
                hpoList1.Add(new HPODataModel { Name = "常染色体隐性遗传", HpoId = "HP:0000007", Matched = "true" });
                hpoList1.Add(new HPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "true" });
                rareDiseaseList[0].HPOMatchedList = hpoList1;

                rareDiseaseList.Add(new DiseaseModel { Name = "晚发型帕金森病", Likeness = "0.9" });
                var hpoList2 = new List<HPODataModel>();
                hpoList2.Add(new HPODataModel { Name = "震颤", HpoId = "HP:0001337", Matched = "false" });
                hpoList2.Add(new HPODataModel { Name = "帕金森症", HpoId = "HP:0001300", Matched = "true" });
                hpoList2.Add(new HPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "true" });
                hpoList2.Add(new HPODataModel { Name = "眼睑失用症", HpoId = "HP:0000658", Matched = "false" });
                rareDiseaseList[1].HPOMatchedList = hpoList2;

                rareDiseaseList.Add(new DiseaseModel { Name = "帕金森病17型", Likeness = "0.8" });

                var hpoList3 = new List<HPODataModel>();
                hpoList3.Add(new HPODataModel { Name = "震颤", HpoId = "HP:0001337", Matched = "true" });
                hpoList3.Add(new HPODataModel { Name = "常染色体隐性遗传", HpoId = "HP:0000007", Matched = "false" });
                hpoList3.Add(new HPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "false" });
                hpoList3.Add(new HPODataModel { Name = "曳行步态", HpoId = "HP:0002362", Matched = "false" });
                rareDiseaseList[2].HPOMatchedList = hpoList3;


                _logRepository.Add("罕见病分析结果:", "API", "应用端：" + appName + " 客户端ip:" + ipAddress + "  电子病历：" + EMRDetail + " " + JsonConvert.SerializeObject(rareDiseaseList));

                return Json(new { success = true, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("API 罕见病分析：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }


        // POST api/<controller>
        [HttpPost]
        [EnableCors("any")]
        [Route("PostEmpid")]
        public JsonResult PostEmpiId(string empiId, string appName)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;

              


                _logRepository.Add("罕见病分析结果:", "API", "应用端：" + appName + " 客户端ip:" + ipAddress + " EmpiId：" + empiId + " " + JsonConvert.SerializeObject(rareDiseaseList));

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
