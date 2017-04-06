﻿using Pharos.Logic.Entity;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Pharos.Logic.BLL
{
    public class ProductBrandService : BaseService<Entity.ProductBrand>
    {
        public static List<Entity.ProductBrand> GetList(bool isAll = false)
        {
            if (isAll)
            {
                var all = FindList(o=>o.CompanyId==CommonService.CompanyId);
                all.ForEach(a => { if (a.State == 0) a.Title = "*" + a.Title; });
                return all.OrderByDescending(a => a.State).ThenBy(a => a.Title).ToList();
            }
            return FindList(o => o.State == 1 && o.CompanyId == CommonService.CompanyId).OrderBy(o => o.Title).ToList();
        }
        public static IEnumerable<ProductBrand> GetAllProductBrands(short? state = 1)
        {
            var list = Pharos.Utility.DataCache.Get<List<ProductBrand>>("allbrands");
            if (list == null)
            {
                if (state.HasValue)
                    list = CurrentRepository.FindList(o => o.State == state && o.CompanyId == CommonService.CompanyId).ToList();
                else
                    list = CurrentRepository.FindList(o => o.CompanyId == CommonService.CompanyId).ToList();
                //Pharos.Utility.DataCache.Set("allbrands", list, 3);
            }
            return list;
        }
        public static Dictionary<string, int> GetListByProduct(int subCate)
        {
            var query = BaseService<VwProduct>.CurrentRepository.QueryEntity.Where(o=>o.CompanyId==CommonService.CompanyId);
            var categorys = query.Where(o => o.BrandSN > 0 && o.CategorySN == subCate).Select(o => new { o.BrandSN, o.BrandTitle }).Distinct();
            return categorys.ToDictionary(o => o.BrandTitle, o => o.BrandSN.GetValueOrDefault());
        }
        public static OpResult Import(ImportSet obj, System.Web.HttpFileCollectionBase httpFiles, string fieldName, string columnName)
        {
            var op = new OpResult();
            var errLs = new List<string>();
            int count = 0;
            try
            {
                Dictionary<string, char> fieldCols = null;
                DataTable dt = null;
                op = ImportSetService.ImportSet(obj, httpFiles, fieldName, columnName, ref fieldCols, ref dt);
                if (!op.Successed) return op;
                var brandClass = SysDataDictService.FindList(o => o.DicPSN == (int)DicType.品牌分类 && o.CompanyId == CommonService.CompanyId);
                var otherClass= brandClass.FirstOrDefault(o => o.Title.StartsWith("其"));
                var brands= GetAllProductBrands(null).ToList();
                var max = SysDataDictService.GetMaxSN;
                var clsIdx = Convert.ToInt32(fieldCols["ClassifyId"]) - 65;
                var titleIdx = Convert.ToInt32(fieldCols["Title"]) - 65;
                count = dt.Rows.Count;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var dr = dt.Rows[i];
                        var text = dr[clsIdx].ToString();
                        if (text.IsNullOrEmpty()) continue;
                        var cls = brandClass.FirstOrDefault(o => o.Title == text);
                        if (cls != null)
                        {
                            dr[clsIdx] = cls.DicSN.ToString();
                        }
                        else
                        {
                            if (obj.RefCreate)
                            {
                                var data = new Sys.Entity.SysDataDictionary()
                                {
                                    DicPSN = (int)DicType.品牌分类,
                                    DicSN = max++,
                                    Status = true,
                                    Title = text,
                                    CompanyId=CommonService.CompanyId
                                };
                                SysDataDictService.Add(data);
                                brandClass.Add(data);
                                dr[clsIdx] = data.DicSN.ToString();
                            }
                            else if (otherClass != null)
                            {
                                dr[clsIdx] = otherClass.DicSN.ToString();
                            }
                            else
                            {
                                errLs.Add("品牌分类[" + text + "]不存在!");
                                dt.Rows.RemoveAt(i);//去除不导入
                                continue;
                            }
                        }
                        text=dr[titleIdx].ToString().Trim();
                        if (brands.Any(o => o.Title == text))
                        {
                            errLs.Add("品牌名称[" + text + "]已存在!");
                            dt.Rows.RemoveAt(i);//去除不导入
                        }
                        else
                        {
                            brands.Add(new ProductBrand() { Title = text });
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("品牌分类处理失败!", e);
                    }
                }
                max = MaxSN;
                StringBuilder sb = new StringBuilder();
                sb.Append("begin tran ");
                foreach (DataRow dr in dt.Rows)
                {
                    sb.Append("insert into ");
                    sb.Append(obj.TableName);
                    sb.Append("(CompanyId,BrandSN,State,");
                    sb.Append(string.Join(",", fieldCols.Keys));
                    sb.Append(") values(");
                    sb.AppendFormat("{0},", obj.CompanyId);
                    sb.AppendFormat("{0},", max++);
                    sb.AppendFormat("1,");
                    foreach (var de in fieldCols)
                    {
                        var index = Convert.ToInt32(de.Value) - 65;
                        try
                        {
                            var text = dr[index].ToString();
                            sb.Append("'" + text + "',");
                        }
                        catch (Exception e)
                        {
                            throw new Exception("列选择超过范围!",e);
                        }
                    }
                    sb= sb.Remove(sb.Length - 1, 1);
                    sb.Append(");");
                }
                sb.Append(" commit tran");
                op.Successed= new Pharos.Logic.DAL.CommonDAL()._db.ExecuteNonQueryText(sb.ToString(), null)>0;
                Log.WriteInsert("品牌导入", Sys.LogModule.档案管理);
            }
            catch(Exception ex)
            {
                op.Message = ex.Message;
                op.Successed = false;
                Log.WriteError(ex);
                errLs.Add("导入出现异常!");
            }
            return CommonService.GenerateImportHtml(errLs, count);
        }
        public static int MaxSN
        {
            get
            {
                int max = 0;
                try
                {
                    max = CurrentRepository.QueryEntity.Max(o => o.BrandSN);
                }
                catch { max = 0; }
                return max + 1;
            }
        }
    }
}