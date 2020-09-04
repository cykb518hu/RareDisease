using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class PatientOverviewModel
    {
        [JsonProperty("iEMPINumber")]
        public string EMPINumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("tel")]
        public string PhoneNumber { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }


    public class PatientVisitInfoModel
    {
        [JsonProperty("visitTime")]
        public string VisitTime { get; set; }

        [JsonProperty("visitType")]
        public string VisitType { get; set; }

        [JsonProperty("diagDesc")]
        public string DiagDesc { get; set; }

        [JsonProperty("center")]
        public string Center { get; set; }
    }
}
