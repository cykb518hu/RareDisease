using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDisease.Data.Model
{
    public class HPONLPDbModel
    {
        public string hpoid { get; set; }
        public string name_en { get; set; }
        public string name_cn { get; set; }

    }


    public class HPODataModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameEnglish")]
        public string NameEnglish { get; set; }

        [JsonProperty("hpoId")]
        public string HPOId { get; set; }

        //目前NLP 不支持
        [JsonProperty("certain")]
        public string Certain { get { return "阳性"; } }


        //目前NLP 不支持
        [JsonProperty("isSelf")]
        public string IsSelf { get { return "本人"; } }

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


        [JsonProperty("hasExam")]
        public bool HasExam { get; set; }

        [JsonProperty("examData")]
        public List<ExamBaseDataModel> ExamData { get; set; }

        [JsonProperty("indexList")]
        public List<HPOMatchIndexModel> IndexList { get; set; }
    }

    public class HPOMatchIndexModel
    {
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
