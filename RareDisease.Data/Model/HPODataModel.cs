using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class HPODataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("editable")]
        public bool Editable { get; set; }
    }
}
