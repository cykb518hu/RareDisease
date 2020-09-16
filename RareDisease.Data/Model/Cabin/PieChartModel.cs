using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class PieChartModel
    {
        [JsonProperty("legendData")]
        public List<string> legendData { get; set; }

        [JsonProperty("seriesData")]
        public List<SeriesDataModel> seriesData { get; set; }
    }
}
