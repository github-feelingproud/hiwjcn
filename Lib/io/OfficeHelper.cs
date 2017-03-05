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
    public class ExcelHelper
    {
        public ExcelHelper()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        public static ICellStyle GetStyle(XSSFWorkbook workbook,
            short background,
            short color,
            bool BoldFont = false)
        {
            ICellStyle style = workbook.CreateCellStyle();
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

            IFont font = workbook.CreateFont();
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
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static byte[] ObjectToExcel<T>(List<T> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return null; }

            var table = new DataTable();
            var props = typeof(T).GetProperties();
            props.ToList().ForEach(p =>
            {
                table.Columns.Add(p.Name, typeof(string));
            });
            list.ForEach(x =>
            {
                var data = props.Select(m => ConvertHelper.GetString(m.GetValue(x))).ToArray();
                table.Rows.Add(data);
            });

            return DataTableToExcel(table);
        }

        /// <summary>
        /// return File(bs, "application/vnd.ms-excel");
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static byte[] DataTableToExcel(DataTable tb)
        {
            if (tb == null) { return null; }

            using (var ms = new MemoryStream())
            {
                var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = workbook.CreateSheet(ValidateHelper.IsPlumpString(tb.TableName) ? tb.TableName : "sheet");

                var style = GetStyle(workbook,
                    NPOI.HSSF.Util.HSSFColor.Red.Index, NPOI.HSSF.Util.HSSFColor.White.Index);

                NPOI.SS.UserModel.IRow row = null;
                NPOI.SS.UserModel.ICell cell = null;

                for (int i = 0; i < tb.Rows.Count; ++i)
                {
                    row = sheet.CreateRow(i);
                    for (int j = 0; j < tb.Columns.Count; ++j)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(ConvertHelper.GetString(tb.Rows[i][j]));
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
            catch
            {
                return string.Empty;
            }
        }

        public static List<List<string>> ExcelToList(string path)
        {
            var list = new List<List<string>>();
            if (!IOHelper.FileHelper.Exists(path)) { return list; }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);
                IRow row = null;
                if (sheet == null) { return list; }
                for (int i = 0; i < sheet.LastRowNum; ++i)
                {
                    if ((row = sheet.GetRow(i)) == null) { continue; }
                    var rowlist = new List<string>();
                    for (int j = 0; j < row.LastCellNum; ++j)
                    {
                        rowlist.Add(GetCellValue(row.GetCell(j)));
                    }
                    list.Add(rowlist);
                }
            }
            return list;
        }

        public static List<List<string>> ExcelToDataTable(string path)
        {
            return null;
        }
    }

    public class WordHelper
    {
        public static byte[] CreateWord(Dictionary<string, string> paragraphs)
        {
            try
            {
                XWPFDocument doc = new XWPFDocument();
                //创建段落对象
                if (!ValidateHelper.IsPlumpDict(paragraphs))
                {
                    return null;
                }
                paragraphs.Keys.ToList().ForEach(key =>
                {
                    XWPFParagraph paragraph = doc.CreateParagraph();
                    XWPFRun run = paragraph.CreateRun();
                    run.IsBold = true;
                    run.SetText(ConvertHelper.GetString(key));
                    run.SetText(ConvertHelper.GetString(paragraphs[key]));
                });
                MemoryStream stream = new MemoryStream();
                doc.Write(stream);
                return ConvertHelper.MemoryStreamToBytes(stream, true);
            }
            catch
            {
                return null;
            }
        }
    }
}
