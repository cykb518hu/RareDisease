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
        DiseaseHPOSummaryBarModel GetDiseaseHPOSummaryBar(string name, string hideHpoStr, int nlpMinCount);
    }

    public class ExcelRepository: IExcelRepository
    {
        private readonly IConfiguration _config;
        private readonly ILocalMemoryCache _localMemoryCache;
        public ExcelRepository(IConfiguration config, ILocalMemoryCache localMemoryCache)
        {
            _config = config;
            _localMemoryCache = localMemoryCache;
        }
        public DiseaseHPOSummaryBarModel GetDiseaseHPOSummaryBar(string name, string hideHpoStr, int nlpMinCount)
        {
            name = name.Trim();
            var summaryBar = new DiseaseHPOSummaryBarModel();
            var result = new DiseaseHPOSummaryModel();
            result.HPOList = new List<DiseaseHPOSummaryHPOModel>();
            var fileName = _config.GetValue<string>("GlobalSetting:HPOExcelPath");
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
              
                var mapping = new DiseaseHPOSummaryLibraryMappingModel();
                //获取当前疾病和知识库的关系
                var librarySheet= package.Workbook.Worksheets[2];
                int row = librarySheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    if (librarySheet.Cells[i, 1].Value.ToString().Trim() == name)
                    {
                        result.DiseaseName = name;
                        result.CasesCount = Convert.ToInt32(librarySheet.Cells[i, 3].Value.ToString());
                        mapping.ChineseName = name;
                        mapping.CasesCount = librarySheet.Cells[i, 3].Value.ToString();
                        mapping.EramId = librarySheet.Cells[i, 4].Value.ToString();
                        mapping.OMIMId = librarySheet.Cells[i, 5].Value.ToString();
                        mapping.ORPHAId = librarySheet.Cells[i, 6].Value.ToString();
                        break;
                    }
                }
                //获取疾病所有case 和hpo
                if (!_config.GetValue<bool>("GlobalSetting:UseCache"))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    row = workSheet.Dimension.Rows;
                    for (int i = 2; i <= row; i++)
                    {
                        if (workSheet.Cells[i, 2].Value != null && workSheet.Cells[i, 2].Value.ToString().Trim() == name)
                        {
                            result.DiseaseName = name;
                            result.CasesCount++;

                            //NLP hpo
                            if (workSheet.Cells[i, 3].Value != null)
                            {
                                var arr = workSheet.Cells[i, 3].Value.ToString().Split(",");
                                foreach (var r in arr)
                                {
                                    var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == r);
                                    if (hpo != null)
                                    {
                                        hpo.NlpCount++;
                                    }
                                    else
                                    {
                                        result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = r, NlpCount = 1 });
                                    }
                                }
                            }
                            //检验 hpo
                            if (workSheet.Cells[i, 4].Value != null)
                            {
                                var arr = workSheet.Cells[i, 4].Value.ToString().Split(",");
                                foreach (var r in arr)
                                {
                                    var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == r);
                                    if (hpo != null)
                                    {
                                        hpo.ExamCount++;
                                    }
                                    else
                                    {
                                        result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = r, ExamCount = 1 });
                                    }
                                }
                            }
                        }
                    }
                }

                //获取各个知识库
                var summarySheet = package.Workbook.Worksheets[3];
                row = summarySheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    if (summarySheet.Cells[i, 1].Value != null && summarySheet.Cells[i, 4].Value != null)
                    {
                        var hpoId = summarySheet.Cells[i, 4].Value.ToString();
                        var source = summarySheet.Cells[i, 1].Value.ToString().ToUpper();
                      
                        hpoId = string.IsNullOrEmpty(hpoId) ? "" : hpoId.Trim();
                        var hpo = result.HPOList.FirstOrDefault(x => x.HPOId == hpoId);

                        if(_config.GetValue<bool>("GlobalSetting:UseCache")&& summarySheet.Cells[i, 2].Value!=null)
                        {
                            var diseaseName = summarySheet.Cells[i, 2].Value.ToString();
                            if (source == "EMR" && diseaseName == name)
                            {

                                var type = summarySheet.Cells[i, 5].Value.ToString();
                                var count = Convert.ToInt32(summarySheet.Cells[i, 6].Value.ToString());
                                if (hpo != null)
                                {
                                    if (type == "NLP")
                                    {
                                        hpo.NlpCount = count;
                                    }
                                    if (type == "LAB")
                                    {
                                        hpo.ExamCount = count;
                                    }
                                }
                                else
                                {
                                    hpo = new DiseaseHPOSummaryHPOModel();
                                    hpo.HPOId = hpoId;
                                    if (type == "NLP")
                                    {
                                        hpo.NlpCount = count;
                                    }
                                    if (type == "LAB")
                                    {
                                        hpo.ExamCount = count;
                                    }
                                    result.HPOList.Add(hpo);
                                }

                            }
                        }
                        if (summarySheet.Cells[i, 3].Value != null)
                        {
                            var linkedId = summarySheet.Cells[i, 3].Value.ToString();

                            if (source == "ERAM" && linkedId == mapping.EramId)
                            {
                                if (hpo != null)
                                {
                                    hpo.EramCount = -1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, EramCount = -1 });
                                }
                            }
                            if (source == "OMIM" && linkedId == mapping.OMIMId)
                            {

                                if (hpo != null)
                                {
                                    hpo.OMIMCount = -1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, OMIMCount = -1 });
                                }

                            }
                            if (source == "ORPHA" && linkedId == mapping.ORPHAId)
                            {
                                if (hpo != null)
                                {
                                    hpo.ORPHACount = -1;
                                }
                                else
                                {
                                    result.HPOList.Add(new DiseaseHPOSummaryHPOModel { HPOId = hpoId, ORPHACount = -1 });
                                }
                            }
                        }
                    }
                }
            }
            result.HPOList = result.HPOList.OrderByDescending(x => x.NlpCount).ThenByDescending(x=>x.ExamCount).ToList();

           
            var chpoList = from T1 in _localMemoryCache.GetCHPO2020StandardList()
                           join T2 in result.HPOList.Select(x => x.HPOId) on T1.HpoId equals T2
                           select new CHPO2020Model { NameChinese = T1.NameChinese, HpoId = T1.HpoId, NameEnglish = T1.NameEnglish };
            summaryBar.HPOItem = new List<CHPO2020Model>();
            var maxCount = result.HPOList.First().NlpCount;
            var ratio = maxCount / 15;
            foreach (var hpo in result.HPOList)
            {
                if (!string.IsNullOrWhiteSpace(hideHpoStr) && hideHpoStr.ToLower().Contains(hpo.HPOId.ToLower()))
                {
                    continue;
                }
                if (hpo.NlpCount < nlpMinCount && hpo.ExamCount == 0 && hpo.EramCount == 0 && hpo.OMIMCount == 0 && hpo.ORPHACount == 0)
                {
                    continue;
                }
                var chpo = chpoList.FirstOrDefault(x => x.HpoId == hpo.HPOId);
                if(chpo != null)
                {
                    if(string.IsNullOrWhiteSpace(chpo.NameChinese))
                    {
                        chpo.NameChinese = hpo.HPOId;
                    }
                    if (string.IsNullOrWhiteSpace(chpo.NameEnglish))
                    {
                        chpo.NameEnglish = hpo.HPOId;
                    }
                }
                else
                {
                    chpo = new CHPO2020Model { HpoId = hpo.HPOId, NameChinese = hpo.HPOId, NameEnglish = hpo.HPOId };
                }
                if (ratio > 0)
                {
                    if (hpo.EramCount == -1)
                    {
                        hpo.EramCount = hpo.EramCount * ratio;
                    }
                    if (hpo.OMIMCount == -1)
                    {
                        hpo.OMIMCount = hpo.OMIMCount * ratio;
                    }
                    if (hpo.ORPHACount == -1)
                    {
                        hpo.ORPHACount = hpo.ORPHACount * ratio;
                    }
                }
                hpo.Display = true;
                summaryBar.HPOItem.Add(chpo);
                
            }
            summaryBar.CasesCount = result.CasesCount;
            summaryBar.SeriesDataModel = new List<SeriesData>();
            result.HPOList = result.HPOList.Where(x => x.Display == true).ToList();
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "NLP命中", Value = result.HPOList.Select(x => x.NlpCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "检验命中", Value = result.HPOList.Select(x => x.ExamCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "eRAM命中", Value = result.HPOList.Select(x => x.EramCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "OMIM命中", Value = result.HPOList.Select(x => x.OMIMCount).ToList() });
            summaryBar.SeriesDataModel.Add(new SeriesData { Name = "Orphanet命中", Value = result.HPOList.Select(x => x.ORPHACount).ToList() });

            return summaryBar;
        }
        public List<DiseaseHPOSummaryDiseaseNameModel> GetDiseaseNameList()
        {
            //var result = new List<DiseaseHPOSummaryDiseaseNameModel>();
            //var fileName = _config.GetValue<string>("GlobalSetting:HPOExcelPath");
            //FileInfo file = new FileInfo(fileName);

            //using (ExcelPackage package = new ExcelPackage(file))
            //{
            //    var workSheet = package.Workbook.Worksheets[2];
            //    int row = workSheet.Dimension.Rows;
            //    for (int i = 2; i <= row; i++)
            //    {
            //        result.Add(new DiseaseHPOSummaryDiseaseNameModel { Value = workSheet.Cells[i, 1].Value.ToString(), Label = workSheet.Cells[i, 1].Value.ToString()+" | " + workSheet.Cells[i, 2].Value.ToString() });
            //    }
            //}
            return _localMemoryCache.SummaryDiseaseList();
        }
    }
}
