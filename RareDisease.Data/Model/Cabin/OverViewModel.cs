using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model.Cabin
{
    public class OverViewModel
    {
        [JsonProperty("ititle")]
        public string Title { get; set; }

        [JsonProperty("iresult")]
        public string Result { get; set; }
    }
}
