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

        [JsonProperty("nameEnglish")]
        public string NameEnglish { get; set; }

        [JsonProperty("chpoName")]
        public string CHPOName { get; set; }

        [JsonProperty("hpoId")]
        public string HPOId { get; set; }

        [JsonIgnore]
        public int Positivie { get; set; } = 1;

        [JsonProperty("certain")]
        public string Certain { get { return Positivie == 1 ? "阳性" : "阴性"; } }

        //目前NLP 不支持
        [JsonProperty("isSelf")]
        public string IsSelf { get { return "本人"; } }

        [JsonProperty("count")]
        public int Count { get { return IndexList == null ? 1 : IndexList.Count; } }

        [JsonIgnore]
        public int StartIndex { get; set; }

        [JsonIgnore]
        public int EndIndex { get; set; }

        [JsonProperty("editable")]
        public bool Editable { get; set; }


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

    /// <summary>
    /// 记录每次就诊病历的长度 用于计算start end index
    /// </summary>
    public class PatientVisitHPOResultModel
    {
        public string EMR { get; set; }
        public string HPOResult { get; set; }
        public int StartIndex { get; set; }
    }


    /// <summary>
    /// 用户CHPO 库
    /// </summary>
    public class CHPO2020Model
    {
        [JsonProperty("hpoId")]
        public string HpoId { get; set; }

        [JsonProperty("name_en")]
        public string NameEnglish { get; set; }

        [JsonProperty("name_cn")]
        public string NameChinese { get; set; }

    }

    /// <summary>
    /// 电子病历spicy 算法的结果
    /// </summary>
    public class HPOAPIEngineResultModel
    {
        [JsonProperty("emrword")]
        public string Emrword { get; set; }

        [JsonProperty("similarchpoterm")]
        public string Similarchpoterm { get; set; }

        [JsonProperty("similarchpoid")]
        public List<string> Similarchpoid { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("positive")]
        public int Positivie { get; set; } = 1;
    }

}
