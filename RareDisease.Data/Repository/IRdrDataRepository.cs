using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RareDisease.Data.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RareDisease.Data.Repository
{
    public interface IRdrDataRepository
    {
        List<PatientOverviewModel> GetPatientOverview(string number,string numberType);
        List<PatientVisitInfoModel> GetPatientVisitList(string number);
        string GetPatientEMRDetail(string patientVisitIds);


        /// <summary>
        /// 病人原始检验数据
        /// </summary>
        /// <param name="patientEmpiId"></param>
        /// <returns></returns>
        List<HPODataModel> GetPatientExamDataResult(string patientVisitIds);


        List<RareDiseaseDetailModel> SearchStandardRareDiseaseList(string searchText);

        List<HPODataModel> SearchStandardHPOList(string searchHPOText);

        string GetVisitIdByNumber(string number);
        string GetEmrForNLP(string patientVisitId);

        string GetFullEmrAll(string patientVisitId);
        List<HPODataModel> GetExamHPOResultBatch(string patientVisitId);


    }

    public class RdrDataRepository: RareDiseaseGPDbContext,IRdrDataRepository
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<RdrDataRepository> _logger;
        private readonly ILocalMemoryCache _localMemoryCache;
        public RdrDataRepository(IHostingEnvironment hostingEnvironment, ILogger<RdrDataRepository> logger, ILocalMemoryCache localMemoryCache)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _localMemoryCache = localMemoryCache;
        }



        public List<PatientOverviewModel> GetPatientOverview(string number, string numberType)
        {
            var result = new List<PatientOverviewModel>();
            if (_hostingEnvironment.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(number))
                {
                    string sql =GetSqlText("home-patient-overview-empi-sql.txt");
                    if (numberType == "card")
                    {
                        sql = GetSqlText("home-patient-overview-card-sql.txt");
                    }
                    sql = string.Format(sql, number);
                    using (var reader = dbgp.Ado.GetDataReader(sql))
                    {
                        if (reader.Read())
                        {
                            var data = new PatientOverviewModel();
                            data.EMPINumber = reader["empi"] == DBNull.Value ? "" : reader["empi"].ToString();
                            data.Name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            data.Gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                            data.CardNo = reader["cardno"] == DBNull.Value ? "" : reader["cardno"].ToString();
                            data.PhoneNumber = reader["tel"] == DBNull.Value ? "" : reader["tel"].ToString();
                            data.Address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                            result.Add(data);
                        }
                    }
                }
            }
            else
            {
                result.Add(new PatientOverviewModel { EMPINumber = "1234567890", Address = "成都市龙泉驿区大面镇银河路118号恒大绿洲", CardNo = "511025196903220551", Gender = "男", Name = "叶问", PhoneNumber = "13550330299" });

            }
            return result;
        }

        public List<PatientVisitInfoModel> GetPatientVisitList(string number)
        {
            var patientVisitList = new List<PatientVisitInfoModel>();

            if (_hostingEnvironment.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(number))
                {
                    string sql = GetSqlText("home-patient-visit-list-empi-sql.txt");
                    sql = string.Format(sql, number);
                    using (var reader = dbgp.Ado.GetDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            var data = new PatientVisitInfoModel();
                            data.VisitId = reader["visitid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["visitid"]);
                            data.VisitTime = reader["visittime"] == DBNull.Value ? "" : reader["visittime"].ToString();
                            data.VisitType = reader["visittype"] == DBNull.Value ? "" : reader["visittype"].ToString();
                            data.DiagDesc = reader["diagdesc"] == DBNull.Value ? "" : reader["diagdesc"].ToString();
                            data.Center = reader["center"] == DBNull.Value ? "" : reader["center"].ToString();
                            patientVisitList.Add(data);
                        }
                    }
                }
            }
            else
            {
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "现病史", DiagDesc = "患者李**，男性，67岁，主因“右侧肢体抖动、僵硬、动作不灵活7年，累及左侧5年”，来我院门诊就诊...", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2019-08-03", VisitType = "现病史", DiagDesc = "患者李**，男性，66岁，主因“右侧肢体抖动、僵硬、动作不灵活6年，累及左侧4年”，来我院门诊就诊...", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2017-04-13", VisitType = "现病史", DiagDesc = "患者李**，男性，65岁，以安静状态下明显，紧张、激动时加重，平静放松后减轻，睡眠后消失...", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2019-07-22", VisitType = "住院", DiagDesc = "发烧", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2019-02-21", VisitType = "住院", DiagDesc = "铁沉积性疾病", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-11-13", VisitType = "门诊", DiagDesc = "流行性感冒", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-09-15", VisitType = "门诊", DiagDesc = "发烧", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-19-03", VisitType = "住院", DiagDesc = "多系统萎缩", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2016-02-12", VisitType = "住院", DiagDesc = "脊髓小脑性共济失调", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2015-09-21", VisitType = "住院", DiagDesc = "运动迟缓", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2015-01-03", VisitType = "门诊", DiagDesc = "肌无力", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2014-05-05", VisitType = "门诊", DiagDesc = "肌无力", Center = "华西医院" });
                //patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2024-01-01", VisitType = "住院", DiagDesc = "流行性感冒", Center = "华西医院" });
            }
            return patientVisitList;
        }

        public string GetPatientEMRDetail(string patientVisitIds)
        {
            var result = string.Empty;
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(patientVisitIds))
            {
                string sql = GetSqlText("home-patient-EMR-sql.txt");
                sql = string.Format(sql, patientVisitIds);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        result += reader["emr_text"] == DBNull.Value ? "" : reader["emr_text"].ToString() + "\n\n";
                    }
                }
            }
            else
            {
                result = @"患者李**，男性，67岁，主因“右侧肢体抖动、僵硬、动作不灵活7年，累及左侧5年”，来我院门诊就诊。患者7年前无明显诱因出现右手不自主抖动，以安静状态下明显，紧张、激动时加重，平静放松后减轻，睡眠后消失；伴右侧肢体活动不灵活、僵硬，如写字慢、越写越小。症状逐渐加重，波及右下肢。5年前左侧肢体亦出现上述症状，切菜、系扣等动作慢。走路慢，小碎步，起床迈步转身费力，呈弯腰驼背姿势，症状缓慢加重。5年前开始口服美多巴，上述症状明显改善。但2年前因逐渐出现药效减退，患者自行将药量逐渐增加250mg 4id。约半年前开始出现服药2-3小时后肢体不自主扭动表现，且一天之中上述症状波动明显。发病以来便秘明显，睡眠差。发病以来否认站立头晕、吞咽困难、饮水呛咳、平衡障碍。既往：无构音障碍、CO中毒史、脑炎病史、重金属中毒史、农药中毒史、脑出血脑梗塞病史，家族中有类似疾病患者，可能有常染色体隐性遗传，无长期大量应用D2受体阻滞剂、多巴胺耗竭剂病史。专科查体：体温：36.5℃，呼吸：18次/分，脉搏：76次/分。神志清楚，面具脸，流涎较多、颜面躯干皮脂分泌增多。平卧血压120/ 80 mmHg，立位血压120/ 80mmHg。颅神经检查：双眼各向活动无障碍，无复视。面部感觉对称正常，咬肌、颞肌有力，张口下颌不偏。双侧闭目有力、示齿口角无偏斜。伸舌居中。躯体深浅感觉对称正常。慌张步态，行走时躯干前屈，双上臂无伴随动作。四肢肌力V级，四肢肌张力高，呈齿轮样强直，右侧重于右侧。肌肉无明显萎缩。双侧肢体3~5Hz粗大搓丸样静止性震颤，小写征明显。指鼻试验、跟膝胫试验稳准。肱二头肌、膝腱反射无明显亢进，双侧Hoffmann征、Babinski征阴性。颈部僵硬。双侧Kernig’ s sign (-) 。辅助检查：头颅MRI平扫未见明显异常。" + "\n\n";
            }
            return result;
            // return "";
        }

        public List<HPODataModel> GetPatientExamDataResult(string patientVisitIds)
        {
            var result = new List<HPODataModel>();
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(patientVisitIds))
            {
                //获取所有检验HPO规则
                var examBase = _localMemoryCache.GetExamBaseDataList();
                //获取所有检验数据
                var examList= new List<ExamBaseDataModel>();
                string sql = GetSqlText("home-get-exam-data-sql.txt");
                sql = string.Format(sql, patientVisitIds);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        var data = new ExamBaseDataModel();
                        data.ExamCode = reader["exam_code"] == DBNull.Value ? "" : reader["exam_code"].ToString();
                        data.ExamName = reader["exam_name"] == DBNull.Value ? "" : reader["exam_name"].ToString();
                        data.SampleCode = reader["sample_code"] == DBNull.Value ? "" : reader["sample_code"].ToString();
                        data.SampleName = reader["sample_name"] == DBNull.Value ? "" : reader["sample_name"].ToString();
                        data.Range = reader["range"] == DBNull.Value ? "" : reader["range"].ToString();
                        data.ExamValue = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        data.ExamTimeStr = reader["examTimeStr"] == DBNull.Value ? "" : reader["examTimeStr"].ToString();
                        examList.Add(data);
                    }

                }
                //遍历每条规则，查看检验数据里面是否有符合规则的数据
                foreach (var r in examBase)
                {
                    var subList = examList.Where(x => x.ExamCode == r.ExamCode && x.SampleCode.ToUpper() == r.SampleCode.ToUpper()).ToList();
                    if(subList.Count==0)
                    {
                        continue;
                    }
                    var list = new List<ExamBaseDataModel>();
                    //区间命中
                    if (r.Maxinum > 0 && r.Minimum > 0)
                    {
                        list = subList.Where(x => x.ExamValue > r.Minimum && x.ExamValue < r.Maxinum).ToList();
                    }
                    //低于最小值命中
                    else if (r.Maxinum == 0 && r.Minimum > 0)
                    {
                        list = subList.Where(x => x.ExamValue < r.Minimum ).ToList();
                    }
                    //大于最大值命中
                    else if (r.Maxinum > 0 && r.Minimum == 0)
                    {
                        list = subList.Where(x => x.ExamValue > r.Maxinum).ToList();
                    }
                    if (list != null && list.Any())
                    {
                        if (list.Count >= r.MatchTime)
                        {
                            var data = result.FirstOrDefault(x => x.HPOId == r.HPOId);
                            if (data != null)
                            {
                                data.ExamData.AddRange(list);
                            }
                            else
                            {
                                var hpoItem = new HPODataModel();
                                hpoItem.HPOId = r.HPOId;
                                hpoItem.Name = r.HPOName;
                                hpoItem.CHPOName = r.HPOName;
                                hpoItem.NameEnglish = r.HPOEnglish;
                                hpoItem.TermSource = "检验规则";
                                hpoItem.HasExam = true;
                                hpoItem.ExamData = new List<ExamBaseDataModel>();
                                hpoItem.ExamData.AddRange(list);
                                result.Add(hpoItem);
                            }
                        }
                    }
                }

            }
            else
            {
                var hpoItem = new HPODataModel { Name = "高蛋白血症", NameEnglish = "Hyperproteinemia", HPOId = "HP:0002152", CHPOName = "高蛋白血症" , TermSource = "检验规则" };
                var item = new ExamBaseDataModel();
                item.HPOId = "HP:0002152";
                item.HPOName = "高蛋白血症";
                item.HPOEnglish = "Hyperproteinemia";
                item.ExamCode = "2925";
                item.ExamName = "总蛋白";
                item.SampleCode = "LIS126";
                item.SampleName = "血清";
                item.Range = "60.0-83.0 g/L";
                item.ExamValue = 121;
                item.ExamTimeStr = "2019-12-12";

                var item1 = new ExamBaseDataModel();
                item1.HPOId = "HP:0002152";
                item1.HPOName = "高蛋白血症";
                item1.HPOEnglish = "Hyperproteinemia";
                item1.ExamCode = "2925";
                item1.ExamName = "总蛋白";
                item1.SampleCode = "LIS126";
                item1.SampleName = "血清";
                item1.Range = "60.0-83.0 g/L";
                item1.ExamValue = 99;
                item1.ExamTimeStr = "2020-01-12";

                hpoItem.HasExam = true;
                hpoItem.ExamData = new List<ExamBaseDataModel>();
                hpoItem.ExamData.Add(item);
                hpoItem.ExamData.Add(item1);
                result.Add(hpoItem);
            }

            return result;
        }


        public List<RareDiseaseDetailModel> SearchStandardRareDiseaseList(string searchText)
        {
            var result = new List<RareDiseaseDetailModel>();
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(searchText))
            {
                string sql = GetSqlText("search-standard-disease-sql.txt");
                sql = string.Format(sql, searchText);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        var data = new RareDiseaseDetailModel();
                        data.Source = reader["source"] == DBNull.Value ? "" : reader["source"].ToString();
                        data.NameEnglish = reader["name_en"] == DBNull.Value ? "" : reader["name_en"].ToString();
                        data.HPOId = reader["hpoid"] == DBNull.Value ? "" : reader["hpoid"].ToString();
                        data.HPONameChinese = reader["hpo_name"] == DBNull.Value ? "" : reader["hpo_name"].ToString();
                        data.HPONameEnglish = reader["hpo_name_en"] == DBNull.Value ? "" : reader["hpo_name_en"].ToString();
                        result.Add(data);
                    }
                }
            }
            else
            {
                var data = new RareDiseaseDetailModel();
                data.Source = "omaha";
                data.NameEnglish = "Hyperproteinemia";
                data.HPOId = "HP:000007";
                data.HPONameChinese = "帕金森病";
                data.HPONameEnglish = "Hyperproteinemia";
                result.Add(data);
                //var data1 = new RareDiseaseDetailModel();
                //data1.Source = "OMIM";
                //data1.NameEnglish = "Wilson disease addsdlflajsdlfj,sdlfjlasjdfk, adsfasdlkjadskj,1,daslkjlfkjds, adslkjladj, adlkjlkdj, adljlkjd, adslkjlkjad, adlkjlkad, aldkjklad,dalkjd";
                //data1.HPOId = "HP:000007";
                //data1.HPONameChinese = "高蛋白血症2";
                //data1.HPONameEnglish = "Hyperproteinemia";
                //result.Add(data1);
                //var data2 = new RareDiseaseDetailModel();
                //data2.Source = "OMIM";
                //data2.NameEnglish = "WWilson disease addsdlflajsdlfj,sdlfjlasjdfk, adsfasdlkjadskj,1,daslkjlfkjds, adslkjladj, adlkjlkdj, adljlkjd, adslkjlkjad, adlkjlkad, aldkjklad,dalkjde";
                //data2.HPOId = "HP:000007";
                //data2.HPONameChinese = "高蛋白血症3";
                //data2.HPONameEnglish = "Hyperproteinemia";
                //result.Add(data2);

                //var data3 = new RareDiseaseDetailModel();
                //data3.Source = "OMIM";
                //data3.NameEnglish = "WWilson disease addsdlflajsdlfj,sdlfjlasjdfk, adsfasdlkjadskj,1,daslkjlfkjds, adslkjladj, adlkjlkdj, adljlkjd, adslkjlkjad, adlkjlkad, aldkjklad,dalkjde";
                //data3.HPOId = "HP:000007";
                //data3.HPONameChinese = "高蛋白血症4";
                //data3.HPONameEnglish = "Hyperproteinemia";
                //result.Add(data3);
            }

            return result;
        }

        public List<HPODataModel> SearchStandardHPOList(string searchHPOText)
        {
            var searchedHPOList = new List<HPODataModel>();
            if (_hostingEnvironment.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(searchHPOText))
                {
                    string sql = GetSqlText("search-standard-hpo-sql.txt");
                    sql = string.Format(sql, searchHPOText);
                    using (var reader = dbgp.Ado.GetDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            var data = new HPODataModel();
                            data.Name = reader["name_cn"] == DBNull.Value ? "" : reader["name_cn"].ToString();
                            data.NameEnglish = reader["name_en"] == DBNull.Value ? "" : reader["name_en"].ToString();
                            data.HPOId = reader["hpoid"] == DBNull.Value ? "" : reader["hpoid"].ToString();
                            searchedHPOList.Add(data);
                        }
                    }
                }
            }
            else
            {
                searchedHPOList.Add(new HPODataModel { Name = "震颤", NameEnglish = "Tremor", HPOId = "HP:0001337"});
                searchedHPOList.Add(new HPODataModel { Name = "帕金森症", NameEnglish = "Parkinsonism", HPOId = "HP:0001300"});
                searchedHPOList.Add(new HPODataModel { Name = "运动迟缓", NameEnglish = "Bradykinesia", HPOId = "HP:0002067"});
                searchedHPOList.Add(new HPODataModel { Name = "强直", NameEnglish = "Rigidity", HPOId = "HP:0002063"});
                searchedHPOList.Add(new HPODataModel { Name = "姿势不稳", NameEnglish = "Postural instability", HPOId = "HP:0002172"});
                searchedHPOList.Add(new HPODataModel { Name = "核上性凝视麻痹", NameEnglish = "Supranuclear gaze palsy", HPOId = "HP:0000605"});
                searchedHPOList.Add(new HPODataModel { Name = "眼睑失用症", NameEnglish = "Eyelid apraxia", HPOId = "HP:0000658"});
                searchedHPOList.Add(new HPODataModel { Name = "肌张力障碍", NameEnglish = "Dystonia", HPOId = "HP:0001332"});
                searchedHPOList.Add(new HPODataModel { Name = "智能衰退", NameEnglish = "Mental deterioration", HPOId = "HP:0001268"});
                searchedHPOList.Add(new HPODataModel { Name = "构音障碍", NameEnglish = "Dysarthria", HPOId = "HP:0001260"});
                searchedHPOList.Add(new HPODataModel { Name = "曳行步态", NameEnglish = "Shuffling gait", HPOId = "HP:0002362"});
                searchedHPOList.Add(new HPODataModel { Name = "常染色体隐性遗传", NameEnglish = "Autosomal recessive inheritance", HPOId = "HP:0000007"});
            }
            foreach(var r in searchedHPOList)
            {
                r.CHPOName = r.Name;
            }
            return searchedHPOList;
        }


        public string GetVisitIdByNumber(string number)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(number))
            {
                string sql = GetSqlText("get-visitid-sql.txt");
                sql = string.Format(sql, number);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    if (reader.Read())
                    {
                        result = reader["id"] == DBNull.Value ? "" : reader["id"].ToString();
                    }
                }
            }
            return result;
            // return "";
        }

        public string GetEmrForNLP(string patientVisitId)
        {
            var emrList = new List<PatientEMRModel>();
            var result = string.Empty;
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(patientVisitId))
            {
                string sql = GetSqlText("home-patient-EMR-full-sql.txt");
                sql = string.Format(sql, patientVisitId);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        var data = new PatientEMRModel();
                        data.Detail = reader["emr_text"] == DBNull.Value ? "" : reader["emr_text"].ToString();
                        data.Type = reader["emr_type"] == DBNull.Value ? "" : reader["emr_type"].ToString();
                        emrList.Add(data);
                    }
                }
                emrList = emrList.Where(x => x.Detail.Length > 20).ToList();
                var mainemr = emrList.FirstOrDefault(x => x.Type == "主诉");
                if (mainemr != null)
                {
                    emrList.Remove(mainemr);
                    emrList.Insert(0, mainemr);
                }
                var currentemr = emrList.FirstOrDefault(x => x.Type == "现病史");
                if (currentemr != null)
                {
                    emrList.Remove(currentemr);
                    emrList.Insert(0, currentemr);
                }
                foreach (var r in emrList)
                {
                    if (result.Length < 50000)
                    {
                        result += r.Detail;
                    }
                }
            }
            else
            {
                result = @"患者李**，男性，67岁，主因“右侧肢体抖动、僵硬、动作不灵活7年，累及左侧5年”，来我院门诊就诊。患者7年前无明显诱因出现右手不自主抖动，以安静状态下明显，紧张、激动时加重，平静放松后减轻，睡眠后消失；伴右侧肢体活动不灵活、僵硬，如写字慢、越写越小。症状逐渐加重，波及右下肢。5年前左侧肢体亦出现上述症状，切菜、系扣等动作慢。走路慢，小碎步，起床迈步转身费力，呈弯腰驼背姿势，症状缓慢加重。5年前开始口服美多巴，上述症状明显改善。但2年前因逐渐出现药效减退，患者自行将药量逐渐增加250mg 4id。约半年前开始出现服药2-3小时后肢体不自主扭动表现，且一天之中上述症状波动明显。发病以来便秘明显，睡眠差。发病以来否认站立头晕、吞咽困难、饮水呛咳、平衡障碍。既往：无构音障碍、CO中毒史、脑炎病史、重金属中毒史、农药中毒史、脑出血脑梗塞病史，家族中有类似疾病患者，可能有常染色体隐性遗传，无长期大量应用D2受体阻滞剂、多巴胺耗竭剂病史。专科查体：体温：36.5℃，呼吸：18次/分，脉搏：76次/分。神志清楚，面具脸，流涎较多、颜面躯干皮脂分泌增多。平卧血压120/ 80 mmHg，立位血压120/ 80mmHg。颅神经检查：双眼各向活动无障碍，无复视。面部感觉对称正常，咬肌、颞肌有力，张口下颌不偏。双侧闭目有力、示齿口角无偏斜。伸舌居中。躯体深浅感觉对称正常。慌张步态，行走时躯干前屈，双上臂无伴随动作。四肢肌力V级，四肢肌张力高，呈齿轮样强直，右侧重于右侧。肌肉无明显萎缩。双侧肢体3~5Hz粗大搓丸样静止性震颤，小写征明显。指鼻试验、跟膝胫试验稳准。肱二头肌、膝腱反射无明显亢进，双侧Hoffmann征、Babinski征阴性。颈部僵硬。双侧Kernig’ s sign (-) 。辅助检查：头颅MRI平扫未见明显异常。" + "\n\n";
            }
            return result;
        }


        public string GetFullEmrAll(string patientVisitId)
        {
            var emrList = new List<PatientEMRModel>();
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(patientVisitId))
            {
                string sql = GetSqlText("home-patient-EMR-full-sql.txt");
                sql = string.Format(sql, patientVisitId);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        var data = new PatientEMRModel();
                        data.Detail = reader["emr_text"] == DBNull.Value ? "" : reader["emr_text"].ToString();
                        data.Type = reader["emr_type"] == DBNull.Value ? "" : reader["emr_type"].ToString();
                        emrList.Add(data);
                    }
                }
            }

            var mainemr = emrList.FirstOrDefault(x => x.Type == "主诉");
            if (mainemr != null)
            {
                emrList.Remove(mainemr);
                emrList.Insert(0, mainemr);
            }
            var currentemr = emrList.FirstOrDefault(x => x.Type == "现病史");
            if (currentemr != null)
            {
                emrList.Remove(currentemr);
                emrList.Insert(0, currentemr);
            }
            foreach (var r in emrList)
            {
                result += r.Detail;
            }
            return result;
        }

        public List<HPODataModel> GetExamHPOResultBatch(string patientVisitId)
        {
            var result = new List<HPODataModel>();
            if (string.IsNullOrWhiteSpace(patientVisitId))
            {
                //获取所有检验HPO规则
                var examBase = _localMemoryCache.GetExamBaseDataList();
                //获取所有检验数据
                var examList = new List<ExamBaseDataModel>();
                string sql = GetSqlText("home-get-exam-data-sql.txt");
                sql = string.Format(sql, patientVisitId);
                using (var reader = dbgp.Ado.GetDataReader(sql))
                {
                    while (reader.Read())
                    {
                        var data = new ExamBaseDataModel();
                        data.ExamCode = reader["exam_code"] == DBNull.Value ? "" : reader["exam_code"].ToString();
                        data.ExamName = reader["exam_name"] == DBNull.Value ? "" : reader["exam_name"].ToString();
                        data.SampleCode = reader["sample_code"] == DBNull.Value ? "" : reader["sample_code"].ToString();
                        data.SampleName = reader["sample_name"] == DBNull.Value ? "" : reader["sample_name"].ToString();
                        data.Range = reader["range"] == DBNull.Value ? "" : reader["range"].ToString();
                        data.ExamValue = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        data.ExamTimeStr = reader["examTimeStr"] == DBNull.Value ? "" : reader["examTimeStr"].ToString();
                        examList.Add(data);
                    }

                }
                //遍历每条规则，查看检验数据里面是否有符合规则的数据
                foreach (var r in examBase)
                {
                    var subList = examList.Where(x => x.ExamCode == r.ExamCode && x.SampleCode.ToUpper() == r.SampleCode.ToUpper()).ToList();
                    if (subList.Count == 0)
                    {
                        continue;
                    }
                    var list = new List<ExamBaseDataModel>();
                    //区间命中
                    if (r.Maxinum > 0 && r.Minimum > 0)
                    {
                        list = subList.Where(x => x.ExamValue > r.Minimum && x.ExamValue < r.Maxinum).ToList();
                    }
                    //低于最小值命中
                    else if (r.Maxinum == 0 && r.Minimum > 0)
                    {
                        list = subList.Where(x => x.ExamValue < r.Minimum).ToList();
                    }
                    //大于最大值命中
                    else if (r.Maxinum > 0 && r.Minimum == 0)
                    {
                        list = subList.Where(x => x.ExamValue > r.Maxinum).ToList();
                    }
                    if (list != null && list.Any())
                    {
                        if (list.Count >= r.MatchTime)
                        {
                            if (!result.Any(x => x.HPOId == r.HPOId))
                            {
                                var hpoItem = new HPODataModel();
                                hpoItem.HPOId = r.HPOId;
                                result.Add(hpoItem);
                            }
                        }
                    }
                }

            }
            else
            {
     
            }

            return result;
        }


        public string GetSqlText(string fileName)
        {
            var path = _hostingEnvironment.ContentRootPath + "//App_Data//sql//" + fileName;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册简体中文的支持
            var str = File.ReadAllText(path, Encoding.GetEncoding("gb2312"));
            return str;
        }
    }
}
