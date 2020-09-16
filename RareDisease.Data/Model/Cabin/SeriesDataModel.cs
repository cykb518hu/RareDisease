using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class SeriesDataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
