using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class DiseasePatientTimeLineDistributionModel
    {
        [JsonProperty("legendData")]
        public List<string> LegendData { get; set; }

        [JsonProperty("xAxisData")]
        public List<string> xAxisData { get; set; }

        [JsonProperty("lineData")]
        public Dictionary<string, List<double>> LineData { get; set; }
    }
}
