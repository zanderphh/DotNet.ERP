﻿using Pharos.Logic.Entity;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data;
using Pharos.Sys.Entity;
using Newtonsoft.Json;

namespace Pharos.Logic.BLL
{
    public class PrivilegeOrderService:BaseService<PrivilegeSolution>
    {
        #region 返利方案
        /// <summary>
        /// 方案列表
        /// </summary>
        /// <param name="nvl"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static object SolutionList(NameValueCollection nvl, out int recordCount)
        {
            var mode = nvl["mode"].IsNullOrEmpty() ? -1 :short.Parse(nvl["mode"]);
            var supplierId = nvl["Supplier"];
            var text = nvl["SearchText"].Trim();
            var lamda = DynamicallyLinqHelper.Empty<PrivilegeSolution>().And(o => o.ModeSN == mode, mode == -1)
                .And(o =>o.Title.Contains(text), text.IsNullOrEmpty()).And(o=>o.SupplierIds.Contains(supplierId),supplierId.IsNullOrEmpty())
                .And(o=>o.CompanyId==CommonService.CompanyId);
            var querySolution = CurrentRepository.QueryEntity.Where(lamda);
            var queryProduct = ProductService.CurrentRepository.QueryEntity.Where(o=>o.CompanyId==CommonService.CompanyId);
            var queryCategory = ProductCategoryService.CurrentRepository.QueryEntity;
            var queryProd = BaseService<PrivilegeProduct>.CurrentRepository.QueryEntity;
            var queryRegVal = BaseService<PrivilegeRegionVal>.CurrentRepository.QueryEntity;
            var queryUser = UserInfoService.CurrentRepository.QueryEntity;
            var query = from a in querySolution
                        join b in queryUser on a.OperatorUID equals b.UID into temp
                        from x in temp.DefaultIfEmpty()
                        select new 
                        { 
                            a.Id,
                            a.Title,
                            Suppliers = a.SupplierIds,
                            a.ModeSN,
                            a.CreateDT,
                            a.OperatorUID,
                            x.FullName
                        };
            recordCount = query.Count();
            var list = query.ToPageList();
            var solIds = list.Select(o => o.Id).ToList();
            var prods = queryProd.Where(o => solIds.Contains(o.PrivilegeSolutionId)).ToList();
            var prodIds = prods.Select(o => o.Id).ToList();
            var prodvals = queryRegVal.Where(o => prodIds.Contains(o.PrivilegeProductId)).ToList();
            var barcodes = prods.Where(o => o.Type == 1).Select(o => o.BarcodeOrCategorySN).ToList();
            var categorySNs = prods.Where(o => o.Type == 2).Select(o => int.Parse(o.BarcodeOrCategorySN)).ToList();
            var products = barcodes.Count > 0 ? queryProduct.Where(o => barcodes.Contains(o.Barcode)).ToList() : new List<ProductRecord>();
            var categorys = categorySNs.Count > 0 ? queryCategory.Where(o => categorySNs.Contains(o.CategorySN)).ToList() : new List<ProductCategory>();
            var modes= SysDataDictService.GetDictionaryList(Logic.DicType.返利模式);
            var supplierIds = new List<string>();
            list.Each(o => {
                if (!o.Suppliers.IsNullOrEmpty())
                    supplierIds.AddRange(o.Suppliers.Split(','));
            });
            var suppliers = supplierIds.Count > 0 ? SupplierService.FindList(o => supplierIds.Contains(o.Id)) : new List<Supplier>();
            return list.Select(o => new { 
                o.Id,
                o.ModeSN,
                o.OperatorUID,
                o.FullName,
                o.Title,
                o.CreateDT,
                Mode=Mode(o.ModeSN,modes),
                Detail = Detail(o.Id, products, categorys, prods, prodvals),
                Suppliers = Suppliers(o.Suppliers,suppliers)
            });
        }
        public static PrivilegeSolution GetObj(int? id,bool addEmpty=true)
        {
            var obj = new PrivilegeSolution();
            if (id.HasValue)
            {
                obj = CurrentRepository.QueryEntity.Include("Regions.RegionVals").Include("Regions.RegionVals.ProductGifts").Include(o => o.Products).FirstOrDefault(o => o.Id == id.Value);
            }
            obj.Regions = obj.Regions ?? new List<PrivilegeRegion>();
            if (addEmpty)
            {
                for (int i = obj.Regions.Count; i < 5; i++)
                {
                    obj.Regions.Add(new PrivilegeRegion());
                }
            }
            return obj;
        }
        /// <summary>
        /// 保存方案
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="startVal">区间小值</param>
        /// <param name="endVal">区间大值</param>
        /// <returns></returns>
        public static OpResult SaveSolution(PrivilegeSolution obj,string startVal,string endVal)
        {
            var repRegion = BaseService<PrivilegeRegion>.CurrentRepository;
            var op = new OpResult();
            try
            {
                obj.SupplierIds = obj.SupplierIds.TrimStart(',');
                obj.CompanyId = CommonService.CompanyId;
                var regions = new List<PrivilegeRegion>();
                var starts = startVal.Split(',');
                var ends = endVal.Split(',');
                string nstr = "",ostr="";
                for (int i = 0; i < starts.Length; i++)
                {
                    if (starts[i].IsNullOrEmpty()) continue;
                    var sval = decimal.Parse(starts[i]);
                    PrivilegeRegion region = new PrivilegeRegion();
                    try
                    {
                        var eval = decimal.Parse(ends[i]);
                        region.StartVal = sval;
                        region.EndVal = eval;
                    }
                    catch
                    {
                        region.StartVal = sval;
                    }
                    region.PrivilegeSolutionId = obj.Id;
                    regions.Add(region);
                    nstr += region.ToString();
                }
                if (obj.Id == 0)
                {
                    obj.CreateDT = DateTime.Now;
                    obj.OperatorUID = Sys.CurrentUser.UID;
                    obj.Regions = regions;
                    op=Add(obj);
                }
                else
                {
                    var source = CurrentRepository.QueryEntity.Include("Regions.RegionVals").FirstOrDefault(o => o.Id == obj.Id);
                    obj.ToCopyProperty(source);
                    source.Regions.Each(o => { ostr += o.ToString(); });
                    if (nstr!= ostr)
                    {
                        source.Regions.Each(o => {
                            BaseService<PrivilegeRegionVal>.CurrentRepository.RemoveRange(o.RegionVals, false);
                        });
                        BaseService<PrivilegeRegion>.CurrentRepository.RemoveRange(source.Regions,false);
                        source.Regions.Clear();
                        source.Regions.AddRange(regions);
                    }
                    op = Update(source);
                }
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                Log.WriteError(ex);
            }
            return op;
        }
        /// <summary>
        /// 保存设定的商品
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static OpResult SaveDesign(PrivilegeSolution obj)
        {
            var op = new OpResult();
            try
            {
                var source = CurrentRepository.QueryEntity.Include("Regions").Include("Products.RegionVals").FirstOrDefault(o => o.Id == obj.Id);
                DataTable dt = null;
                if (!obj.InsertProducted.IsNullOrEmpty())//添加商品
                {
                    dt = obj.InsertProducted.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = new PrivilegeProduct();
                            proc.Type = 1;
                            proc.BarcodeOrCategorySN = dr["Barcode"].ToString();
                            proc.RegionVals = new List<PrivilegeRegionVal>();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("a")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val == "null") continue;
                                var regs = dc.ColumnName.Split('a');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var regVal = new PrivilegeRegionVal();
                                regVal.Region = reg;
                                regVal.Product = proc;
                                regVal.Value = decimal.Parse(val);
                                proc.RegionVals.Add(regVal);
                            }
                            source.Products.Add(proc);
                        }
                    }
                }
                if (!obj.InsertTypeed.IsNullOrEmpty())//添加系列
                {
                    dt = obj.InsertTypeed.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = new PrivilegeProduct();
                            proc.Type = 2;
                            proc.BarcodeOrCategorySN = dr["CategorySN"].ToString();
                            proc.RegionVals = new List<PrivilegeRegionVal>();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("a")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val == "null") continue;
                                var regs = dc.ColumnName.Split('a');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var regVal = new PrivilegeRegionVal();
                                regVal.Region = reg;
                                regVal.Product = proc;
                                regVal.Value = decimal.Parse(val);
                                proc.RegionVals.Add(regVal);
                            }
                            source.Products.Add(proc);
                        }
                    }
                }
                if (!obj.UpdateProducted.IsNullOrEmpty())
                {
                    dt = obj.UpdateProducted.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["Barcode"].ToString() && o.Type == 1);
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("a")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val=="null") continue;
                                var regs = dc.ColumnName.Split('a');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var regVal = proc.RegionVals.FirstOrDefault(o => o.PrivilegeRegionId == reg.Id);
                                if (regVal == null)
                                {
                                    regVal = new PrivilegeRegionVal();
                                    proc.RegionVals.Add(regVal);
                                }
                                regVal.Region = reg;
                                regVal.Product = proc;
                                regVal.Value = decimal.Parse(val);
                            }
                        }
                    }
                }
                if (!obj.UpdateTypeed.IsNullOrEmpty())
                {
                    dt = obj.UpdateTypeed.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["CategorySN"].ToString() && o.Type == 2);
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("a")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val == "null") continue;
                                var regs = dc.ColumnName.Split('a');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var regVal = proc.RegionVals.FirstOrDefault(o => o.PrivilegeRegionId == reg.Id);
                                if (regVal == null)
                                {
                                    regVal = new PrivilegeRegionVal();
                                    proc.RegionVals.Add(regVal);
                                }
                                regVal.Region = reg;
                                regVal.Product = proc;
                                regVal.Value = decimal.Parse(val);
                            }
                        }
                    }
                }
                if (!obj.DeleteProducted.IsNullOrEmpty())
                {
                    dt = obj.DeleteProducted.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["Barcode"].ToString() && o.Type == 1);
                            BaseService<PrivilegeRegionVal>.CurrentRepository.RemoveRange(proc.RegionVals, false);
                            BaseService<PrivilegeProduct>.CurrentRepository.Remove(proc, false);
                        }
                    }
                }
                if (!obj.DeleteTypeed.IsNullOrEmpty())
                {
                    dt = obj.DeleteTypeed.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["CategorySN"].ToString() && o.Type == 2);
                            BaseService<PrivilegeRegionVal>.CurrentRepository.RemoveRange(proc.RegionVals, false);
                            BaseService<PrivilegeProduct>.CurrentRepository.Remove(proc, false);
                        }
                    }
                }
                op = Update(source);
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                Log.WriteError(ex);
            }
            return op;
        }
        public static OpResult SaveDesign2(PrivilegeSolution obj)
        {
            var op = new OpResult();
            try
            {
                var source = CurrentRepository.QueryEntity.Include("Regions").Include("Products.RegionVals").Include("Products.RegionVals.ProductGifts").FirstOrDefault(o => o.Id == obj.Id);
                DataTable dt = null;
                if (!obj.InsertProducted.IsNullOrEmpty())//添加商品
                {
                    dt = obj.InsertProducted.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var barcode= dr["Barcode"].ToString();
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == barcode && o.Type==1);
                            if (proc == null)
                            {
                                proc = new PrivilegeProduct();
                                proc.Type = 1;
                                proc.BarcodeOrCategorySN = barcode;
                                proc.RegionVals = new List<PrivilegeRegionVal>();
                                source.Products.Add(proc);
                            }
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("b")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val=="null") continue;
                                var regs = dc.ColumnName.Split('b');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var vals = val.Split(',');
                                var regVal = proc.RegionVals.FirstOrDefault(o => o.PrivilegeRegionId == reg.Id)??new PrivilegeRegionVal();
                                if (regVal.ProductGifts != null && regVal.ProductGifts.Any())
                                {
                                    continue;
                                }
                                regVal.ProductGifts = new List<PrivilegeProductGift>();
                                proc.RegionVals.Add(regVal);
                                vals.Each(o =>
                                {
                                    var gift = regVal.ProductGifts.FirstOrDefault(i => i.Barcode == o.Split('~')[0]);
                                    if (gift != null) gift.GiftNumber = short.Parse(o.Split('~')[1]);
                                    else
                                        regVal.ProductGifts.Add(new PrivilegeProductGift()
                                        {
                                            Barcode = o.Split('~')[0],
                                            GiftNumber = short.Parse(o.Split('~')[1])
                                        });
                                });
                                regVal.Region = reg;
                                regVal.Product = proc;
                            }
                        }
                    }
                }
                if (!obj.InsertTypeed.IsNullOrEmpty())//添加系列
                {
                    dt = obj.InsertTypeed.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var categorySN= dr["CategorySN"].ToString();
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == categorySN && o.Type == 2);
                            if (proc == null)
                            {
                                proc = new PrivilegeProduct();
                                proc.Type = 2;
                                proc.BarcodeOrCategorySN = categorySN;
                                proc.RegionVals = new List<PrivilegeRegionVal>();
                                source.Products.Add(proc);
                            }
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (!dc.ColumnName.StartsWith("b")) continue;
                                var val = dr[dc].ToString();
                                if (val.IsNullOrEmpty() || val == "null") continue;
                                var regs = dc.ColumnName.Split('b');
                                var start = regs[1].IsNullOrEmpty() ? 0 : decimal.Parse(regs[1]);
                                var end = regs[2].IsNullOrEmpty() ? new Nullable<decimal>() : decimal.Parse(regs[2]);
                                var reg = source.Regions.FirstOrDefault(o => o.StartVal == start && o.EndVal == end);
                                var vals = val.Split(',');
                                var regVal = proc.RegionVals.FirstOrDefault(o => o.PrivilegeRegionId == reg.Id) ?? new PrivilegeRegionVal();
                                if (regVal.ProductGifts != null && regVal.ProductGifts.Any())
                                {
                                    continue;
                                }
                                regVal.ProductGifts = new List<PrivilegeProductGift>();
                                proc.RegionVals.Add(regVal);
                                vals.Each(o =>
                                {
                                    var gift = regVal.ProductGifts.FirstOrDefault(i => i.Barcode == o.Split('~')[0]);
                                    if (gift != null) gift.GiftNumber = short.Parse(o.Split('~')[1]);
                                    else
                                        regVal.ProductGifts.Add(new PrivilegeProductGift()
                                        {
                                            Barcode = o.Split('~')[0],
                                            GiftNumber = short.Parse(o.Split('~')[1])
                                        });
                                });
                                regVal.Region = reg;
                                regVal.Product = proc;
                            }
                        }
                    }
                }
                if (!obj.DeleteProducted.IsNullOrEmpty())
                {
                    dt = obj.DeleteProducted.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["Barcode"].ToString() && o.Type == 1);
                            if (proc == null) continue;
                            BaseService<PrivilegeRegionVal>.CurrentRepository.RemoveRange(proc.RegionVals, false);
                            BaseService<PrivilegeProduct>.CurrentRepository.Remove(proc, false);
                        }
                    }
                }
                if (!obj.DeleteTypeed.IsNullOrEmpty())
                {
                    dt = obj.DeleteTypeed.JsonToDataTable();
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            var proc = source.Products.FirstOrDefault(o => o.BarcodeOrCategorySN == dr["CategorySN"].ToString() && o.Type == 2);
                            if (proc == null) continue;
                            BaseService<PrivilegeRegionVal>.CurrentRepository.RemoveRange(proc.RegionVals, false);
                            BaseService<PrivilegeProduct>.CurrentRepository.Remove(proc, false);
                        }
                    }
                }
                op = Update(source);
            }
            catch (Exception ex)
            {
                op.Message = ex.Message;
                Log.WriteError(ex);
            }
            return op;
        }
        public static OpResult DeleteSolution(int[] ids)
        {
            var list = CurrentRepository.QueryEntity.Include("Regions").Include("Products.RegionVals").Include("Products.RegionVals.ProductGifts").Where(o => ids.Contains(o.Id)).ToList();
            return Delete(list);
        }
        public static string LoadTypeDetailJson(int? id,bool getVal=true)
        {
            if (!id.HasValue) return "";
            var obj = PrivilegeOrderService.GetObj(id, false);
            var dal = new Pharos.Logic.DAL.PrivilegeProductDAL();
            var dt=dal.GetProductGifts(id.Value);
            obj.Regions.Each(o =>
            {
                dt.Columns.Add("a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" + (o.EndVal.HasValue ? o.EndVal.GetValueOrDefault().ToAutoString() : ""),typeof(string));
                dt.Columns.Add("b" + o.StartVal.GetValueOrDefault().ToAutoString() + "b" + (o.EndVal.HasValue ? o.EndVal.GetValueOrDefault().ToAutoString() : ""), typeof(string));
            });
            if (getVal)
            {
                var bars = obj.Regions.SelectMany(o => o.RegionVals).SelectMany(o => o.ProductGifts).Select(o => o.Barcode).ToList();
                var products = ProductService.FindList(o => bars.Contains(o.Barcode));
                foreach (DataRow dr in dt.Rows)
                {
                    obj.Regions.Each(o =>
                    {
                        var regVal = o.RegionVals.FirstOrDefault(i => i.PrivilegeProductId == (int)dr["Id"]);
                        if (regVal != null)
                        {
                            var end = (o.EndVal.HasValue ? o.EndVal.GetValueOrDefault().ToAutoString() : "");
                            if (regVal.Value.HasValue)
                                dr["a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" + end] = regVal.Value.GetValueOrDefault().ToAutoString();
                            else if (regVal.ProductGifts.Any())
                            {
                                dr["a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" + end] = "<div style='color:orange'>赠品:</div>" + string.Join("<br/>", regVal.ProductGifts.Select(i => i.Barcode + " " + GetProductTitle(products, i.Barcode) + " " + i.GiftNumber + "件"));
                                dr["b" + o.StartVal.GetValueOrDefault().ToAutoString() + "b" + end] = string.Join(",", regVal.ProductGifts.Select(i => i.Barcode + "~" + i.GiftNumber));
                            }
                        }
                    });
                }
            }
            else
                dt.Rows.Clear();
            return JsonConvert.SerializeObject(dt);
        }
        public static string LoadProductDetailJson(int? id, bool getVal = true)
        {
            if (!id.HasValue) return "";
            var obj = PrivilegeOrderService.GetObj(id, false);
            var setproducts = obj.Products.Where(o => o.Type == 1);
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductCode");
            dt.Columns.Add("Barcode");
            dt.Columns.Add("Title");
            obj.Regions.Each(o =>
            {
                dt.Columns.Add("a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" +(o.EndVal.HasValue?o.EndVal.GetValueOrDefault().ToAutoString():""));
                dt.Columns.Add("b" + o.StartVal.GetValueOrDefault().ToAutoString() + "b" + (o.EndVal.HasValue ? o.EndVal.GetValueOrDefault().ToAutoString() : ""));
            });
            if (getVal)
            {
                var barcodes = setproducts.Select(o => o.BarcodeOrCategorySN).ToList();
                var bars = obj.Regions.SelectMany(o => o.RegionVals).SelectMany(o => o.ProductGifts).Select(o => o.Barcode);
                barcodes.InsertRange(0, bars);
                var products = ProductService.FindList(o => barcodes.Contains(o.Barcode));
                if (products.Any())
                {
                    var proIds = products.Select(o => o.Id);
                    var regIds = obj.Regions.Select(o => o.Id);
                    //var regVals = BaseService<PrivilegeRegionVal>.CurrentRepository.FindList(o => proIds.Contains(o.PrivilegeProductId) && regIds.Contains(o.PrivilegeRegionId));
                    foreach (var pro in setproducts)
                    {
                        var dr = dt.NewRow();
                        var product = products.FirstOrDefault(o => o.Barcode == pro.BarcodeOrCategorySN);
                        dr["ProductCode"] = product.ProductCode;
                        dr["Barcode"] = product.Barcode;
                        dr["Title"] = product.Title;
                        obj.Regions.Each(o =>
                        {
                            var regVal = o.RegionVals.FirstOrDefault(i => i.PrivilegeProductId == pro.Id);
                            if (regVal != null)
                            {
                                var end = (o.EndVal.HasValue ? o.EndVal.GetValueOrDefault().ToAutoString() : "");
                                if (regVal.Value.HasValue)
                                    dr["a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" + end] = regVal.Value.GetValueOrDefault().ToAutoString();
                                else if (regVal.ProductGifts.Any())
                                {
                                    dr["a" + o.StartVal.GetValueOrDefault().ToAutoString() + "a" + end] = "<div style='color:orange'>赠品:</div>" + string.Join("<br/>", regVal.ProductGifts.Select(i => i.Barcode + " " + GetProductTitle(products, i.Barcode) + " " + i.GiftNumber + "件"));
                                    dr["b" + o.StartVal.GetValueOrDefault().ToAutoString() + "b" + end] = string.Join(",", regVal.ProductGifts.Select(i => i.Barcode + "~" + i.GiftNumber));
                                }
                            }
                        });
                        dt.Rows.Add(dr);
                    }
                }
            }
            else
                dt.Rows.Clear();
            return JsonConvert.SerializeObject(dt);
        }
        public static List<PrivilegeSolution> GetSolutionList()
        {
            return FindList(o=>o.CompanyId==CommonService.CompanyId);
        }
        #endregion
        #region 返利计算
        public static object CalcList(NameValueCollection nvl, out int recordCount)
        {
            var state = nvl["State"].IsNullOrEmpty()?-1:short.Parse(nvl["State"]);
            var start = nvl["StartDate"].IsNullOrEmpty() ? new Nullable<DateTime>() : DateTime.Parse(nvl["StartDate"]);
            var end = nvl["EndDate"].IsNullOrEmpty() ? new Nullable<DateTime>() : DateTime.Parse(nvl["EndDate"]).AddDays(1);
            var text = nvl["SearchText"].Trim();
            var lamda = DynamicallyLinqHelper.Empty<PrivilegeCalc>().And(o => o.State == state, state == -1).And(o => o.CreateDT >= start, !start.HasValue).And(o => o.CreateDT < end, !end.HasValue)
                .And(o=>(o.PrivilegeSolutionTitle.Contains(text) || o.SupplierTitle.Contains(text)),text.IsNullOrEmpty()).And(o=>o.CompanyId==CommonService.CompanyId);
            var queryCalc = BaseService<PrivilegeCalc>.CurrentRepository.QueryEntity.Where(lamda);
            var queryUser = UserInfoService.CurrentRepository.QueryEntity;
            var query = from a in queryCalc
                        from b in queryUser
                        where a.OperatorUID == b.UID
                        select new
                        {
                            a.Id,
                            a.PrivilegeSolutionTitle,
                            a.PrivilegeModeSNTitle,
                            a.SupplierTitle,
                            a.StartDate,
                            a.EndDate,
                            a.TotalMoney,
                            a.PrivilegeNum,
                            a.State,
                            a.CreateDT,
                            a.OperatorUID,
                            b.FullName
                        };
            recordCount = query.Count();
            var list = query.ToPageList();
            return list.Select(a=>new{
                a.Id,
                a.PrivilegeSolutionTitle,
                a.PrivilegeModeSNTitle,
                a.SupplierTitle,
                a.StartDate,
                a.EndDate,
                a.TotalMoney,
                a.PrivilegeNum,
                a.State,
                a.CreateDT,
                a.OperatorUID,
                a.FullName,
                BetweenDate = a.StartDate.GetValueOrDefault().ToString("yyyy-MM-dd") + "至" + a.EndDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
                StateTitle=Enum.GetName(typeof(Pharos.Logic.PrivilegeState),a.State)
            });
        }
        public static OpResult SaveCalc(PrivilegeCalc obj)
        {
            var op = new OpResult();
            try
            {
                var calcs = DoCalc(obj);
                if (!calcs.Any())
                    op.Message = "没有相关数据!";
                else
                    op = BaseService<PrivilegeCalc>.AddRange(calcs);
            }
            catch(Exception ex)
            {
                op.Message = ex.Message;
                Log.WriteError(ex);
            }
            return op;
        }
        public static string GetCalc(int solId, DateTime start, DateTime end)
        {
            string str = "";
            try
            {
                var calcs = DoCalc(new PrivilegeCalc() { PrivilegeSolutionId = solId, StartDate = start, EndDate = end });
                if (!calcs.Any())
                    str = "没有相关数据!";
                else
                {
                    calcs.Each(o => { str += o.SupplierTitle + ":" + o.PrivilegeNum.ToString("f2") + ","; });
                    str = str.TrimEnd(',');
                }
            }
            catch (Exception ex)
            {
                str = "计算出现异常!";
                Log.WriteError(ex);
            }
            return str;
        }
        static List<PrivilegeCalc> DoCalc(PrivilegeCalc obj)
        {
            var objSol = CurrentRepository.QueryEntity.Include("Regions").Include("Products.RegionVals").FirstOrDefault(o => o.Id == obj.PrivilegeSolutionId);
            var sids = objSol.SupplierIds.Split(',');
            var start = obj.StartDate.GetValueOrDefault();
            var end = obj.EndDate.GetValueOrDefault().AddDays(1);
            IEnumerable<dynamic> query = null;
            if (objSol.ModeSN == 44 || objSol.ModeSN == 45 || objSol.ModeSN == 46)
            {
                var queryOrder = OrderService.CurrentRepository.QueryEntity;
                var queryDetail = BaseService<IndentOrderList>.CurrentRepository.QueryEntity;
                var queryProduct = BaseService<VwProduct>.CurrentRepository.QueryEntity;
                var querySupplier = SupplierService.CurrentRepository.QueryEntity;
                query = from b in queryDetail
                        join c in queryOrder on b.IndentOrderId equals c.IndentOrderId
                        join e in querySupplier on c.SupplierID equals e.Id
                        from d in queryProduct where b.Barcode == d.Barcode || (","+d.Barcodes+",").Contains(","+b.Barcode+",")
                        let o = from a in queryDetail where a.IndentOrderId == b.IndentOrderId && a.Nature == 1 && a.ResBarcode == b.Barcode select a
                        where c.State == 5 && !o.Any() && sids.Contains(c.SupplierID) && c.CreateDT >= start && c.CreateDT < end
                        select new
                        {
                            c.IndentOrderId,
                            b.Barcode,
                            d.Title,
                            d.CategorySN,
                            d.SubCategoryTitle,
                            d.MidCategoryTitle,
                            d.SubUnit,
                            OrderDate = c.CreateDT,
                            c.ReceivedDT,
                            b.IndentNum,
                            BuyPrice=b.Price,
                            SubTotal = b.Subtotal,
                            SupplierId=c.SupplierID,
                            SupplierTitle=e.Title
                        };
            }
            else
            {
                var queryOrder = BaseService<SaleOrders>.CurrentRepository.QueryEntity;
                var queryDetail = BaseService<SaleDetail>.CurrentRepository.QueryEntity;
                var queryProduct = BaseService<VwProduct>.CurrentRepository.QueryEntity;
                query = from b in queryDetail
                        join c in queryOrder on b.PaySN equals c.PaySN
                        from d in queryProduct where b.Barcode == d.Barcode || ("," + d.Barcodes + ",").Contains("," + b.Barcode + ",")
                        where sids.Contains(d.SupplierId) && c.CreateDT >= start && c.CreateDT < end
                        select new
                        {
                            IndentOrderId=c.PaySN,
                            b.Barcode,
                            d.Title,
                            d.CategorySN,
                            d.SubCategoryTitle,
                            d.MidCategoryTitle,
                            d.SubUnit,
                            OrderDate = c.CreateDT,
                            ReceivedDT=new Nullable<DateTime>(),
                            IndentNum=b.PurchaseNumber,
                            BuyPrice = b.ActualPrice,
                            SubTotal = b.PurchaseNumber * b.ActualPrice,
                            d.SupplierId,
                            d.SupplierTitle
                        };
            }
            var list= query.ToList();
            var sql = query.ToString();
            //var totals=list.GroupBy(o => o.SupplierTitle).Select(o =>new{Supplier=o.Key,Total=o.Sum(i=>i.SubTotal)});
            var calcs = new List<PrivilegeCalc>();
            var barcodes = new List<string>();//不重复计算
            var suppliers = list.GroupBy(o => new { o.SupplierId, o.SupplierTitle }).Select(o => o.Key).ToList();
            obj.CompanyId = CommonService.CompanyId;
            foreach (var supp in suppliers)
            {
                var orderIds = list.Where(o => o.SupplierTitle == supp.SupplierTitle).Select(o => o.IndentOrderId).Distinct();
                var calc = new PrivilegeCalc();
                obj.ToCopyProperty(calc);
                calc.CreateDT = DateTime.Now;
                calc.OperatorUID = Sys.CurrentUser.UID;
                calc.Details = new List<PrivilegeCalcDetail>();
                calc.Results = new List<PrivilegeCalcResult>();
                decimal nums = 0, total = 0;
                foreach (var oid in orderIds)
                {
                    foreach (var pro in objSol.Products.OrderBy(o=>o.Type))
                    {
                        var products = list;
                        if (pro.Type == 1)
                        {
                            products = list.Where(o => o.Barcode == pro.BarcodeOrCategorySN && o.SupplierTitle == supp.SupplierTitle && o.IndentOrderId==oid).ToList();
                            if (!products.Any()) continue;
                            barcodes.Add(pro.BarcodeOrCategorySN);//排除已算条码
                        }
                        else//类别
                        {
                            var categorys = ProductCategoryService.GetChildSNs(new List<int>() { int.Parse(pro.BarcodeOrCategorySN) }, true);
                            products = list.Where(o => categorys.Contains(o.CategorySN) && !barcodes.Contains(o.Barcode) && o.SupplierTitle == supp.SupplierTitle && o.IndentOrderId == oid).ToList();
                            if (!products.Any()) continue;
                        }
                        var tot = products.Sum(o => (decimal)o.SubTotal);
                        if (objSol.ModeSN == 149)
                            tot = products.Sum(o => (decimal)o.IndentNum);

                        var maxVal = objSol.Regions.Max(o => o.EndVal);
                        var reg = objSol.Regions.Where(o => (o.StartVal <= tot && o.EndVal >= tot) || (tot > o.EndVal && maxVal == o.EndVal) || (o.StartVal == maxVal && o.EndVal.HasValue == false)).OrderBy(o => o.EndVal).FirstOrDefault();
                        if (reg == null) continue;
                        var objVal = pro.RegionVals.FirstOrDefault(o => o.PrivilegeRegionId == reg.Id);
                        if (objVal == null) continue;
                        var num = objVal.Value.GetValueOrDefault();
                        if (objSol.ModeSN == 44 || objSol.ModeSN == 147)//返X%
                        {
                            num = tot * objVal.Value.GetValueOrDefault();
                        }
                        nums += num;
                        total += tot;
                        calc.Details.AddRange(products.Select(o => new PrivilegeCalcDetail()
                        {
                            Barcode = o.Barcode,
                            CategorySN = o.CategorySN,
                            CategoryTitle = string.IsNullOrWhiteSpace(o.SubCategoryTitle) ? o.MidCategoryTitle : o.SubCategoryTitle,
                            IndentOrderId = o.IndentOrderId,
                            OrderDate = o.OrderDate,
                            PartName = o.Title,
                            Price = o.BuyPrice,
                            TakeDate = o.ReceivedDT,
                            SubTotal = o.SubTotal,
                            TakeNum = o.IndentNum,
                            Unit = o.SubUnit,
                        }));
                        calc.Results.Add(new PrivilegeCalcResult()
                        {
                            BarcodeOrCategorySN = pro.BarcodeOrCategorySN,
                            EndVal = reg.EndVal.GetValueOrDefault(),
                            StartVal = reg.StartVal.GetValueOrDefault(),
                            TotalNum = tot,
                            Value = objVal.Value.GetValueOrDefault(),
                            Type = pro.Type
                        });
                    }
                }
                if (!calc.Details.Any()) continue;
                calc.SupplierId = supp.SupplierId;
                calc.SupplierTitle = supp.SupplierTitle;
                calc.PrivilegeNum = nums;
                calc.TotalMoney = total;
                calcs.Add(calc);
            }
            return calcs;
        }
        public static OpResult DeleteCalc(int[] ids)
        {
            var list = BaseService<PrivilegeCalc>.CurrentRepository.QueryEntity.Include(o=>o.Details).Include(o=>o.Results).Where(o => ids.Contains(o.Id)).ToList();
            return BaseService<PrivilegeCalc>.Delete(list);
        }
        public static PrivilegeCalc GetCalc(int id)
        {
            return BaseService<PrivilegeCalc>.CurrentRepository.QueryEntity.Include(o => o.Details).FirstOrDefault(o => o.Id == id);
        }
        public static IEnumerable<dynamic> OrderCalc(string supplierId, string barcodes, string categorys,decimal ordernum)
        {
            var bars = barcodes.Split(',').ToList();
            var cates = categorys.Split(',').Distinct();
            var dicts = new Dictionary<int, List<int>>();
            foreach (var child in cates)
            {
                var c= int.Parse(child);
                var psn = ProductCategoryService.GetParentSNs(new List<int>() {c });
                dicts[c] = psn;
                bars.AddRange(psn.Select(o=>o.ToString()));
            }
            var dal = new Pharos.Logic.DAL.PrivilegeProductDAL();
            var dt= dal.GetProductGifts(supplierId, bars.Distinct(), ordernum);
            var list = new List<dynamic>();
            var maxEnd= dt.AsEnumerable().Max(o => (decimal?)o["EndVal"]);//
            foreach(DataRow dr in dt.Rows)
            {
                var startVal = (decimal)dr["StartVal"];
                var endVal = (decimal)dr["EndVal"];
                if ((ordernum > maxEnd && endVal==maxEnd) || (ordernum >= startVal && ordernum <= endVal))
                {
                    var barCate= dr["BarcodeOrCategorySN"].ToString();
                    if (barCate.Length < 10)
                    {
                        var cate = int.Parse(barCate);
                        foreach (var de in dicts)
                        {
                            if (de.Key != cate && de.Value.Any(o => o == cate))
                            {
                                list.Add(new
                                {
                                    BarcodeOrCategorySN = de.Key.ToString(),
                                    Detail = System.Web.HttpUtility.HtmlDecode(dr["detail"].ToString()),
                                    Gift = dr["barnum"]
                                });
                            }
                            else if (de.Key != cate)
                                barCate = null;
                        }
                    }
                    if (barCate.IsNullOrEmpty()) continue;
                    list.Add(new
                    {
                        BarcodeOrCategorySN = barCate,
                        Detail =System.Web.HttpUtility.HtmlDecode(dr["detail"].ToString()),
                        Gift = dr["barnum"]
                    });
                }
            };
            return list;
        }
        #endregion
        #region 辅助方法
        static string Mode(int sn,IList<SysDataDictionary> datas)
        {
            var obj= datas.FirstOrDefault(o => o.DicSN == sn);
            if (obj == null) return "";
            return obj.Title;
        }
        static string Suppliers(string supplierIds,List<Supplier> suppliers)
        {
            var str = "";
            if(!supplierIds.IsNullOrEmpty())
            {
                var sids= supplierIds.Split(',');
                str = string.Join(",", suppliers.Where(o => sids.Contains(o.Id)).Select(o=>o.FullTitle));
            }
            return str;
        }
        static string Detail(int solId,List<ProductRecord> products,List<ProductCategory> categorys, List<PrivilegeProduct> setProducts, List<PrivilegeRegionVal> setProductVals)
        {
            var str = "";
            var setList= setProducts.Where(o => o.PrivilegeSolutionId == solId).ToList();
            var setListIds = setList.Select(o => o.Id).ToList();
            var barcodes = setList.Where(o => o.Type == 1).Select(o => o.BarcodeOrCategorySN).ToList();
            var categorySNs = setList.Where(o => o.Type == 2).Select(o => int.Parse(o.BarcodeOrCategorySN)).ToList();
            var setVals = setProductVals.Where(o => setListIds.Contains(o.PrivilegeProductId)).ToList();
            products = barcodes.Count > 0 ? products.Where(o => barcodes.Contains(o.Barcode)).ToList() : new List<ProductRecord>();
            categorys = categorySNs.Count > 0 ? categorys.Where(o => categorySNs.Contains(o.CategorySN)).ToList() : new List<ProductCategory>();
            int count = 0;
            if (products.Any())
            {
                str += "<div style='color:orange'>商品信息</div>";
                foreach (var pro in setList.Where(o=>o.Type==1))
                {
                    if(count==5)
                    {
                        str += "...";
                        count = 0;
                        break;
                    }
                    var obj= products.FirstOrDefault(o => o.Barcode == pro.BarcodeOrCategorySN);
                    if (obj == null) continue;
                    str+= obj.Title + " ";
                    str+=" "+ string.Join(",", setVals.Where(o => o.PrivilegeProductId == pro.Id).Select(o => o.Value))+"<br>";
                    count++;
                }
            }
            if (categorys.Any())
            {
                str += "<div style='color:orange'>系列信息</div>";
                foreach (var pro in setList.Where(o => o.Type == 2))
                {
                    if (count == 5)
                    {
                        str += "...";
                        count = 0;
                        break;
                    }
                    var obj = categorys.FirstOrDefault(o => o.CategorySN ==int.Parse(pro.BarcodeOrCategorySN));
                    if (obj == null) continue;
                    str += obj.Title + " ";
                    str += " " + string.Join(",", setVals.Where(o => o.PrivilegeProductId == pro.Id).Select(o => o.Value)) + "<br>";
                    count++;
                }
            }
            str = str.TrimEnd("<br>".ToCharArray());
            return str;

        }
        static string GetProductTitle(List<ProductRecord> list,string barcode)
        {
            if (barcode.IsNullOrEmpty()) return "";
            var obj= list.FirstOrDefault(o => o.Barcode == barcode);
            if (obj == null) return "";
            return obj.Title;
        }
        #endregion
    }
}