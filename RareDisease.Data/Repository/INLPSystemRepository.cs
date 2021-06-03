using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RareDisease.Data.Repository
{
    public interface INLPSystemRepository
    {
        List<HPODataModel> GetPatientHPOResult(string nlpEngine, string patientEMRDetail, string patientVisitIds);
        List<NlpRareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine);
        Task<List<NlpRareDiseaseResponseModel>> GetRareDiseaseResult(string hpoStr, string rareAnalyzeEngine, string rareDataBaseEngine);

        List<HPODataModel> GetNlpHPOResultBatch(string patientEMRDetail);
    }

    public class NLPSystemRepository : INLPSystemRepository
    {
        private readonly IHostingEnvironment _env;

        private readonly IRdrDataRepository _rdrDataRepository;
        private readonly ILocalMemoryCache _localMemoryCache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<NLPSystemRepository> _logger;

        public NLPSystemRepository(IHostingEnvironment env, IRdrDataRepository rdrDataRepository, ILocalMemoryCache localMemoryCache, IHttpClientFactory clientFactory, IConfiguration config,ILogger<NLPSystemRepository> logger)
        {
            _env = env;
            _rdrDataRepository = rdrDataRepository;
            _localMemoryCache = localMemoryCache;
            _clientFactory = clientFactory;
            _config = config;
            _logger = logger;
        }
        /// <summary>
        /// patientEmpiId can be empiid or cardNo
        /// </summary>
        /// <param name="nlpEngine"></param>
        /// <param name="patientEMRDetail"></param>
        /// <param name="patientEmpiId"></param>
        /// <returns></returns>
        public List<HPODataModel> GetPatientHPOResult(string nlpEngine, string patientEMRDetail, string patientVisitIds)
        {
            var hpoList = new List<HPODataModel>();
            //首先获取检验数据HPO
            if (!string.IsNullOrWhiteSpace(patientVisitIds))
            {
                hpoList.AddRange(_rdrDataRepository.GetPatientExamDataResult(patientVisitIds));
            }
            if (!string.IsNullOrWhiteSpace(patientEMRDetail))
            {
                //如果API 启用，就调用API 否则取数据库跑出来的结果
                if (_config.GetValue<bool>("NLPAddress:HPOApiEnable"))
                {
                    try
                    {
                        var clientName = "HPOStringMatchHost";
                        var api = _config.GetValue<string>("NLPAddress:HPOStringMatchApi");
                        if (nlpEngine.Equals("Spacy"))
                        {
                            clientName = "HPOSpacyMatchHost";
                            api = _config.GetValue<string>("NLPAddress:HPOSpacyMatchApi");
                        }                   
                        var client = _clientFactory.CreateClient(clientName);
                        string boundary = DateTime.Now.Ticks.ToString("X");
                        var formData = new MultipartFormDataContent(boundary);

                        patientEMRDetail = "{\"text\":\"" + patientEMRDetail + "\"}";
                        _logger.LogError($"GetPatientHPOResult request，HPO engine:{nlpEngine},api:{api} 请求数据：" + patientEMRDetail);

                        formData.Add(new StringContent(patientEMRDetail), "texts");
                        var response = client.PostAsync(api, formData);
                        var result = response.Result.Content.ReadAsStringAsync().Result.ToString();

                        _logger.LogError($"GetPatientHPOResult result，HPO engine:{nlpEngine},api:{api} 返回数据：" + result);

                        var hpoEngineList = JsonConvert.DeserializeObject<List<HPOAPIEngineResultModel>>(result);

                        var subList = new List<HPODataModel>();
                        foreach (var r in hpoEngineList)
                        {
                            foreach (var hpo in r.Similarchpoid)
                            {
                                var data = new HPODataModel();
                                if (nlpEngine.Equals("Spacy"))
                                {
                                    data.Name = r.Emrword + ":" + r.Similarchpoterm;
                                }
                                else
                                {
                                    data.Name = r.Emrword;
                                }
                                    
                                data.Positivie = r.Positivie;
                                data.StartIndex = r.Start;
                                data.EndIndex = r.End;
                                data.HPOId = hpo;
                                data.TermSource = r.TermSource == null ? "" : string.Join(",", r.TermSource);
                                data.Editable = true;
                                data.IndexList = new List<HPOMatchIndexModel>();
                                data.IndexList.Add(new HPOMatchIndexModel { StartIndex = data.StartIndex, EndIndex = data.EndIndex });
                                var item = subList.FirstOrDefault(x => x.HPOId == data.HPOId && x.Name == data.Name && x.Positivie == data.Positivie);
                                if (item != null)
                                {
                                    item.IndexList.AddRange(data.IndexList);
                                }
                                else
                                {
                                    subList.Add(data);
                                }
                            }
                        }
                        var chpoList = from T1 in _localMemoryCache.GetCHPO2020StandardList()
                                       join T2 in subList.Select(x => x.HPOId) on T1.HpoId equals T2
                                       select new CHPO2020Model { NameChinese = T1.NameChinese, HpoId = T1.HpoId, NameEnglish = T1.NameEnglish };

                        foreach (var r in subList)
                        {
                            r.IndexList = r.IndexList.OrderBy(x => x.StartIndex).ToList();
                            var chpo2020Data = chpoList.FirstOrDefault(x => x.HpoId == r.HPOId);
                            if (chpo2020Data != null)
                            {
                                r.CHPOName = chpo2020Data.NameChinese;
                                r.NameEnglish = chpo2020Data.NameEnglish;
                            }
                        }
                        hpoList.AddRange(subList);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError("NLPSystemRepository GetPatientHPOResult报错：" + ex.ToString());
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(patientVisitIds))
                    {

                        hpoList.Add(new HPODataModel { Name = "口咽部吞咽困难", NameEnglish = "Oral-pharyngeal dysphagia", HPOId = "HP:0200136", Editable = true, CHPOName = "吞咽困难", TermSource = "umls" });
                        hpoList[0].IndexList = new List<HPOMatchIndexModel>();
                        hpoList[0].IndexList.Add(new HPOMatchIndexModel { StartIndex = 315, EndIndex = 319 });
                    }
                    hpoList.Add(new HPODataModel { Name = "僵硬", NameEnglish = "Rigors", HPOId = "HP:0025145",  Editable = true, CHPOName= "运动迟缓",TermSource="hpo,omaha"});

                    hpoList[1].IndexList = new List<HPOMatchIndexModel>();
                    hpoList[1].IndexList.Add(new HPOMatchIndexModel { StartIndex = 23, EndIndex = 25 });
                    hpoList[1].IndexList.Add(new HPOMatchIndexModel { StartIndex = 113, EndIndex = 115 });
                    hpoList[1].IndexList.Add(new HPOMatchIndexModel { StartIndex = 722, EndIndex = 724 });

                    hpoList.Add(new HPODataModel { Name = "抖动", NameEnglish = "Tremor", HPOId = "HP:0001337",  Editable = true, CHPOName = "震颤",TermSource="umls" });
                    hpoList[2].IndexList = new List<HPOMatchIndexModel>();
                    hpoList[2].IndexList.Add(new HPOMatchIndexModel { StartIndex = 20, EndIndex = 22 });

                    hpoList.Add(new HPODataModel { Name = "运动迟缓", NameEnglish = "Bradykinesia", HPOId = "HP:0002067",  Editable = true,TermSource= "umls,hpo,omaha",CHPOName= "运动迟缓" });
                    hpoList[3].IndexList = new List<HPOMatchIndexModel>();
                    hpoList[3].IndexList.Add(new HPOMatchIndexModel { StartIndex = 160, EndIndex = 163 });

                    hpoList.Add(new HPODataModel { Name = "构音障碍", NameEnglish = "Dysarthria", HPOId = "HP:0001260", Editable = true, Positivie = 0, TermSource = "umls,hpo,omaha", CHPOName = "构音障碍" });
                    hpoList[4].IndexList = new List<HPOMatchIndexModel>();
                    hpoList[4].IndexList.Add(new HPOMatchIndexModel { StartIndex = 334, EndIndex = 338 });

                    hpoList.Add(new HPODataModel { Name = "睡眠差", NameEnglish = "Sleep disturbance", HPOId = "HP:0002360", Editable = true, Positivie = 0, TermSource = "umls,hpo,omaha", CHPOName = "睡眠障碍" });
                    hpoList[5].IndexList = new List<HPOMatchIndexModel>();
                    hpoList[5].IndexList.Add(new HPOMatchIndexModel { StartIndex = 300, EndIndex = 303 });
                }
            }
            return hpoList;
        }


        public List<NlpRareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine)
        {
            var hpoSubList = hpoList.Where(x => x.Certain == "阳性").Select(x => x.HPOId).ToList();
            hpoSubList = hpoSubList.Where((x, i) => hpoSubList.FindIndex(z => z == x) == i).ToList();
            var hpoStr = string.Join(",", hpoSubList);
            var rareDiseaseList = GetRareDiseaseResult(hpoStr, rareAnalyzeEngine, rareDataBaseEngine).Result;
            return rareDiseaseList;
        }

        public async Task<List<NlpRareDiseaseResponseModel>> GetRareDiseaseResult(string hpoStr, string rareAnalyzeEngine, string rareDataBaseEngine)
        {
            var rareDiseaseList = new List<NlpRareDiseaseResponseModel>();
            if (_config.GetValue<bool>("NLPAddress:DiseaseApiEnable"))
            {
                var requestData = new RareDiseaseEngineRequestModel();
                requestData.AnalyzeEngine = rareAnalyzeEngine;
                requestData.DataBase = rareDataBaseEngine;
                requestData.HPOList = hpoStr;
                var data = string.Empty;
                var requestStr = JsonConvert.SerializeObject(requestData);
                _logger.LogError("GetPatientRareDiseaseResult 请求数据：" + requestStr);

                var client = _clientFactory.CreateClient("DiseaseHost");
                var api = _config.GetValue<string>("NLPAddress:DiseaseApi");
                string boundary = DateTime.Now.Ticks.ToString("X");
                var formData = new MultipartFormDataContent(boundary);
                formData.Add(new StringContent(requestStr), "texts");
                var response = client.PostAsync(api, formData);
                data = await response.Result.Content.ReadAsStringAsync();
                _logger.LogError("GetPatientRareDiseaseResult 返回数据：" + data);

                rareDiseaseList = JsonConvert.DeserializeObject<List<NlpRareDiseaseResponseModel>>(data);
            }
            else
            {
                #region
                Thread.Sleep(2000);

                var data = new NlpRareDiseaseResponseModel
                {
                    Name = "帕金森病 -" + rareAnalyzeEngine,
                    Ratio = (double)rareAnalyzeEngine.Length/10,
                    HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>()
                };
                data.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0025145", HpoName = "僵硬", Match = 1 });
                data.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0001337", HpoName = "抖动", Match = 1 });
                data.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0001332", HpoName = "肌张力障碍", Match = 0 });
                data.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0000007", HpoName = "常染色体隐性遗传", Match = 0 });
                rareDiseaseList.Add(data);

                var data1 = new NlpRareDiseaseResponseModel
                {
                    Name = "老年痴呆 -" + rareAnalyzeEngine,
                    Ratio = (double)rareAnalyzeEngine.Length / 20,
                    HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>()
                };
                data1.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0025145", HpoName = "运动迟缓", Match = 1 });
                data1.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0002362", HpoName = "曳行步态", Match = 0 });
                data1.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0001332", HpoName = "肌张力障碍", Match = 0 });
                data1.HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:0002172", HpoName = "姿势不稳", Match = 0 });
                rareDiseaseList.Add(data1);
                #endregion

            }
            foreach (var x in rareDiseaseList)
            {
                x.Ratio = Math.Round(x.Ratio, 4);
                x.HPOMatchedList.ForEach(y => y.Source = y.Match == 1 ? "" : hpoStr.Contains(y.HpoId) ? "当前病例" : "知识库");
                x.HPOMatchedList = x.HPOMatchedList.OrderByDescending(y => y.Match).ThenBy(y => y.Source).ToList();
                x.Engine = rareAnalyzeEngine;
            }
            rareDiseaseList = rareDiseaseList.OrderByDescending(x => x.Ratio).ToList();

            return  rareDiseaseList;
        }

        /// <summary>
        /// for batch task
        /// </summary>
        /// <param name="nlpEngine"></param>
        /// <param name="patientEMRDetail"></param>
        /// <returns></returns>
        public List<HPODataModel> GetNlpHPOResultBatch(string patientEMRDetail)
        {
            var hpoList = new List<HPODataModel>();
            if (!string.IsNullOrWhiteSpace(patientEMRDetail))
            {
                try
                {
                    var clientName = "HPOStringMatchHost";
                    var api = _config.GetValue<string>("NLPAddress:HPOStringMatchApi");
                    var client = _clientFactory.CreateClient(clientName);
                    string boundary = DateTime.Now.Ticks.ToString("X");
                    var formData = new MultipartFormDataContent(boundary);
                    patientEMRDetail = "{\"text\":\"" + patientEMRDetail + "\"}";
                    formData.Add(new StringContent(patientEMRDetail), "texts");
                    var response = client.PostAsync(api, formData);
                    var result = response.Result.Content.ReadAsStringAsync().Result.ToString();
                    var hpoEngineList = JsonConvert.DeserializeObject<List<HPOAPIEngineResultModel>>(result);
                    foreach (var r in hpoEngineList)
                    {
                        if (r.Positivie == 0)
                        {
                            continue;
                        }
                        foreach (var hpo in r.Similarchpoid)
                        {
                            var data = new HPODataModel();
                            data.HPOId = hpo;
                            var item = hpoList.FirstOrDefault(x => x.HPOId == data.HPOId);
                            if (item == null)
                            {
                                hpoList.Add(data);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("NLPSystemRepository GetPatientHPOResult报错：" + ex.ToString());
                }
            }
            return hpoList;
        }


    }
}
