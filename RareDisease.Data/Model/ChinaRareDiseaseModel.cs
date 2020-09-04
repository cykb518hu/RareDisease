using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RareDisease.Data.Model
{
    public class ChinaRareDiseaseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }
    }
}
