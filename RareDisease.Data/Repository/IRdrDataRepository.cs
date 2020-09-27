﻿using Microsoft.AspNetCore.Hosting;
using RareDisease.Data.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface IRdrDataRepository
    {
        List<PatientOverviewModel> GetPatientOverview(string number);
        List<PatientVisitInfoModel> GetPatientVisitList(string number);
        string GetPatientEMRText(string patientEmpiId);

        List<RareDiseaseDetailModel> SearchRareDiseaseList(string searchText);

        List<HPODataModel> GetAnalyzeHPOResult(string patientEmpiId);

    }

    public class RdrDataRepository: RareDiseaseGPDbContext,IRdrDataRepository
    {
        private IHostingEnvironment _hostingEnvironment;

        public RdrDataRepository(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public List<PatientOverviewModel> GetPatientOverview(string number)
        {
            var result = new List<PatientOverviewModel>();

            if (_hostingEnvironment.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(number))
                {
                    string sql = GetSqlText("home-patient-overview-sql.txt");
                    var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",number)
                 };
                    result = dbgp.SqlQueryable<PatientOverviewModel>(sql).AddParameters(parameters).ToList();
                }
            }
            else
            {
                result.Add(new PatientOverviewModel { EMPINumber = "12345678", Address = "成都市龙泉驿区大面镇银河路118号恒大绿洲", CardNo = "511025196903220551", Gender = "男", Name = "叶问", PhoneNumber = "13550330299" });

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
                    string sql = GetSqlText("home-patient-visit-list-sql.txt");
                    var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",number)
                };
                    patientVisitList = dbgp.SqlQueryable<PatientVisitInfoModel>(sql).AddParameters(parameters).ToList();
                }  
            }
            else
            {
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-09-03", VisitType = "住院", DiagDesc = "多巴反应性肌张力障碍", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-08-03", VisitType = "门诊", DiagDesc = "震颤", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2020-04-13", VisitType = "住院", DiagDesc = "肝豆状核变性", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2019-07-22", VisitType = "住院", DiagDesc = "发烧", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2019-02-21", VisitType = "住院", DiagDesc = "铁沉积性疾病", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-11-13", VisitType = "门诊", DiagDesc = "流行性感冒", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-09-15", VisitType = "门诊", DiagDesc = "发烧", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2018-19-03", VisitType = "住院", DiagDesc = "多系统萎缩", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2016-02-12", VisitType = "住院", DiagDesc = "脊髓小脑性共济失调", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2015-09-21", VisitType = "住院", DiagDesc = "运动迟缓", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2015-01-03", VisitType = "门诊", DiagDesc = "肌无力", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2014-05-05", VisitType = "门诊", DiagDesc = "肌无力", Center = "华西医院" });
                patientVisitList.Add(new PatientVisitInfoModel { VisitTime = "2024-01-01", VisitType = "住院", DiagDesc = "流行性感冒", Center = "华西医院" });
            }
           
            return patientVisitList;
        }

        public string GetPatientEMRText(string patientEmpiId)
        {
            var result = string.Empty;
            if (_hostingEnvironment.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(patientEmpiId))
                {
                    string sql = GetSqlText("home-patient-EMR-sql.txt");
                    var parameters = new List<SugarParameter>(){
                    new SugarParameter("@number",patientEmpiId)
                   };
                    //var result = dbgp.Ado.GetString(sql);
                    var entity = dbgp.SqlQueryable<PatientEMRModel>(sql).AddParameters(parameters).ToList();
                    var list = entity.Select(x => x.Detail);
                    result = string.Join(' ', entity);
                }
            }
            else
            {
                result = @"患者李**，男性，67岁，主因“右侧肢体抖动、僵硬、动作不灵活7年，累及左侧5年”，来我院门诊就诊。患者7年前无明显诱因出现右手不自主抖动，以安静状态下明显，紧张、激动时加重，平静放松后减轻，睡眠后消失；伴右侧肢体活动不灵活、僵硬，如写字慢、越写越小。症状逐渐加重，波及右下肢。5年前左侧肢体亦出现上述症状，切菜、系扣等动作慢。走路慢，小碎步，起床迈步转身费力，呈弯腰驼背姿势，症状缓慢加重。5年前开始口服美多巴，上述症状明显改善。但2年前因逐渐出现药效减退，患者自行将药量逐渐增加250mg 4id。约半年前开始出现服药2-3小时后肢体不自主扭动表现，且一天之中上述症状波动明显。发病以来便秘明显，睡眠差。发病以来否认站立头晕、吞咽困难、饮水呛咳、平衡障碍。既往：无构音障碍、CO中毒史、脑炎病史、重金属中毒史、农药中毒史、脑出血脑梗塞病史，家族中有类似疾病患者，可能有常染色体隐性遗传，无长期大量应用D2受体阻滞剂、多巴胺耗竭剂病史。专科查体：体温：36.5℃，呼吸：18次/分，脉搏：76次/分。神志清楚，面具脸，流涎较多、颜面躯干皮脂分泌增多。平卧血压120/ 80 mmHg，立位血压120/ 80mmHg。颅神经检查：双眼各向活动无障碍，无复视。面部感觉对称正常，咬肌、颞肌有力，张口下颌不偏。双侧闭目有力、示齿口角无偏斜。伸舌居中。躯体深浅感觉对称正常。慌张步态，行走时躯干前屈，双上臂无伴随动作。四肢肌力V级，四肢肌张力高，呈齿轮样强直，右侧重于右侧。肌肉无明显萎缩。双侧肢体3~5Hz粗大搓丸样静止性震颤，小写征明显。指鼻试验、跟膝胫试验稳准。肱二头肌、膝腱反射无明显亢进，双侧Hoffmann征、Babinski征阴性。颈部僵硬。双侧Kernig’ s sign (-) 。辅助检查：头颅MRI平扫未见明显异常。";

            }
            return result;
            // return "";
        }

        public List<RareDiseaseDetailModel> SearchRareDiseaseList(string searchText)
        {
            var result = new List<RareDiseaseDetailModel>();
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(searchText))
            {
                string sql = GetSqlText("raredetail-search-disease-sql.txt");
                var parameters = new List<SugarParameter>(){
                  new SugarParameter("@DiseaseText",searchText)
                };
                result = dbgp.SqlQueryable<RareDiseaseDetailModel>(sql).AddParameters(parameters).ToList();
            }
          
            return result;
        }


        public List<HPODataModel> GetAnalyzeHPOResult(string patientEmpiId)
        {
            var result = new List<HPODataModel>();
            if (_hostingEnvironment.IsProduction() && !string.IsNullOrWhiteSpace(patientEmpiId))
            {
                string sql = GetSqlText("home-get-hpo-result-sql.txt");
                var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",patientEmpiId)
                };
                result = dbgp.SqlQueryable<HPODataModel>(sql).AddParameters(parameters).ToList();
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
