﻿using Pharos.Logic.OMS.Entity;
using Pharos.Logic.OMS.IDAL;
using Pharos.Utility;
using Pharos.Utility.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharos.Logic.OMS.BLL
{
    public class ImportSetService
    {
        [Ninject.Inject]
        public IBaseRepository<ImportSet> ImportSetRepository { get; set; }
        public Pharos.Utility.OpResult SaveOrUpdate(ImportSet model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<dynamic> GetPageList(System.Collections.Specialized.NameValueCollection nvl, out int recordCount)
        {
            throw new NotImplementedException();
        }

        public Pharos.Utility.OpResult Deletes(object[] ids)
        {
            throw new NotImplementedException();
        }

        public ImportSet GetOne(object id)
        {
            return ImportSetRepository.Find(o => o.TableName == id);
        }


        public List<ImportSet> GetList()
        {
            throw new NotImplementedException();
        }

        public OpResult ImportSet(ImportSet obj, System.Web.HttpFileCollectionBase httpFiles, string fieldName, string columnName, ref Dictionary<string, char> fieldCols, ref System.Data.DataTable dt)
        {
            var op = new OpResult();

            if (httpFiles.Count <= 0 || httpFiles[0].ContentLength <= 0)
            {
                op.Message = "请先选择Excel文件";
                return op;
            }
            var stream = httpFiles[0].InputStream;
            var ext = httpFiles[0].FileName.Substring(httpFiles[0].FileName.LastIndexOf("."));
            if (!(ext.Equals(".xls", StringComparison.CurrentCultureIgnoreCase) ||
                ext.Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase)))
            {
                op.Message = "请先选择Excel文件";
                return op;
            }
            fieldCols = fieldCols ?? new Dictionary<string, char>();
            if (!fieldName.IsNullOrEmpty() && !columnName.IsNullOrEmpty())
            {
                var fields = fieldName.Split(',');
                var columns = columnName.Split(',');
                if (fields.Length != columns.Length)
                {
                    op.Message = "配置的字段和列数不一致!";
                    return op;
                }
                for (var i = 0; i < fields.Length; i++)
                {
                    if (columns[i].IsNullOrEmpty()) continue;
                    fieldCols[fields[i]] = Convert.ToChar(columns[i]);
                }
                obj.FieldJson = fieldCols.Select(o => new { o.Key, o.Value }).ToJson();
            }
            if (obj.Id == 0)
            {
                if (!ImportSetRepository.GetQuery(o => o.TableName == obj.TableName).Any())
                     ImportSetRepository.Add(obj);
            }
            else
            {
                var res = ImportSetRepository.Get(obj.Id);
                obj.ToCopyProperty(res);
            }
            ImportSetRepository.SaveChanges();

            dt = new ExportExcel().ToDataTable(stream, minRow: obj.MinRow, maxRow: obj.MaxRow.HasValue ? obj.MaxRow.Value : Int32.MaxValue);
            if (dt == null || dt.Rows.Count <= 0)
            {
                op.Message = "无数据，无法导入!";
                op.Successed = false;
                return op;
            }

            #region 允许配置在同一列
            var cols = fieldCols.GroupBy(o => o.Value).Where(o => o.Count() > 1).ToList();//取重复列
            foreach (var item in cols)
            {
                System.Diagnostics.Debug.WriteLine(item.Key);//重复列value
                var idx = Convert.ToInt32(item.Key) - 65;
                foreach (var subitem in item)
                {
                    System.Diagnostics.Debug.WriteLine(subitem.Key);//重复列key
                    var lastValue = Convert.ToChar(fieldCols.Values.OrderBy(o => o).LastOrDefault() + 1);
                    if (dt.Columns[idx].CloneTo(subitem.Key))
                        fieldCols[subitem.Key] = lastValue;
                }
            }
            #endregion

            op.Successed = true;
            return op;
        }
    }
}