using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Handler;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RareDiseasesSystem
{
    /// <summary>
    /// 后台任务
    /// </summary>
    public class RareDiseaseBatchTaskController : Controller
    {
        private readonly ILogger<RareDiseaseBatchTaskController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly INLPSystemRepository _nLPSystemRepository;
        private readonly IRdrDataRepository _rdrDataRepository;
        private readonly IConfiguration _config;
        public RareDiseaseBatchTaskController(ILogger<RareDiseaseBatchTaskController> logger, ILogRepository logRepository, INLPSystemRepository nLPSystemRepository, IRdrDataRepository rdrDataRepository, IConfiguration config)
        {
            _logger = logger;
            _logRepository = logRepository;
            _nLPSystemRepository = nLPSystemRepository;
            _rdrDataRepository = rdrDataRepository;
            _config = config;
        }
        public JsonResult GetPatientHPOResultList()
        {
            try
            {
                var fileName = _config.GetValue<string>("GlobalSetting:BatchExcelPath");
                var visitList = BatchTaskHandler.ReadVisitExcel(fileName);
                int count = 0;
                _logger.LogError($"批量导入开始");
                foreach (var r in visitList)
                {
                    _logger.LogError($"：第{count}条记录");
                    try
                    {
                        r.VisitId = _rdrDataRepository.GetVisitIdByNumber(r.VisitNumber);
                        var patientEMRDetail = _rdrDataRepository.GetEmrForNLP(r.VisitId);
                        patientEMRDetail = HttpUtility.UrlEncode(patientEMRDetail);
                        var hpoList = _nLPSystemRepository.GetNlpHPOResultBatch(patientEMRDetail);
                        if (hpoList != null && hpoList.Any())
                        {
                            r.NlpHpoListStr = string.Join(",", hpoList.Select(x => x.HPOId).ToList());
                        }
                        var examHpo = _rdrDataRepository.GetExamHPOResultBatch(r.VisitId);
                        if (examHpo != null && examHpo.Any())
                        {
                            r.ExamHpoListStr = string.Join(",", examHpo.Select(x => x.HPOId).ToList());
                        }
                        r.Done = "yes";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"：第{count}条记录,发生错误:" + ex.ToString());
                        r.Done = "no";
                    }
                    count++;
                }
                BatchTaskHandler.UpdateVisitExcel(fileName, visitList);
                _logger.LogError($"批量导入结束");
                return Json(new { success = true, data = "成功", });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.ToString() });
            }
        }


        public JsonResult GetPatientEMRTextAll()
        {
            try
            {
                var fileName = _config.GetValue<string>("GlobalSetting:BatchExcelPath");
                var visitList = BatchTaskHandler.ReadVisitExcel(fileName);
                int count = 0;
                _logger.LogError($"批量导入开始");
                foreach (var r in visitList)
                {
                    _logger.LogError($"：第{count}条记录");
                    try
                    {
                        r.VisitId = _rdrDataRepository.GetVisitIdByNumber(r.VisitNumber);
                        r.EMR = _rdrDataRepository.GetFullEmrAll(r.VisitId);
                        r.Done = "yes";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"：第{count}条记录,发生错误:" + ex.ToString());
                        r.Done = "no";
                    }
                    count++;
                }
                BatchTaskHandler.UpdateVisitEMRExcel(fileName, visitList);
                _logger.LogError($"批量导入结束");
                return Json(new { success = true, data = "成功", });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

    }
}
