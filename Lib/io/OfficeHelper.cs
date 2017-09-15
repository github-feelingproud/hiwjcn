using System;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Lib.core;
using NPOI.XWPF.UserModel;
using Lib.helper;

namespace Lib.io
{
    /// <summary>
    ///Excel 的摘要说明
    /// </summary>
    public static class ExcelHelper
    {
        public static ICellStyle GetStyle(XSSFWorkbook workbook,
            short background,
            short color,
            bool BoldFont = false)
        {
            var style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.FillPattern = FillPattern.SolidForeground;
            style.BorderTop = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Black.Index;
            style.FillForegroundColor = background;

            var font = workbook.CreateFont();
            font.FontHeightInPoints = 14;
            if (BoldFont)
            {
                font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            }
            font.Color = color;
            style.SetFont(font);
            return style;
        }

        /// <summary>
        /// return File(bs, "application/vnd.ms-excel");
        /// </summary>
        public static byte[] ObjectToExcel<T>(List<T> list, string sheet_name = "sheet")
        {
            list = list ?? throw new Exception("参数为空");

            var table = new DataTable();
            table.TableName = sheet_name ?? throw new Exception($"{nameof(sheet_name)}不能为空");
            var props = typeof(T).GetProperties();
            foreach (var p in props)
            {
                table.Columns.Add(p.Name, typeof(string));
            }
            foreach (var x in list)
            {
                var data = props.Select(m => ConvertHelper.GetString(m.GetValue(x))).ToArray();
                table.Rows.Add(data);
            }

            return DataTableToExcel(table);
        }

        /// <summary>
        /// return File(bs, "application/vnd.ms-excel");
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static byte[] DataTableToExcel(DataTable tb)
        {
            tb = tb ?? throw new Exception($"无法把空{nameof(DataTable)}转成Excel");

            using (var ms = new MemoryStream())
            {
                var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = workbook.CreateSheet(ValidateHelper.IsPlumpString(tb.TableName) ? tb.TableName : "sheet");

                var style = GetStyle(workbook,
                    NPOI.HSSF.Util.HSSFColor.Red.Index, NPOI.HSSF.Util.HSSFColor.White.Index);

                for (int i = 0; i < tb.Rows.Count; ++i)
                {
                    var row = sheet.CreateRow(i);
                    for (int j = 0; j < tb.Columns.Count; ++j)
                    {
                        var cell = row.CreateCell(j);
                        var data = tb.Rows[i][j];
                        cell.SetCellValue(ConvertHelper.GetString(data));
                        cell.CellStyle = style;
                    }
                }

                workbook.Write(ms);
                workbook.Clear();
                tb.Clear();

                var bs = ms.ToArray();
                return bs;
            }
        }

        public static string GetCellValue(NPOI.SS.UserModel.ICell cell)
        {
            try
            {
                if (cell == null) { return string.Empty; }
                string value = string.Empty;
                switch (cell.CellType)
                {
                    case CellType.Blank: { value = string.Empty; } break;
                    case CellType.Boolean: { value = ConvertHelper.GetString(cell.BooleanCellValue); } break;
                    case CellType.Error: { value = ConvertHelper.GetString(cell.ErrorCellValue); } break;
                    case CellType.Formula: { value = ConvertHelper.GetString(cell); } break;
                    case CellType.Numeric: { value = ConvertHelper.GetString(cell.NumericCellValue); } break;
                    case CellType.String: { value = ConvertHelper.GetString(cell.StringCellValue); } break;
                    case CellType.Unknown:
                    default: { value = ConvertHelper.GetString(cell); } break;
                }
                return value;
            }
            catch (Exception e)
            {
                return $"获取cell数据异常：{e.Message}";
            }
        }

        private static List<List<string>> SheetToList(ISheet sheet)
        {
            sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));

            var list = new List<List<string>>();
            for (int i = 0; i < sheet.LastRowNum; ++i)
            {
                var row = sheet.GetRow(i);
                if (row == null) { continue; }
                var rowlist = new List<string>();
                for (int j = 0; j < row.LastCellNum; ++j)
                {
                    rowlist.Add(GetCellValue(row.GetCell(j)));
                }
                list.Add(rowlist);
            }
            return list;
        }

        public static List<List<List<string>>> ExcelToList(string path)
        {
            var list = new List<List<List<string>>>();
            if (!IOHelper.FileHelper.Exists(path)) { throw new ArgumentNullException(nameof(path)); }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var workbook = new XSSFWorkbook(stream);
                for (var i = 0; i < workbook.NumberOfSheets; ++i)
                {
                    var sheet = workbook.GetSheetAt(i);
                    list.Add(SheetToList(sheet));
                }
            }
            return list;
        }

        public static List<List<string>> ExcelToDataTable(string path)
        {
            throw new NotImplementedException();
        }
    }

    public static class WordHelper
    {
        public static byte[] CreateWord(Dictionary<string, string> paragraphs)
        {
            var doc = new XWPFDocument();
            try
            {            //创建段落对象
                paragraphs = paragraphs ?? new Dictionary<string, string>() { };
                paragraphs.Keys.ToList().ForEach(key =>
                {
                    XWPFParagraph paragraph = doc.CreateParagraph();
                    XWPFRun run = paragraph.CreateRun();
                    run.IsBold = true;
                    run.SetText(ConvertHelper.GetString(key));
                    run.SetText(ConvertHelper.GetString(paragraphs[key]));
                });
                using (var stream = new MemoryStream())
                {
                    doc.Write(stream);
                    var bs = stream.ToArray();
                    return bs;
                }
            }
            finally
            {
                doc.Close();
            }
        }
    }
}
