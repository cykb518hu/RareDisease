using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class PatientTimeLineDistributionModel
    {
        [JsonProperty("legendData")]
        public List<string> LegendData { get; set; }

        [JsonProperty("xAxisData")]
        public List<string> xAxisData { get; set; }

        [JsonProperty("lineData")]
        public Dictionary<string, List<int>> LineData { get; set; }
    }

    public class CabinPatientGenderTimeLine
    {
        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("male")]
        public int Male { get; set; }

        [JsonProperty("female")]
        public int Female { get; set; }
    }
}
