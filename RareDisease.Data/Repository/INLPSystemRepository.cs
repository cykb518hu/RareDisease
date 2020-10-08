using Microsoft.AspNetCore.Hosting;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            if (_env.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(patientEmpiId))
                {
                    hpoList = _rdrDataRepository.GetPatientNlpResult(patientEmpiId);
                    var examBase = _localMemoryCache.GetExamBaseDataList();
                    var examList = _rdrDataRepository.GetPatientExamData(patientEmpiId);
                    foreach (var r in examBase)
                    {
                        var item = new ExamBaseDataModel();
                        if (r.Maxinum > 0 && r.Minimum > 0)
                        {
                            item = examList.FirstOrDefault(x => x.ExamValue > r.Minimum && x.ExamValue < r.Maxinum);
                         
                        }
                        else if(r.Maxinum == 0 && r.Minimum > 0)
                        {
                            item = examList.FirstOrDefault(x => x.ExamValue < r.Minimum );
                        }
                        else if (r.Maxinum > 0 && r.Minimum == 0)
                        {
                            item = examList.FirstOrDefault(x => x.ExamValue > r.Maxinum);
                        }
                        if (item != null&&!string.IsNullOrWhiteSpace(item.HPOId))
                        {
                            var hpoItem = new HPODataModel();
                            hpoItem.HpoId = item.HPOId;
                            hpoItem.Name = item.HPOName;
                            hpoItem.NameEnglish = item.HPOEnglish;
                            hpoItem.IsSelf = "本人";
                            hpoItem.Certain = "阳性";
                            hpoItem.HasExam = true;
                            hpoItem.ExamData = new List<ExamBaseDataModel>();
                            hpoItem.ExamData.Add(item);
                            hpoList.Add(hpoItem);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(patientEMRDetail))
                {
                    // to do interface to get hpo list by emr text
                }

            }
            else
            {
                hpoList.Add(new HPODataModel { Name = "运动迟缓", NameEnglish = "Bradykinesia", HpoId = "HP:0002067", StartIndex = 26, EndIndex = 31, Count = 1, Editable = true, Certain = "阳性", IsSelf = "本人" });
                hpoList.Add(new HPODataModel { Name = "常染色体隐性遗传", NameEnglish = "Autosomal recessive inheritance", HpoId = "HP:0000007", StartIndex = 386, EndIndex = 394, Count = 1, Editable = true, Certain = "阳性", IsSelf = "他人" });
                hpoList.Add(new HPODataModel { Name = "构音障碍", NameEnglish = "Dysarthria", HpoId = "HP:0001260", StartIndex = 334, EndIndex = 338, Count = 1, Editable = true, Certain = "阴性", IsSelf = "本人" });

                hpoList.Add(new HPODataModel { Name = "高蛋白血症", NameEnglish = "Hyperproteinemia", HpoId = "HP:0002152",  Count = 1, Editable = false, Certain = "阴性", IsSelf = "本人" });
                var item = new ExamBaseDataModel();
                item.HPOId = "HP:0002152";
                item.HPOName = "高蛋白血症";
                item.HPOEnglish = "Hyperproteinemia";
                item.ExamCode = "2925";
                item.ExamName = "总蛋白";
                item.SampleCode = "LIS126";
                item.SampleName = "血清";
                item.Range = "60.0-83.0 g/L";
                item.ExamValue = 121;
                item.ExamTime = DateTime.Now.AddDays(-30);
                hpoList[3].HasExam = true;
                hpoList[3].ExamData = new List<ExamBaseDataModel>();
                hpoList[3].ExamData.Add(item);
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
