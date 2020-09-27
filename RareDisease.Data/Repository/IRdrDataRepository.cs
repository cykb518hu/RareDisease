using Microsoft.AspNetCore.Hosting;
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
        List<PatientEMRModel> GetPatientEMRText(string patientEmpiId);

        List<HPODataModel> SearchRareDiseaseList(string searchHPOText);

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

            string sql = GetSqlText("home-patient-overview-sql.txt");
            var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",number)
            };
            var result = dbgp.SqlQueryable<PatientOverviewModel>(sql).AddParameters(parameters).ToList();
            return result;
        }

        public List<PatientVisitInfoModel> GetPatientVisitList(string number)
        {

            string sql = GetSqlText("home-patient-visit-list-sql.txt");
            var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",number)
            };
            var result = dbgp.SqlQueryable<PatientVisitInfoModel>(sql).AddParameters(parameters).ToList();
            return result;
        }

        public List<PatientEMRModel> GetPatientEMRText(string patientEmpiId)
        {

            string sql = GetSqlText("home-patient-EMR-sql.txt");
            var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",patientEmpiId)
            };
            //var result = dbgp.Ado.GetString(sql);
            var result = dbgp.SqlQueryable<PatientEMRModel>(sql).AddParameters(parameters).ToList();
            return result;
           // return "";
        }

        public List<HPODataModel> SearchRareDiseaseList(string searchHPOText)
        {

            string sql = GetSqlText("home-search-disease-sql.txt");
            var parameters = new List<SugarParameter>(){
                  new SugarParameter("@DiseaseText",searchHPOText)
            };
            var result = dbgp.SqlQueryable<HPODataModel>(sql).AddParameters(parameters).ToList();
            return result;
        }

        public List<HPODataModel> GetAnalyzeHPOResult(string patientEmpiId)
        {

            string sql = GetSqlText("home-get-hpo-result-sql.txt");
            var parameters = new List<SugarParameter>(){
                  new SugarParameter("@number",patientEmpiId)
            };
            var result = dbgp.SqlQueryable<HPODataModel>(sql).AddParameters(parameters).ToList();
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
