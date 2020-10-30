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
using System.Threading.Tasks;

namespace RareDisease.Data.Repository
{
    public interface INLPSystemRepository
    {
        List<HPODataModel> GetPatientHPOResult(string nlpEngine, string patientEMRDetail, string patientVisitIds);
        List<NlpRareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine);
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
                                data.Editable = true;
                                data.IndexList = new List<HPOMatchIndexModel>();
                                data.IndexList.Add(new HPOMatchIndexModel { StartIndex = data.StartIndex, EndIndex = data.EndIndex });
                                var item = subList.FirstOrDefault(x => x.HPOId == data.HPOId && x.Name == data.Name);
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
                    hpoList.AddRange(_rdrDataRepository.GetPatientNlpResult(patientVisitIds));
                }
            }
            return hpoList;
        }

  
        public List<NlpRareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine)
        {
            var rareDiseaseList = new List<NlpRareDiseaseResponseModel>();
            if (_config.GetValue<bool>("NLPAddress:DiseaseApiEnable"))
            {
                var requestData = new RareDiseaseEngineRequestModel();
                requestData.AnalyzeEngine = rareAnalyzeEngine;
                requestData.DataBase = rareDataBaseEngine;
                var hpoStr = string.Empty;
                hpoStr = string.Join(",", hpoList.Where(x => x.Certain == "阳性").Select(x => x.HPOId).ToList());
                requestData.HPOList = hpoStr;

                var data = string.Empty;
                var requestStr = JsonConvert.SerializeObject(requestData);
                _logger.LogError("GetPatientRareDiseaseResult NLP request data：" + requestStr);

                var client = _clientFactory.CreateClient("DiseaseHost");
                var api = _config.GetValue<string>("NLPAddress:DiseaseApi");
                string boundary = DateTime.Now.Ticks.ToString("X");
                var formData = new MultipartFormDataContent(boundary);
                formData.Add(new StringContent(requestStr), "texts");
                var response = client.PostAsync(api, formData);
                data = response.Result.Content.ReadAsStringAsync().Result.ToString();
                _logger.LogError("GetPatientRareDiseaseResult 返回数据：" + data);

                rareDiseaseList = JsonConvert.DeserializeObject<List<NlpRareDiseaseResponseModel>>(data);
            }
            else
            {
                rareDiseaseList.Add(new NlpRareDiseaseResponseModel { Name = "口面运动障碍", Ratio = 0.5123213, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });

                rareDiseaseList.Add(new NlpRareDiseaseResponseModel { Name = "帕金森病（青年型、早发型）", Ratio = 0.8, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                rareDiseaseList[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001644", HpoName = "痴呆", Match = 1 });
                rareDiseaseList[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001345", HpoName = "行动不便", Match = 0 });


            }
            foreach (var x in rareDiseaseList)
            {
                x.Ratio = Math.Round(x.Ratio, 4);
                x.HPOMatchedList = x.HPOMatchedList.OrderByDescending(y => y.Match).ToList();
            }
            rareDiseaseList = rareDiseaseList.OrderByDescending(x => x.Ratio).ToList();
            
            return rareDiseaseList;
        }

    }
}
