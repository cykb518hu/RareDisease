using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class DiseaseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("likeness")]
        public string Likeness { get; set; }
    }
}
