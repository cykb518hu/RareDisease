using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RareDisease.Data.Model
{

    public class RareDiseaseDetailModel
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("name_en")]
        public string NameEnglish { get; set; }

        [JsonProperty("HPOId")]
        public string HPOId { get; set; }

        [JsonProperty("HPOText")]
        public string HPOText { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("editable")]
        public bool Editable { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }
    }
    public class ChinaRareDiseaseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_en")]
        public string NameEnglish { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }
    }
}
