using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class DiseaseHPOSummaryModel
    {
        public string DiseaseName { get; set; }
        public List<DiseaseHPOSummaryHPOModel> HPOList { get; set; } 

        public int CasesCount { get; set; }
    }

    public class DiseaseHPOSummaryHPOModel
    {
        public string HPOId { get; set; }
        public int NlpCount { get; set; }
        public int ExamCount { get; set; }
        public int EramCount { get; set; }
        public int OMIMCount { get; set; }
        public int ORPHACount { get; set; }

        public bool Display { get; set; }
    }

    public class DiseaseHPOSummaryDiseaseNameModel
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
    public class DiseaseHPOSummaryLibraryMappingModel
    {
        public string ChineseName { get; set; }

        public string EnglishName { get; set; }
        
        public string CasesCount { get; set; }

        public string EramId { get; set; }

        public string OMIMId { get; set; }

        public string ORPHAId { get; set; }

    }

    public class DiseaseHPOSummaryBarModel
    {
        [JsonProperty("casesCount")]
        public int CasesCount { get; set; }
        [JsonProperty("yAxis")]
        public List<CHPO2020Model> HPOItem { get; set; }

        [JsonProperty("SeriesData")]
        public List<SeriesData> SeriesDataModel { get; set; }
    }
}
