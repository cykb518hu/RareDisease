using Microsoft.AspNetCore.Hosting;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RareDisease.Data.Repository
{
    public interface INLPSystemRepository
    {

        List<HPODataModel> GetPatientHPOResult(string nlpEngine, string patientEMRDetail, string patientEmpiId);

       

        List<RareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine);
    }

    public class NLPSystemRepository : INLPSystemRepository
    {
        private readonly IHostingEnvironment _env;

        private readonly IRdrDataRepository _rdrDataRepository;
        private readonly ILocalMemoryCache _localMemoryCache;

        public NLPSystemRepository(IHostingEnvironment env, IRdrDataRepository rdrDataRepository, ILocalMemoryCache localMemoryCache)
        {
            _env = env;
            _rdrDataRepository = rdrDataRepository;
            _localMemoryCache = localMemoryCache;
        }
        /// <summary>
        /// patientEmpiId can be empiid or cardNo
        /// </summary>
        /// <param name="nlpEngine"></param>
        /// <param name="patientEMRDetail"></param>
        /// <param name="patientEmpiId"></param>
        /// <returns></returns>
        public List<HPODataModel> GetPatientHPOResult(string nlpEngine, string patientEMRDetail, string patientEmpiId)
        {
            var hpoList = new List<HPODataModel>();
            if (!string.IsNullOrWhiteSpace(patientEmpiId))
            {
                var backgroundTasks = new[]
                {
                    Task.Run(() => _rdrDataRepository.GetPatientNlpResult(patientEmpiId)),
                    Task.Run(() =>_rdrDataRepository.GetPatientExamDataResult(patientEmpiId))
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

  

        public List<RareDiseaseResponseModel> GetPatientRareDiseaseResult(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine)
        {
            var rareDiseaseList = new List<RareDiseaseResponseModel>();
            if (_env.IsProduction())
            {
                // to do interface to get hpo list by emr text
            }
            else
            {
                rareDiseaseList.Add(new RareDiseaseResponseModel { Name = "常染色体显性帕金森病8型", Likeness = "1" });
                var hpoList1 = new List<RareDiseaseResponseHPODataModel>();
                hpoList1.Add(new RareDiseaseResponseHPODataModel { Name = "构音障碍", HpoId = "HP:0001260", Matched = "true" });
                hpoList1.Add(new RareDiseaseResponseHPODataModel { Name = "常染色体隐性遗传", HpoId = "HP:0000007", Matched = "true" });
                hpoList1.Add(new RareDiseaseResponseHPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "true" });
                rareDiseaseList[0].HPOMatchedList = hpoList1;

                rareDiseaseList.Add(new RareDiseaseResponseModel { Name = "晚发型帕金森病", Likeness = "0.9" });
                var hpoList2 = new List<RareDiseaseResponseHPODataModel>();
                hpoList2.Add(new RareDiseaseResponseHPODataModel { Name = "震颤", HpoId = "HP:0001337", Matched = "false" });
                hpoList2.Add(new RareDiseaseResponseHPODataModel { Name = "帕金森症", HpoId = "HP:0001300", Matched = "true" });
                hpoList2.Add(new RareDiseaseResponseHPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "true" });
                hpoList2.Add(new RareDiseaseResponseHPODataModel { Name = "眼睑失用症", HpoId = "HP:0000658", Matched = "false" });
                rareDiseaseList[1].HPOMatchedList = hpoList2;

                rareDiseaseList.Add(new RareDiseaseResponseModel { Name = "帕金森病17型", Likeness = "0.8" });

                var hpoList3 = new List<RareDiseaseResponseHPODataModel>();
                hpoList3.Add(new RareDiseaseResponseHPODataModel { Name = "震颤", HpoId = "HP:0001337", Matched = "true" });
                hpoList3.Add(new RareDiseaseResponseHPODataModel { Name = "常染色体隐性遗传", HpoId = "HP:0000007", Matched = "false" });
                hpoList3.Add(new RareDiseaseResponseHPODataModel { Name = "运动迟缓", HpoId = "HP:0002067", Matched = "false" });
                hpoList3.Add(new RareDiseaseResponseHPODataModel { Name = "曳行步态", HpoId = "HP:0002362", Matched = "false" });
                rareDiseaseList[2].HPOMatchedList = hpoList3;
            }
            return rareDiseaseList;
        }

    }
}
