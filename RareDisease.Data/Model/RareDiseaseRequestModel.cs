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

    public class RareDiseaseResponseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("likeness")]
        public string Likeness { get; set; }

        [JsonProperty("hpoMatchedList")]
        public List<RareDiseaseResponseHPODataModel> HPOMatchedList { get; set; }
    }

    public class RareDiseaseResponseHPODataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameEnglish")]
        public string NameEnglish { get; set; }

        [JsonProperty("hpoId")]
        public string HpoId { get; set; }

        [JsonProperty("matched")]
        public string Matched { get; set; }
    }
}
