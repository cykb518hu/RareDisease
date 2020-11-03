using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class DiseaseCaculateOverviewModel
    {
        public int Rank { get; set; }
        public string Disease { get; set; }

        public string SupportMethod { get; set; }
        public double Score { get; set; }

        public int SupportMethodCount { get; set; }
        public int QueryHPOCount { get; set; }
        public int MatchedHPOCount { get; set; }
        public int DiseaseHPOCount { get; set; }
    }

    public class DiseaseCaculateSingleModel
    {
        public string Disease { get; set; }

        public double Score { get; set; }

        public string Souce { get; set; }
    }


    public class DiseaseCaluateBarModel
    {
        [JsonProperty("yAxis")]
        public List<string> Disease { get; set; }

        [JsonProperty("SeriesData")]
        public List<SeriesDataModel> SeriesDataModel { get; set; }
    }
    public class SeriesDataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public List<int> Value { get; set; }
    }

}
