﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class HPODataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameEnglish")]
        public string NameEnglish { get; set; }

        [JsonProperty("hpoId")]
        public string HpoId { get; set; }

        [JsonProperty("certain")]
        public string Certain { get; set; }

        [JsonProperty("isSelf")]
        public string IsSelf { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }

        [JsonProperty("editable")]
        public bool Editable { get; set; }

        [JsonProperty("matched")]
        public string Matched { get; set; }
    }
}