using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
