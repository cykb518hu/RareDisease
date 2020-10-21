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
            if (!string.IsNullOrWhiteSpace(patientVisitIds))
            {
                var backgroundTasks = new[]
                {
                    Task.Run(() => _rdrDataRepository.GetPatientNlpResult(patientVisitIds)),
                    Task.Run(() =>_rdrDataRepository.GetPatientExamDataResult(patientVisitIds))
                };
                Task.WaitAll(backgroundTasks);
                foreach (var task in backgroundTasks)
                {
                    hpoList.AddRange(task.Result);
                }
            }
            else if (!string.IsNullOrWhiteSpace(patientEMRDetail))
            {
                // to do interface to get hpo list by emr text
            }
            return hpoList;
        }

  

        public List<NlpRareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine)
        {
            var rareDiseaseList = new List<NlpRareDiseaseResponseModel>();
            if (_env.IsProduction() || 1 == 1)
            {
                var requestData = new RareDiseaseEngineRequestModel();
                requestData.AnalyzeEngine = rareAnalyzeEngine;
                requestData.DataBase = rareDataBaseEngine;
                requestData.HPOList = hpoList.Select(x => x.HPOId).ToArray();


                //var requestStr = rareAnalyzeEngine + "|" + rareDataBaseEngine + "|";
                //foreach (var r in hpoList)
                //{
                //    requestStr += r.HPOId + ",";
                //}
                //requestStr = requestStr.Substring(0, requestStr.Length - 1);

                var requestStr = JsonConvert.SerializeObject(requestData);
                var data = string.Empty;
                var client = _clientFactory.CreateClient("nlp");
                var api = _config.GetValue<string>("NLPAddress:DiseaseApi");
                try
                {

                    var request = new HttpRequestMessage(HttpMethod.Post, api);
                    var requestContent = string.Format("texts={0}", requestStr);
                    request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.SendAsync(request);
                    data = response.Result.Content.ReadAsStringAsync().Result.ToString();
                }
                catch (Exception ex)
                {
                    _logger.LogError("调用NLP RareDisease API 出错:" + ex.ToString());
                    var request = new HttpRequestMessage(HttpMethod.Post, api);
                    requestStr = "'" + requestStr + "'";
                    var requestContent = string.Format("texts={0}", requestStr);
                    request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = client.SendAsync(request);
                    data = response.Result.Content.ReadAsStringAsync().Result.ToString();
                }

                //字符串API 可能有引号在开始和结尾 
                if (data.StartsWith("\""))
                {
                    data = data.TrimStart(new char[] { '\"' }).TrimEnd(new char[] { '\"' });
                }
                rareDiseaseList = JsonConvert.DeserializeObject<List<NlpRareDiseaseResponseModel>>(data);
            }
            else
            {
                rareDiseaseList.Add(new NlpRareDiseaseResponseModel { Name = "anemia", Ratio = 1, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP:01111", HpoName = "肿大", Match = 1 });
                rareDiseaseList[0].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001745", HpoName = "肺肿大", Match = 0 });

                rareDiseaseList.Add(new NlpRareDiseaseResponseModel { Name = "帕金森", Ratio = 0.9, HPOMatchedList = new List<NLPRareDiseaseResponseHPODataModel>() });
                rareDiseaseList[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001644", HpoName = "痴呆", Match = 1 });
                rareDiseaseList[1].HPOMatchedList.Add(new NLPRareDiseaseResponseHPODataModel { HpoId = "HP0001345", HpoName = "行动不便", Match = 0 });


            }
            rareDiseaseList = rareDiseaseList.OrderByDescending(x => x.Ratio).ToList();
            return rareDiseaseList;
        }

    }
}
