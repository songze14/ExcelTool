using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Excel.Tool
{
    /// <summary>
    /// Excel转位实体类 实体类需要加入ExcelToEntity特性
    /// </summary>
    public static class ExcelToEntity
    {

        private static ExcelWorksheet worksheet;
        private static List<PropertyInfo> excelproperlist;
        private static List<PropertyInfo> properlists;
        private static string[] truevaluestr = { "是", "能", "可以", "1", "完成" };
        private static string[] falsevaluestr = { "否", "不能", "不可以", "0", "未完成" };
        /// <summary>
        /// 工作区转集合
        /// </summary>
        /// <param name="filestream">文件流</param>
        /// <param name="worksheetindex">工作区</param>
        /// <param name="openrow">开始行</param>
        /// <param name="opencol">开始列,如果开始列为0,则使用类中的特性</param>
        /// <param name="endcol">结束列</param>
        /// <param name="endrow">结束行 默认全文</param>
        /// <returns></returns>
        public static List<T> WorksheetToDataRow<T>(Stream filestream, int worksheetindex, int openrow, int opencol, int endcol, int endrow = 0)
        {
            try
            {
                //判断文件是否为空
                if (filestream.Length < 1)
                {
                    throw new Exception("文件不能为空");
                }
                //读取文件流
                ExcelPackage package = new ExcelPackage(filestream);
                //获取sheet表
                ExcelWorksheets worksheets = package.Workbook.Worksheets;
                worksheetindex = worksheetindex - 1;
                worksheet = worksheets[worksheetindex];

                if (endrow == 0)
                {
                    endrow = worksheet.Dimension.End.Row;
                }

                var ty = typeof(T);
                properlists = new List<PropertyInfo>();
                GetAllAttribute(ty);
                //查询带有excel特性的属性
                excelproperlist = properlists.Where(o => o.GetCustomAttributesData().Where(o => o.AttributeType == typeof(ExcelToEntityAttribute)).Count() > 0).ToList();
                var requestlist = new List<T>();
                //如果开始列为0,则使用类中的特性
                if (opencol != 0)
                {
                    FillList(openrow, opencol, endrow, endcol, in requestlist);
                }
                else
                {
                    //取类上的自定义特性
                    var classattris = excelproperlist.Select(o => o.GetCustomAttribute<ExcelToEntityAttribute>()).ToList();
                    FillList(openrow, opencol, endrow, classattris.Count, in requestlist, classattris);


                }

                package.Dispose();
                return requestlist;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        /// <summary>
        /// 实体转excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excelWorkbook">excel表格对象</param>
        /// <param name="data">需要写入表格的数据</param>
        /// <returns></returns>
        public static ExcelPackage ListToExcek<T>(ExcelPackage excelPack, string sheelname, int openrow, IEnumerable<T> data)
        {
            try
            {

                var sheel = excelPack.Workbook.Worksheets.Add(sheelname);
                var ty = typeof(T);
                var properlist = ty.GetProperties();
                //查询带有excel特性的属性
                var excelproperlist = properlist.Where(o => o.GetCustomAttributesData().Where(o => o.AttributeType == typeof(EntityToexcelAttribute)).Count() == 1).ToList();

                //取类上的自定义特性

                var classattri = excelproperlist.Select(o => ty.GetProperty(o.Name).GetCustomAttribute<EntityToexcelAttribute>()).OrderBy(o => o.Idx).ToList();
                int datacol = openrow + 1;
                //加入表头
                for (int i = 0; i < classattri.Count; i++)
                {
                    sheel.Cells[openrow, i + 1].Value = classattri[i].Colname;
                }

                var dd = excelproperlist[0];

                var ss = dd.CustomAttributes.Where(o => o.AttributeType == typeof(EntityToexcelAttribute)).FirstOrDefault();

                //根据list集合数据,填充excel
                foreach (var item in data)
                {
                    if (item == null)
                    {
                        datacol++;
                        continue;
                    }
                    for (int i = 0; i < classattri.Count; i++)
                    {

                        var proper = excelproperlist.Where(o => Convert.ToInt32(o.CustomAttributes.Where(o => o.AttributeType == typeof(EntityToexcelAttribute)).FirstOrDefault().ConstructorArguments[1].Value) == classattri[i].Idx).FirstOrDefault();
                        try
                        {
                            //读取出错,加入空值
                            var celldata = proper.GetValue(item);
                            object value = null;
                            if (celldata != null)
                            {
                                if (celldata.GetType() == typeof(bool))
                                {
                                    value = Convert.ToInt32(celldata);
                                }
                                else
                                {
                                    value = celldata;
                                }
                                sheel.Cells[datacol, i + 1].Value = value.ToString();
                            }
                        }
                        catch (Exception)
                        {

                            sheel.Cells[datacol, i + 1].Value = "";
                        }



                    }
                    datacol++;
                }
                sheel.Cells[sheel.Dimension.Address].AutoFitColumns();
                sheel.Cells[sheel.Dimension.Address].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(191, 191, 191));

            }
            catch (Exception ex)
            {


            }
            return excelPack;
        }
        /// <summary>
        /// 填充返回的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="openrow">开始行</param>
        /// <param name="endrow">结束行</param>
        /// <param name="opencol">开始列</param>
        /// <param name="endcol">结束列</param>
        /// <param name="requestlist">返回的数据集合</param>
        /// <param name="classattris">目标类型中自定义特性集合</param>
        /// <exception cref="Exception"></exception>
#nullable enable
        private static void FillList<T>(int openrow, int opencol, int endrow, int endcol, in List<T> requestlist, List<ExcelToEntityAttribute>? classattris = null)
        {

            //行
            for (int i = openrow; i <= endrow; i++)
            {
                var good = Activator.CreateInstance<T>();
                //列
                for (int j = 0; j < endcol; j++)
                {
                    try
                    {
                        int cellcol;
                        if (classattris != null)
                        {
                            cellcol = classattris[j].ColNo;
                        }
                        else
                        {
                            cellcol = j + 1;
                        }
                        object value = new object();
                        var range = worksheet.MergedCells[i, cellcol];
                        if (range == null)
                        {
                            value = worksheet.Cells[i, cellcol].Value;
                        }
                        else
                        {
                            value = worksheet.Cells[new ExcelAddress(range).Start.Row, new ExcelAddress(range).Start.Column].Value;
                        }
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            continue;
                        }
                        Type valueType = excelproperlist[j].PropertyType;

                        if (valueType.IsEnum)
                        {
                            if (!Enum.TryParse(valueType, value.ToString(), out value))
                            {
                                throw new Exception("枚举数据无法转移");
                            }
                        }
                        else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var genericTypes = valueType.GenericTypeArguments;
                            if (value == null)
                            {
                                excelproperlist[j].SetValue(good, null);
                            }
                            else if (genericTypes.Where(o => o.IsEnum).Count() > 0)
                            {
                                if (!Enum.TryParse(genericTypes[0], value.ToString(), out value))
                                {
                                    throw new Exception("枚举数据无法转移");
                                }
                            }
                            else if (genericTypes.Where(o => o == typeof(bool)).Count() > 0)
                            {
                                ///boolean值单独处理，符合条件赋值
                                var valuestr = value.ToString();
                                if (truevaluestr.Contains(valuestr))
                                {
                                    value = true;
                                }
                                else if (falsevaluestr.Contains(valuestr))
                                {
                                    value = false;
                                }
                            }
                            else
                            {
                                value = Convert.ChangeType(value, valueType.GetGenericArguments()[0]);
                            }
                        }
                        else if (valueType.Name == "DateTime")
                        {
                            if (value is double)
                            {
                                value = DateTime.FromOADate((double)value);
                            }
                        }
                        else
                        {
                            if (valueType == typeof(bool))
                            {
                                ///boolean值单独处理，符合条件赋值
                                var valuestr = value.ToString();
                                if (truevaluestr.Contains(valuestr))
                                {
                                    value = true;
                                }
                                if (falsevaluestr.Contains(valuestr))
                                {
                                    value = false;
                                }
                            }
                            value = Convert.ChangeType(value, valueType);
                        }
                        if (value != null)
                        {


                            excelproperlist[j].SetValue(good, value);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw new Exception($"转换excel出错{ex.Message} ;出错行 {i} :出错列{excelproperlist[j].Name}");
                    }

                }
                requestlist.Add(good);

            }

        }
        /// <summary>
        /// 获取目标类型信息中全部自定义特性
        /// </summary>
        /// <param name="infos">目标信息</param>
        private static void GetAllAttribute(Type type)
        {

            var properlist = type.GetProperties();
            foreach (var item in properlist)
            {

                var itemty = item.PropertyType;
                if (itemty.Namespace != null && !itemty.Namespace.Contains("System"))
                {
                    GetAllAttribute(itemty);
                }

                //判断是否为泛型
                if (itemty.IsGenericType)
                {

                    //获取泛型的T
                    var tys = itemty.GenericTypeArguments;
                    foreach (var ty in tys)
                    {

                        //判断是否为用户自定义类型
                        if (ty.Namespace != null && !ty.Namespace.Contains("System"))
                        {

                            GetAllAttribute(ty);
                        }
                    }

                }

            }
            properlists.AddRange(properlist);
        }
    }
}
