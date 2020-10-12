using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
   public  class ExamBaseDataModel
    {

        [JsonProperty("HPO_eng")]
        public string HPOEnglish { get; set; }

        [JsonProperty("HPO_ch")]
        public string HPOName { get; set; }

        [JsonProperty("HPO_ID")]
        public string HPOId { get; set; }

        [JsonProperty("exam_code")]
        public string ExamCode { get; set; }

        [JsonProperty("exam_name")]
        public string ExamName { get; set; }

        [JsonProperty("sample_code")]
        public string SampleCode { get; set; }

        [JsonProperty("sample_name")]
        public string SampleName { get; set; }

        [JsonProperty("range")]
        public string Range { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }


        [JsonProperty("examTimeStr")]
        public string ExamTimeStr { get; set; }

        [JsonProperty("value")]
        public float ExamValue { get; set; }

        [JsonProperty("minimum")]
        public float Minimum { get; set; }


        [JsonProperty("maxinum")]
        public float Maxinum { get; set; }
    }
}
