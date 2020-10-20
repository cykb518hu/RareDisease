using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class RareDiseaseRequestModel
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("emrDetail")]
        public string EMRDetail { get; set; }

        [JsonProperty("nlpEngine")]
        public string NlpEngine { get; set; }

        [JsonProperty("rareAnalyzeEngine")]
        public string RareAnalyzeEngine { get; set; }

        [JsonProperty("rareDataBaseEngine")]
        public string RareDataBaseEngine { get; set; }

        [JsonProperty("appName")]
        public string AppName { get; set; }

        public string IPAddress { get; set; }
    }

    public class NlpRareDiseaseResponseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ratio")]
        public double Ratio { get; set; }

        [JsonProperty("Hpolist")]
        public List<NLPRareDiseaseResponseHPODataModel> HPOMatchedList { get; set; }

    }

    public class NLPRareDiseaseResponseHPODataModel
    {
        [JsonProperty("hpoId")]
        //Id
        public string HpoId { get; set; }

        [JsonProperty("hpoName")]
        public string HpoName { get; set; }

        [JsonProperty("match")]
        public int Match { get; set; }


    }
    public class RareDiseaseEngineRequestModel
    {
        [JsonProperty("analyzeEngine")]
        public string AnalyzeEngine { get; set; }

        [JsonProperty("dataBase")]
        public string DataBase { get; set; }

        [JsonProperty("HPOList")]
        public string[] HPOList { get; set; }
    }
}
