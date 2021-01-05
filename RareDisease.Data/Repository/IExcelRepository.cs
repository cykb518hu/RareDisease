using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RareDisease.Data.Repository
{
    public interface IExcelRepository
    {
        List<DiseaseHPOSummaryDiseaseNameModel> GetDiseaseNameList();
        DiseaseHPOSummaryBarModel GetDiseaseHPOSummaryBar(string name);
    }

    public class ExcelRepository: IExcelRepository
    {
        private readonly IConfiguration _config;
        public ExcelRepository(IConfiguration config)
        {
            _config = config;
        }
        public DiseaseHPOSummaryBarModel GetDiseaseHPOSummaryBar(string name)
        {
            name = name.Trim();
            var summaryBar = new DiseaseHPOSummaryBarModel();
            var result = new DiseaseHPOSummaryModel();
            result.HPOList = new List<DiseaseHPOSummaryHPOModel>();
            var fileName = _config.GetValue<string>("GlobalSetting:HPOExcelPath");
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                //获取疾病所有case 和hpo
                var workSheet = package.Workbook.Worksheets[1];
                int row = workSheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    if (workSheet.Cells[i, 2].Value != null && workSheet.Cells[i, 2].Value.ToString().Trim() == name)
                    {
                        result.DiseaseName = name;
                        result.CasesCount++;

                        //NLP hpo
                        if (workSheet.Cells[i, 3].Value != null)
                        {
                            CaculateHPOCount(workSheet.Cells[i, 3].Value.ToString(), result);
                        }
                        //检验 hpo
                        if (workSheet.Cells[i, 4].Value != null)
                        {
                            CaculateHPOCount(workSheet.Cells[i, 4].Value.ToString(), result);
                        }
                    }
                }
                var mapping = new DiseaseHPOSummaryLibraryMappingModel();
                //获取当前疾病和知识库的关系
                var librarySheet= package.Workbook.Worksheets[2];
                row = librarySheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    if (librarySheet.Cells[i, 1].Value.ToString().Trim() == name)
                    {
                        mapping.ChineseName = name;
                        mapping.CasesCount = librarySheet.Cells[i, 3].Value.ToString();
                        mapping.EramId = librarySheet.Cells[i, 4].Value.ToString();
                        mapping.OMIMId = librarySheet.Cells[i, 5].Value.ToString();
                        mapping.ORPHAId = librarySheet.Cells[i, 6].Value.ToString();
                        break;
                    }
                }
                //获取eram hpo
                var eramSheet = package.Workbook.Worksheets[3];
                row = eramSheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    if (eramSheet.Cells[i, 1].Value.ToString().Trim() == mapping.EramId)
                    {
                        var hpoStr = eramSheet.Cells[i, 3].Value.ToString();
                        if (!string.IsNullOrEmpty(hpoStr))
                        {
                            var array = hpoStr.Split(";");
                            foreach (var arr in array)
                            {
                                var hpoId = arr.Split("|")[0];
                                var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == hpoId);
                                if (hpo != null)
                                {
                                    hpo.EramCount = 1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, EramCount = 1 });
                                }
                            }
                        }
                        break;
                    }
                }
                //获取omim_orphanet hpo
                var omimOrphanet = package.Workbook.Worksheets[4];
                row = omimOrphanet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    if (omimOrphanet.Cells[i, 1].Value != null && omimOrphanet.Cells[i, 2].Value != null && omimOrphanet.Cells[i, 5].Value != null)
                    {
                        if (omimOrphanet.Cells[i, 1].Value.ToString().Trim() == "OMIM" && omimOrphanet.Cells[i, 2].Value.ToString().Trim() == mapping.OMIMId)
                        {
                            var hpoId = omimOrphanet.Cells[i, 5].Value.ToString();
                            if (!string.IsNullOrEmpty(hpoId))
                            {
                                hpoId = hpoId.Trim();
                                var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == hpoId);
                                if (hpo != null)
                                {
                                    hpo.OMIMCount = 1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, OMIMCount = 1 });
                                }
                            }
                        }
                        if (omimOrphanet.Cells[i, 1].Value.ToString().Trim() == "ORPHA" && omimOrphanet.Cells[i, 2].Value.ToString().Trim() == mapping.ORPHAId)
                        {
                            var hpoId = omimOrphanet.Cells[i, 5].Value.ToString();
                            if (!string.IsNullOrEmpty(hpoId))
                            {
                                hpoId = hpoId.Trim();
                                var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == hpoId);
                                if (hpo != null)
                                {
                                    hpo.ORPHACount = 1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, ORPHACount = 1 });
                                }
                            }
                        }
                    }
                }
            }
            result.HPOList = result.HPOList.OrderByDescending(x => x.MatchedCount).ToList();
            summaryBar.HPOId = result.HPOList.Select(x => x.HPOId).ToList();
            summaryBar.SeriesDataModel = new List<SeriesData>();
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "命中HPO", Value = result.HPOList.Select(x=>x.MatchedCount).ToList()});
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "eRAM命中", Value = result.HPOList.Select(x => x.EramCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "OMIM命中", Value = result.HPOList.Select(x => x.OMIMCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "Orphanet命中", Value = result.HPOList.Select(x => x.ORPHACount).ToList() });

            return summaryBar;
        }
        public void CaculateHPOCount(string text, DiseaseHPOSummaryModel result)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var arr = text.Split(",");
                foreach (var r in arr)
                {
                    var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == r);
                    if (hpo != null)
                    {
                        hpo.MatchedCount++;
                    }
                    else
                    {
                        result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = r, MatchedCount = 1 });
                    }
                }
            }
        }

        public List<DiseaseHPOSummaryDiseaseNameModel> GetDiseaseNameList()
        {
            var result = new List<DiseaseHPOSummaryDiseaseNameModel>();
            var fileName = _config.GetValue<string>("GlobalSetting:HPOExcelPath");
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets[2];
                int row = workSheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    result.Add(new DiseaseHPOSummaryDiseaseNameModel { Value = workSheet.Cells[i, 1].Value.ToString(), Label = workSheet.Cells[i, 1].Value.ToString()+"  " + workSheet.Cells[i, 2].Value.ToString() });
                }
            }
            return result;
        }
    }
}
