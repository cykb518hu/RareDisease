using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;

namespace RareDiseasesSystem.Controllers
{

    [Authorize]
    public class RareDiseaseController : Controller
    {
        private readonly ILogger<RareDiseaseController> _logger;
        private ILocalMemoryCache _localMemoryCache;
        private readonly ILogRepository _logRepository;
        private readonly IRdrDataRepository _rdrDataRepository;
        private readonly INLPSystemRepository _nLPSystemRepository;
        private readonly IExcelRepository _excelRepository;
        public RareDiseaseController(ILocalMemoryCache localMemoryCache, ILogger<RareDiseaseController> logger,ILogRepository logRepository, IRdrDataRepository rdrDataRepository, INLPSystemRepository nLPSystemRepository,IExcelRepository excelRepository)
        {
            _localMemoryCache = localMemoryCache;
            _logger = logger;
            _logRepository = logRepository;
            _rdrDataRepository = rdrDataRepository;
            _nLPSystemRepository = nLPSystemRepository;
            _excelRepository = excelRepository;
        }
        public IActionResult Search()
        {
            _logRepository.Add("罕见病查询页面");
            return View();
        }

        public IActionResult Summary()
        {
            _logRepository.Add("罕见病整理页面");
            return View();
        }

        public JsonResult SearchList(string search = "", int pageIndex = 1,int pageSize = int.MaxValue)
        {
            try
            {
                _logRepository.Add("罕见病查询", "", search);

                var globalList = _rdrDataRepository.SearchStandardRareDiseaseList(search);
                var chinaList = _localMemoryCache.GetChinaRareDiseaseList(search);
                globalList.AddRange(chinaList);

                int count = globalList.Count;
                var data = globalList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                return Json(new { success = true, data, total = count });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病查询错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetDiseaseNameList()
        {
            try
            {
                var data = _excelRepository.GetDiseaseNameList();
                return Json(new { success = true, data});
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDiseaseNameList：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public JsonResult GetDiseaseHPOSummaryBar(string diseaseText)
        {
            try
            {
                diseaseText = HttpUtility.UrlDecode(diseaseText);
                var data = _excelRepository.GetDiseaseHPOSummaryBar(diseaseText);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDiseaseNameList：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public async Task<JsonResult> GetPatientRareDiseaseResult(string rareAnalyzeEngine, string rareDataBaseEngine, string hpoStr)
        {
            try
            {
                var rareDiseaseList = new List<NlpRareDiseaseResponseModel>();
                rareDiseaseList = await _nLPSystemRepository.GetRareDiseaseResult(hpoStr, rareAnalyzeEngine, rareDataBaseEngine);

                _logRepository.Add("罕见病分析", "", $"分析引擎:{rareAnalyzeEngine},罕见病库:{rareDataBaseEngine}");
                return Json(new { success = true, rareDiseaseList });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病分析结果：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        public async Task<JsonResult> GetDiseaseCaculateResult(string rareAnalyzeEngine, string rareDataBaseEngine, string hpoStr)
        {
            try
            {
                _logRepository.Add("罕见病详细计算", "", $"分析引擎:{rareAnalyzeEngine},罕见病库:{rareDataBaseEngine},HPO str:{hpoStr}");
                var hpoCount = hpoStr.Split(",").Count();
                var engineList = rareAnalyzeEngine.Split(",");

                var diseaseCaluateBar = new DiseaseCaluateBarModel();
                var diseaseCaculateDistribution = new DiseaseCaluateHPODistributionModel();
                diseaseCaculateDistribution.Disease = new List<string>();
                diseaseCaculateDistribution.Disease.Add("初始HPO");
                diseaseCaculateDistribution.HPOId = new List<string>();

                diseaseCaluateBar.SeriesDataModel = new List<SeriesData>();
                diseaseCaluateBar.Disease = new List<string>();
                diseaseCaluateBar.SeriesDataModel.Add(new SeriesData { Name = "命中HPO", Value = new List<int>() });
                diseaseCaluateBar.SeriesDataModel.Add(new SeriesData { Name = "疾病HPO", Value = new List<int>() });
                diseaseCaluateBar.SeriesDataModel.Add(new SeriesData { Name = "初始HPO", Value = new List<int>() });
                diseaseCaluateBar.SeriesDataModel.Add(new SeriesData { Name = "支持算法", Value = new List<int>() });

                List<NlpRareDiseaseResponseModel> allList = new List<NlpRareDiseaseResponseModel>();

                var diseaseList = new List<DiseaseCaculateSingleModel>();
                foreach (var engine in engineList)
                {
                    var list = await _nLPSystemRepository.GetRareDiseaseResult(hpoStr, engine, rareDataBaseEngine);
                    foreach (var data in list)
                    {
                        diseaseList.Add(new DiseaseCaculateSingleModel { Source = engine, Disease = data.Name, Score = data.Ratio });
                    }
                    allList.AddRange(list);

                }
                var overviewList = new List<DiseaseCaculateOverviewModel>();
                foreach (var disease in diseaseList.GroupBy(x => x.Disease))
                {
                    var overview = new DiseaseCaculateOverviewModel();
                    overview.Disease = disease.Key;
                    overview.SupportMethod = string.Join(",", disease.ToList().Select(x => x.Source));
                    overview.Score = Math.Round(disease.Sum(x => x.Score) / disease.Count(), 4);
                    overviewList.Add(overview);
                }
                overviewList = overviewList.OrderByDescending(x => x.Score).ToList();

                for (int i = 0; i < 10 && i < overviewList.Count; i++)
                {
                    overviewList[i].Rank = i + 1;
                    diseaseCaluateBar.Disease.Add(overviewList[i].Disease);
                    diseaseCaculateDistribution.Disease.Add(overviewList[i].Disease);
                    diseaseCaluateBar.SeriesDataModel.FirstOrDefault(x => x.Name == "初始HPO").Value.Add(hpoCount);
                    diseaseCaluateBar.SeriesDataModel.FirstOrDefault(x => x.Name == "支持算法").Value.Add(overviewList[i].SupportMethod.Split(",").Count());

                    var diseaseHPOCount = 0;
                    var matchedHPOCount = 0;
                    foreach (var data in allList)
                    {
                        if (data.Name == overviewList[i].Disease)
                        {
                            diseaseHPOCount = data.HPOMatchedList.Where(x => x.Match == 1 || x.Source == "知识库").Count();
                            if (data.HPOMatchedList.Where(x => x.Match == 1).Count() > matchedHPOCount)
                            {
                                matchedHPOCount = data.HPOMatchedList.Where(x => x.Match == 1).Count();
                            }
                            foreach (var item in data.HPOMatchedList)
                            {
                                if (!diseaseCaculateDistribution.HPOId.Any(x => x == item.HpoId))
                                {
                                    diseaseCaculateDistribution.HPOId.Add(item.HpoId);
                                }
                            }
                        }
                    }
                    diseaseCaluateBar.SeriesDataModel.FirstOrDefault(x => x.Name == "疾病HPO").Value.Add(diseaseHPOCount);
                    diseaseCaluateBar.SeriesDataModel.FirstOrDefault(x => x.Name == "命中HPO").Value.Add(matchedHPOCount);
                }
                ArrayList marks = new ArrayList();
                for (int j = 0; j < diseaseCaculateDistribution.Disease.Count; j++)
                {
                    var HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>();
                    if (diseaseCaculateDistribution.Disease[j] == "初始HPO")
                    {
                        foreach(var r in hpoStr.Split(","))
                        {
                            HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = r, Match = 1 });
                        }
                    }
                    var matchedHPOCount = 0;
                    
                    foreach (var data in allList)
                    {
                        if (data.Name == diseaseCaculateDistribution.Disease[j])
                        {
                            if (data.HPOMatchedList.Where(x => x.Match == 1).Count() > matchedHPOCount)
                            {
                                matchedHPOCount = data.HPOMatchedList.Where(x => x.Match == 1).Count();
                                HPOMatchedList = data.HPOMatchedList;
                                //tempHPOStr = string.Join(",", data.HPOMatchedList.Where(x => x.Match == 1).Select(x => x.HpoId));
                            }
                        }
                    }
                    for (int i = 0; i < diseaseCaculateDistribution.HPOId.Count; i++)
                    {
                        var item = HPOMatchedList.FirstOrDefault(x => x.HpoId == diseaseCaculateDistribution.HPOId[i]);
                        if (item != null)
                        {
                            if (item.Match == 1)
                            {
                                int[] mark = new int[] { i, j, 1 };
                                marks.Add(mark);
                            }
                            else if (item.Source == "知识库")
                            {
                                int[] mark = new int[] { i, j, 0 };
                                marks.Add(mark);
                            }                        
                        }
                    }                  
                }
                diseaseCaculateDistribution.Marks = marks;
                return Json(new { success = true, overviewList, diseaseList, diseaseCaluateBar, diseaseCaculateDistribution });
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病详细计算错误：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }
        }

        [AllowAnonymous]
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
                item[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0001345", HpoName = "行动不便", Match = 0 });
                var str = "";
                if (texts.Contains("Jaccard"))
                {
                    str = "[{'name':'帕金森','ratio':0.9,'Hpolist':[{'HpoId':'HP:0002067','hpoName':'痴呆','match':0}, {'HpoId':'HP:0001545','hpoName':'行动不便','match':1},{'HpoId':'HP:0002067','hpoName':'痴呆','match':0},{'HpoId':'HP:0002068','hpoName':'痴呆','match':0},{'HpoId':'HP:0002069','hpoName':'痴呆','match':0}]},{'name':'白化病1','ratio':0.777,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP:0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                }
                if (texts.Contains("Tanimoto"))
                {
                    str = "[{'name':'帕金森','ratio':0.5,'Hpolist':[{'HpoId':'HP:0002067','hpoName':'痴呆','match':0}, {'HpoId':'HP:0001545','hpoName':'行动不便','match':1},{'HpoId':'HP:0002067','hpoName':'痴呆','match':0},{'HpoId':'HP:0002068','hpoName':'痴呆','match':0},{'HpoId':'HP:0002069','hpoName':'痴呆','match':0}]},{'name':'白化病2','ratio':0.999,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP:0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                }
                if (texts.Contains("Overlap"))
                {
                    str = "[{'name':'帕金森Overlap','ratio':0.1,'Hpolist':[{'HpoId':'HP:0002067','hpoName':'痴呆','match':0}, {'HpoId':'HP:0001545','hpoName':'行动不便','match':1},{'HpoId':'HP:0002067','hpoName':'痴呆','match':0},{'HpoId':'HP:0002068','hpoName':'痴呆','match':0},{'HpoId':'HP:0002069','hpoName':'痴呆','match':0}]},{'name':'白化病Overlap','ratio':0.01,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP:0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                }
                if (texts.Contains("Oss"))
                {
                    str = "[{'name':'帕金森Oss','ratio':0.1,'Hpolist':[{'HpoId':'HP:0002067','hpoName':'痴呆','match':0}, {'HpoId':'HP:0001545','hpoName':'行动不便','match':1},{'HpoId':'HP:0002067','hpoName':'痴呆','match':0},{'HpoId':'HP:0002068','hpoName':'痴呆','match':0},{'HpoId':'HP:0002069','hpoName':'痴呆','match':0}]},{'name':'白化病Oss','ratio':0.01,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP:0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                }
                if (texts.Contains("Loglikelihood"))
                {
                    str = "[{'name':'帕金森Loglikelihood','ratio':0.4,'Hpolist':[{'HpoId':'HP:0002067','hpoName':'痴呆','match':0}, {'HpoId':'HP:0001545','hpoName':'行动不便','match':1},{'HpoId':'HP:0002067','hpoName':'痴呆','match':0},{'HpoId':'HP:0002068','hpoName':'痴呆','match':0},{'HpoId':'HP:0002069','hpoName':'痴呆','match':0}]},{'name':'白化病Loglikelihood','ratio':0.23,'Hpolist':[{'HpoId':'HP0001344','hpoName':'流血','match':0}, {'HpoId':'HP:0001345','hpoName':'止不住','match':1},{'HpoId':'HP0001145','hpoName':'测试数据','match':1}]}]";
                }
                return str;// JsonConvert.SerializeObject(item);
                //return str;
            }
            catch (Exception ex)
            {
                _logger.LogError("罕见病分析结果：" + ex.ToString());
                return "";
            }
        }
        [AllowAnonymous]
        public string GetNLHPOMockUp(string texts)
        {
            try
            {

                var str = "[ {'ratio': 1, 'emrword': '腹泻', 'similarchpoterm': '腹泻', 'similarchpoid': \"['hp:00101', 'hp:00102']\",'start': 0, 'end': 2 }, {'ratio': 1,'emrword': '腹泻','similarchpoterm': '腹泻CHPO','similarchpoid': \"['hp:00103','hp:00104']\",'start': 0, 'end': 2 }]";
                var str1 = "[ {'positive':0,'ratio': 1, 'emrword': '腹泻', 'similarchpoterm': '腹泻', 'similarchpoid': ['HP:0002014', 'HP:0002028'],'termlaiyuan': ['umls', 'omaha'],'start': 0, 'end': 2 }, {'ratio': 1,'emrword': '腹泻','similarchpoterm': '腹泻','similarchpoid': ['hp:00101','hp:00102'],'start': 4, 'end': 6 } ,{'ratio': 1,'emrword': '食管狭窄','similarchpoterm': '食管狭窄','similarchpoid': ['HP:0002043'],'termlaiyuan': [],'start': 4, 'end': 6 }]";

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