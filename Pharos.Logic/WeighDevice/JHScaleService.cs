﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Pharos.Logic.BLL;
using Pharos.Logic.BLL.DataSynchronism.Dtos;
using Pharos.Logic.Entity;
using Pharos.Logic.WeighDevice.ScaleEntity;
using Pharos.Sys.Entity;
using Pharos.Utility;

namespace Pharos.Logic.WeighDevice
{
    public class JHScaleService : IWeighScale<JHScaleEntity>
    {
        #region dll方法调用
        /// 数据发送方法
        /// </summary>
        /// <param name="pIP"></param>
        /// <param name="pFileName"></param>
        /// <returns></returns>
        [DllImport("JHScale.dll")]
        private static extern int JHScale_TxDevice(byte[] pIP, byte[] pFileName);
        /// <summary>
        /// 清空原有数据
        /// </summary>
        /// <param name="pIP"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [DllImport("JHScale.dll")]
        private static extern int JH_ClearPLU(IntPtr pIP, int p);
        /// <summary>
        /// 电子秤连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="recvTimeout"></param>
        /// <param name="sendTimeout"></param>
        /// <param name="recvSize"></param>
        /// <param name="sendSize"></param>
        /// <returns></returns>
        [DllImport("JHScale.dll")]
        private static extern IntPtr JH_Connect(string ip, int connectTimeout, int recvTimeout, int sendTimeout, int recvSize, int sendSize);
        /// <summary>
        /// 关闭连接，释放资源
        /// </summary>
        /// <param name="pIP"></param>
        /// <returns></returns>
        [DllImport("JHScale.dll")]
        private static extern int JH_Close(IntPtr pIP);

        #endregion

        private readonly static object obj = new object();
        /// <summary>
        /// 商品传秤
        /// </summary>
        /// <param name="entitys"></param>
        /// <param name="ip"></param>
        /// <param name="isClear"></param>
        /// <returns></returns>
        public OpResult TransferData(List<JHScaleEntity> entitys, List<string> ips, bool isClear)
        {
            try
            {
                List<string> errs = new List<string>();
                foreach (var ip in ips)
                {
                    if (isClear)
                    {
                        var connectResult = JH_Connect(ip, 5000, 5000, 5000, 1024, 1024);
                        if (connectResult == IntPtr.Zero)
                        {
                            errs.Add(ip + "电子秤连接失败！");
                            continue;
                        }
                        var clearResult = JH_ClearPLU(connectResult, 0);
                        if (clearResult != 1)
                        {
                            errs.Add(ip + "电子秤原始数据清除失败！");
                            continue;
                        }
                        JH_Close(connectResult);
                    }
                    lock (obj)
                    {
                        var path = System.Web.HttpContext.Current.Server.MapPath(@"/bin");//Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);

                        string e = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(path));

                        string fileName = Path.Combine(e, "PLUDATA.txt");
                        string file = "PLUDATA.txt";
                        FileStream fs = new FileStream(fileName, FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                        StringBuilder sb = new StringBuilder();

                        var totalAccount = entitys.Count;
                        //最多支持126个
                        if (totalAccount > 126)
                        {
                            totalAccount = 126;
                        }

                        for (int i = 0; i < totalAccount; i++)
                        {
                            sb.Append(entitys[i].Text + "\t");
                            sb.Append(entitys[i].PLUSN + "\t");
                            sb.Append(entitys[i].Barcode + "\t");
                            //sb.Append("" + "\t");
                            sb.Append(entitys[i].Title + "\t");
                            sb.Append(string.Format("{0:F}", entitys[i].Price) + "\t");
                            sb.Append(entitys[i].Unit + "\t");
                            sb.Append(entitys[i].PrintDate + "\t");
                            sb.Append(entitys[i].UsedDate + "\t");
                            sb.Append(entitys[i].KeySN);
                            sb.Append("\r\n");
                        }

                        sw.Write(sb.ToString());
                        sw.Flush();
                        sw.Close();
                        sw.Dispose();
                        fs.Close();
                        fs.Dispose();
                        int result = JHScale_TxDevice(Encoding.ASCII.GetBytes(ip), Encoding.ASCII.GetBytes(file));
                        switch (result)
                        {
                            case 0:
                                break;
                            case 1:
                                errs.Add(ip + "发送数据时连接失败！");
                                break;
                            case 2:
                                errs.Add(ip + "数据文件错误！");
                                break;
                            default:
                                errs.Add(ip + "数据文件错误！");
                                break;
                        }
                    }
                }
                if (errs.Count > 0)
                {
                    return OpResult.Fail(string.Join(";", errs));
                }
                else
                {
                    return OpResult.Success("操作成功！");
                }
            }
            catch (Exception e)
            {
                new Sys.LogEngine().WriteError(e);
                return OpResult.Fail("操作失败！" + e.Message);
            }
        }
        /// <summary>
        /// 将商品信息转为电子秤对应的数据格式
        /// </summary>
        /// <param name="products">商品信息</param>
        /// <param name="units">所有单位</param>
        /// <returns></returns>
        public List<JHScaleEntity> DataFormat(List<VwProduct> products, List<SysDataDictionary> units)
        {
            //unit :1=kg  2=件 3=kg 4=g 5=ton 6=1b 7=500g 8=100g
            List<JHScaleEntity> entitys = new List<JHScaleEntity>();
            for (int i = 0; i < products.Count(); i++)
            {
                //取单位编码
                var unitCode = 1;
                var unit = units.FirstOrDefault(o => o.DicSN == products[i].SubUnitId);
                if (unit != null)
                {
                    var unitTitle = unit.Title.Trim().ToUpper();
                    //友声电子秤对应单位
                    switch (unitTitle)
                    {
                        case "KG": unitCode = 3;
                            break;
                        case "公斤": unitCode = 3;
                            break;
                        case "500G": unitCode = 7;
                            break;
                        case "斤": unitCode = 7;
                            break;
                        case "G": unitCode = 4;
                            break;
                        case "克": unitCode = 4;
                            break;
                        case "100G": unitCode = 8;
                            break;
                    }
                }
                //设置快捷键编码
                string keyCode = string.Empty;
                int page = 0;
                int key = 1;
                if (i + 1 < 63)
                {
                    page = 0;
                    key = i + 1;
                }
                else
                {
                    page = 1;
                    key = i % 63;
                }
                keyCode = page + "," + key;
                JHScaleEntity entity = new JHScaleEntity()
                {
                    Text = "PLU",
                    PLUSN = i + 1,
                    ProductCode = products[i].ProductCode,
                    Barcode = products[i].Barcode,
                    Title = products[i].Title,
                    Price = products[i].SysPrice,
                    Unit = unitCode,
                    PrintDate = 0,
                    UsedDate = 1,
                    KeySN = keyCode
                };
                entitys.Add(entity);
            }
            return entitys;
        }
    }
}