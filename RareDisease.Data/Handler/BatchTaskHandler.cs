using OfficeOpenXml;
using RareDisease.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RareDisease.Data.Handler
{
    public class ExcelHpo
    {
        public string VisitNumber { get; set; }
        public string VisitId { get; set; }

        public string NlpHpoListStr { get; set; }
        public string ExamHpoListStr { get; set; }

        public string EMR { get; set; }
        public string Done { get; set; }
    }

    public class DatabaseHpo
    {
        public string Source { get; set; }
        public string Id { get; set; }

        public string Name { get; set; }
        public string HpoId { get; set; }
    }

    public class DiseaseHPOSummaryLibraryMappingModel
    {
        public string ChineseName { get; set; }

        public string EnglishName { get; set; }

        public string CasesCount { get; set; }

        public string EramId { get; set; }

        public string OMIMId { get; set; }

        public string ORPHAId { get; set; }

    }

    public class BatchTaskHandler
    {
        public static List<ExcelHpo> ReadVisitExcel(string fileName)
        {
            var result = new List<ExcelHpo>();
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets[1];
                int row = workSheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    var data = new ExcelHpo();
                    data.VisitNumber = workSheet.Cells[i, 1].Value.ToString();
                    result.Add(data);
                }
            }
            return result;
        }
        public static void UpdateVisitExcel(string fileName, List<ExcelHpo> list)
        {
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int row = workSheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    workSheet.Cells[i, 3].Value = list[i - 1].NlpHpoListStr;
                    workSheet.Cells[i, 4].Value = list[i - 1].ExamHpoListStr;
                    workSheet.Cells[i, 5].Value = list[i - 1].Done;
                }
                package.Save();
            }
        }

        public static void UpdateVisitEMRExcel(string fileName, List<ExcelHpo> list)
        {
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int row = workSheet.Dimension.Rows;
                for (int i = 1; i <= row; i++)
                {
                    workSheet.Cells[i, 3].Value = list[i - 1].Done;
                    workSheet.Cells[i, 5].Value = list[i - 1].EMR;
                }
                package.Save();
            }
        }

        public static void CombineExcelDatabase(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                var dlist = new List<DiseaseHPOSummaryLibraryMappingModel>();
                //获取当前疾病和知识库的关系
                var librarySheet = package.Workbook.Worksheets[1];

                int row = librarySheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    var mapping = new DiseaseHPOSummaryLibraryMappingModel();
                    mapping.ChineseName = librarySheet.Cells[i, 1].Value.ToString();
                    mapping.CasesCount = librarySheet.Cells[i, 3].Value.ToString();
                    mapping.EramId = librarySheet.Cells[i, 4].Value.ToString();
                    mapping.OMIMId = librarySheet.Cells[i, 5].Value.ToString();
                    mapping.ORPHAId = librarySheet.Cells[i, 6].Value.ToString();
                    dlist.Add(mapping);
                }

                ExcelWorksheet workSheet = package.Workbook.Worksheets[3];
                var tempid = "100050";
                row = workSheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    var source = workSheet.Cells[i, 1].Value.ToString().Trim();
                    var id = workSheet.Cells[i, 2].Value.ToString().Trim();
                   
                    if (id == tempid)
                    {
                        var tempdata = dlist.FirstOrDefault(x => x.OMIMId == id && source == "OMIM");
                    }
                    var data = dlist.FirstOrDefault(x => x.OMIMId == id && source == "OMIM");
                    if (data != null)
                    {
                        workSheet.Cells[i, 3].Value = data.ChineseName;
                    }
                    data = dlist.FirstOrDefault(x => x.ORPHAId== id && source == "ORPHA");
                    if (data != null)
                    {
                        workSheet.Cells[i, 3].Value = data.ChineseName;
                    }
                }
                package.Save();
            }

        }

        public static void CombineExcelDatabaseEram(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            var eramlIst = new List<DatabaseHpo>();
            using (ExcelPackage package = new ExcelPackage(file))
            {
                var dlist = new List<DiseaseHPOSummaryLibraryMappingModel>();
                //获取当前疾病和知识库的关系
                var librarySheet = package.Workbook.Worksheets[1];

                int row = librarySheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    var mapping = new DiseaseHPOSummaryLibraryMappingModel();
                    mapping.ChineseName = librarySheet.Cells[i, 1].Value.ToString();
                    mapping.CasesCount = librarySheet.Cells[i, 3].Value.ToString();
                    mapping.EramId = librarySheet.Cells[i, 4].Value.ToString();
                    mapping.OMIMId = librarySheet.Cells[i, 5].Value.ToString();
                    mapping.ORPHAId = librarySheet.Cells[i, 6].Value.ToString();
                    dlist.Add(mapping);
                }

                //获取eram hpo
                var eramSheet = package.Workbook.Worksheets[2];
                row = eramSheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    var name = "";
                    var data = dlist.FirstOrDefault(x => x.EramId == eramSheet.Cells[i, 1].Value.ToString().Trim());
                    if (data != null)
                    {
                        name = data.ChineseName;
                    }

                    var hpoStr = eramSheet.Cells[i, 3].Value.ToString();
                    if (!string.IsNullOrEmpty(hpoStr))
                    {
                        var array = hpoStr.Split(";");
                        foreach (var arr in array)
                        {
                            var item = new DatabaseHpo();
                            item.Name = name;
                            item.Id = eramSheet.Cells[i, 1].Value.ToString().Trim();
                            item.HpoId = arr.Split("|")[0];
                            item.Source = "eRam";
                            eramlIst.Add(item);
                        }
                    }
                }

                var wordSheet = package.Workbook.Worksheets[4];
                for (int i = 0; i < eramlIst.Count; i++)
                {
                    var data = eramlIst[i];
                    int j = i + 1;
                    wordSheet.Cells[j, 1].Value = data.Source;
                    wordSheet.Cells[j, 2].Value = data.Id;
                    wordSheet.Cells[j, 3].Value = data.Name;
                    wordSheet.Cells[j, 4].Value = data.HpoId;

                }
                package.Save();
            }

        }


        public static void CombineExcelHPO(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            var finalList = new List<DiseaseHPOSummaryModel>();
           
            using (ExcelPackage package = new ExcelPackage(file))
            {
                var dlist = new List<DiseaseHPOSummaryLibraryMappingModel>();
                //获取当前疾病和知识库的关系
                var librarySheet = package.Workbook.Worksheets[2];

                int row = librarySheet.Dimension.Rows;
                for (int i = 2; i <= row; i++)
                {
                    var mapping = new DiseaseHPOSummaryLibraryMappingModel();
                    mapping.ChineseName = librarySheet.Cells[i, 1].Value.ToString();
                    mapping.CasesCount = librarySheet.Cells[i, 3].Value.ToString();
                    mapping.EramId = librarySheet.Cells[i, 4].Value.ToString();
                    mapping.OMIMId = librarySheet.Cells[i, 5].Value.ToString();
                    mapping.ORPHAId = librarySheet.Cells[i, 6].Value.ToString();
                    dlist.Add(mapping);
                }

                //获取eram hpo
                //获取疾病所有case 和hpo
                var workSheet = package.Workbook.Worksheets[1];
                row = workSheet.Dimension.Rows;
                foreach (var item in dlist)
                {
                    var result = new DiseaseHPOSummaryModel();
                    result.HPOList = new List<DiseaseHPOSummaryHPOModel>();
                    var name = item.ChineseName;
                    for (int i = 2; i <= row; i++)
                    {
                        if (workSheet.Cells[i, 2].Value != null && workSheet.Cells[i, 2].Value.ToString().Trim() == name)
                        {
                            result.DiseaseName = name;
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
                    result.HPOList = result.HPOList.OrderByDescending(x => x.NlpCount).ThenByDescending(x => x.ExamCount).ToList();
                    finalList.Add(result);
                }
                var wordSheet = package.Workbook.Worksheets[3];
                int j = 1;
                foreach(var disease in finalList)
                {
                    foreach (var r in disease.HPOList)
                    {
                        wordSheet.Cells[j, 1].Value = "EMR";
                        wordSheet.Cells[j, 2].Value = disease.DiseaseName;
                        wordSheet.Cells[j, 4].Value = r.HPOId;
                        if (r.NlpCount > 0)
                        {
                            wordSheet.Cells[j, 5].Value = "NLP";
                            wordSheet.Cells[j, 6].Value = r.NlpCount;
                        }
                        if (r.ExamCount > 0)
                        {
                            if (r.NlpCount > 0)
                            {
                                j++;
                            }
                            wordSheet.Cells[j, 1].Value = "EMR";
                            wordSheet.Cells[j, 2].Value = disease.DiseaseName;
                            wordSheet.Cells[j, 4].Value = r.HPOId;
                            wordSheet.Cells[j, 5].Value = "LAB";
                            wordSheet.Cells[j, 6].Value = r.ExamCount;
                        }
                        j++;
                    }

                }
                package.Save();
            }

        }
    }
}
