using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class BarChartModel
    {
        [JsonProperty("axisData")]
        public List<string> AxisData { get; set; }

        [JsonProperty("seriesData")]
        public List<double> SeriesData { get; set; }
    }
}
