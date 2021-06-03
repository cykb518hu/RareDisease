using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class DiseaseCaculateOverviewModel
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("disease")]
        public string Disease { get; set; }

        [JsonProperty("supportMethod")]
        public string SupportMethod { get; set; }
        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class DiseaseCaculateSingleModel
    {
        [JsonProperty("disease")]
        public string Disease { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

    }


    public class DiseaseCaluateBarModel
    {
        [JsonProperty("yAxis")]
        public List<string> Disease { get; set; }

        [JsonProperty("SeriesData")]
        public List<SeriesData> SeriesDataModel { get; set; }
    }
    public class SeriesData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public List<int> Value { get; set; }
    }

    public class DiseaseCaluateHPODistributionModel
    {
        [JsonProperty("yAxis")]
        public List<string> Disease { get; set; }

        [JsonProperty("xAxis")]
        public List<string> HPOId { get; set; }

        [JsonProperty("marks")]

        public ArrayList Marks { get; set; }

    }

}
