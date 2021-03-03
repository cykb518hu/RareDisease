using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZW.Localization.Model
{
    public class LanguageModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
