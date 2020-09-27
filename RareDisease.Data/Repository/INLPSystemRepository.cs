using Microsoft.AspNetCore.Hosting;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface INLPSystemRepository
    {

        List<HPODataModel> AnalyzePatientHPO(string nlpEngine, string patientEMRDetail, string patientEmpiId);

        List<HPODataModel> SearchHPOList(string searchHPOText);

        List<RareDiseaseResponseModel> GetDiseaseListByHPO(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine);
    }

    public class NLPSystemRepository : INLPSystemRepository
    {
        private readonly IHostingEnvironment _env;

        private readonly IRdrDataRepository _rdrDataRepository;

        public NLPSystemRepository(IHostingEnvironment env, IRdrDataRepository rdrDataRepository)
        {
            _env = env;
            _rdrDataRepository = rdrDataRepository;
        }
        /// <summary>
        /// patientEmpiId can be empiid or cardNo
        /// </summary>
        /// <param name="nlpEngine"></param>
        /// <param name="patientEMRDetail"></param>
        /// <param name="patientEmpiId"></param>
        /// <returns></returns>
        public List<HPODataModel> AnalyzePatientHPO(string nlpEngine, string patientEMRDetail, string patientEmpiId)
        {
            var hpoList = new List<HPODataModel>();
            if (_env.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(patientEmpiId))
                {
                    hpoList = _rdrDataRepository.GetAnalyzeHPOResult(patientEmpiId);
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
            }
            return hpoList;
        }

        public List<HPODataModel> SearchHPOList(string searchHPOText)
        {
            var searchedHPOList = new List<HPODataModel>();
            if (_env.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(searchHPOText))
                {
                    //interface
                }
            }
            else
            {
                searchedHPOList.Add(new HPODataModel { Name = "震颤", NameEnglish = "Tremor", HpoId = "HP:0001337", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "帕金森症", NameEnglish = "Parkinsonism", HpoId = "HP:0001300", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "运动迟缓", NameEnglish = "Bradykinesia", HpoId = "HP:0002067", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "强直", NameEnglish = "Rigidity", HpoId = "HP:0002063", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "姿势不稳", NameEnglish = "Postural instability", HpoId = "HP:0002172", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "核上性凝视麻痹", NameEnglish = "Supranuclear gaze palsy", HpoId = "HP:0000605", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "眼睑失用症", NameEnglish = "Eyelid apraxia", HpoId = "HP:0000658", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "肌张力障碍", NameEnglish = "Dystonia", HpoId = "HP:0001332", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "智能衰退", NameEnglish = "Mental deterioration", HpoId = "HP:0001268", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "构音障碍", NameEnglish = "Dysarthria", HpoId = "HP:0001260", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "曳行步态", NameEnglish = "Shuffling gait", HpoId = "HP:0002362", Certain = "阳性", IsSelf = "本人", Count = 1 });
                searchedHPOList.Add(new HPODataModel { Name = "常染色体隐性遗传", NameEnglish = "Autosomal recessive inheritance", HpoId = "HP:0000007", Certain = "阳性", IsSelf = "本人", Count = 1 });
            }
            return searchedHPOList;
        }

        public List<RareDiseaseResponseModel> GetDiseaseListByHPO(List<HPODataModel> hpoList, string rareAnalyzeEngine, string rareDataBaseEngine)
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
