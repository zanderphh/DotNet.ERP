﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;
using Pharos.POS.Retailing.Devices.Exceptions;
using Pharos.POS.Retailing.Models.PosModels;
using System.Globalization;
using System.Threading.Tasks;

namespace Pharos.POS.Retailing.Devices.QuickConnectTools
{
    /// <summary>
    /// 连接工具类
    /// </summary>
    public class NewLandPOSDeviceConnectTool
    {
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="token">取消操作通知</param>
        /// <param name="serialPortRequest">打开串口信息</param>
        /// <returns>串口对象</returns>
        internal static SerialPort OpenSerialPort(CancellationToken token, SerialPortRequest serialPortRequest)
        {
            try
            {
                var ports = SerialPort.GetPortNames();
                if (!ports.Contains(serialPortRequest.ComPort))
                {
                    throw new DeviceException("未能找到PC端串口，请检查串口是否驱动并且连接正常！");
                }
                var connection = new SerialPort(serialPortRequest.ComPort, serialPortRequest.BaudRate, serialPortRequest.Parity, serialPortRequest.DataBits, serialPortRequest.StopBits);
                connection.Handshake = serialPortRequest.Handshake;
                connection.Open();
                HasDataReceived = false;
                connection.DataReceived += connection_DataReceived;
                if (token.IsCancellationRequested)
                {
                    connection.Close();
                    throw new DeviceException("已取消操作，并且关闭设备连接！");
                }
                return connection;
            }
            catch (Win32Exception)
            {
                throw new DeviceException(@"无法查询的串行端口名称！请检查串口注册表 HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM 信息是否完整！");
            }
            catch (IOException)
            {
                throw new DeviceException(string.Format("未能找到或打开指定的端口{0}！", serialPortRequest.ComPort));
            }
            catch (UnauthorizedAccessException)
            {
                throw new DeviceException(string.Format("对端口{0}的访问被拒绝，请在设备上取消前一次操作！", serialPortRequest.ComPort));
            }
        }
        /// <summary>
        /// 根据请求格式化操作命令
        /// </summary>
        /// <param name="posRequest">请求内容</param>
        /// <returns>命令字符串</returns>
        internal static string FormatDeviceCommand(POSDevicePayRequest posRequest)
        {
            try
            {
                var code = "";
                var qrcode = "";
                switch (posRequest.Type)
                {
                    case TransactionType.Consumption:
                        code = "00";
                        break;
                    case TransactionType.Refund:
                        code = "20";
                        break;
                    case TransactionType.ReadCard:
                        code = "12";
                        break;
                    case TransactionType.QRCode:
                        code = "13";
                        qrcode = posRequest.QRCode;
                        break;
                }
                var userCode = "0000" + posRequest.CashierId;
                userCode = userCode.Substring(userCode.Length - 4, 4);
                var orderSn = "00000000000000000000" + posRequest.OrderSn;
                orderSn = orderSn.Substring(orderSn.Length - 20, 20);

                var reqInfo = string.Format(
                    "{10}{0:000000000000}{1,8}{2:0000}{3:MMddHHmmss}{4:00000000000000000000}{5,6}{6,10}{7,6}{8,12}{11}{9}",
                    Math.Round(posRequest.Amount * 100),
                    posRequest.MachineSn,
                    userCode,
                    DateTime.Now,
                    orderSn,
                    posRequest.OldTransactionCode,
                    "",
                    "",
                    "",
                    "",
                    code,
                    qrcode);
                return reqInfo;

            }
            catch (Exception)
            {
                throw new DeviceException("生成设备操作命令时出错！");
            }
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="token">取消操作通知</param>
        /// <param name="connection">串口对象</param>
        /// <param name="cmd">命令</param>
        internal static void SendCommandToSerialPortConnection(CancellationToken token, SerialPort connection, string cmd)
        {
            if (connection == null)
            {
                throw new DeviceException("串口连接不能为null！");
            }
            if (string.IsNullOrEmpty(cmd))
            {
                throw new DeviceException("操作命令不能为空！");
            }
            try
            {
                if (token.IsCancellationRequested && connection.IsOpen)
                {
                    connection.Close();
                    throw new DeviceException("已取消操作，并且关闭设备连接！");
                }
                List<byte> buffers = new List<byte>();
                var code = Encoding.UTF8.GetBytes(cmd);
                var lenHex = code.Length.ToString("0000");
                buffers.Add(byte.Parse(lenHex.Substring(0, 2), NumberStyles.HexNumber));
                buffers.Add(byte.Parse(lenHex.Substring(2, 2), NumberStyles.HexNumber));
                buffers.AddRange(code);
                buffers.Add(0x03);//从串口数据看此处应该协议尾
                buffers.Add(GetXOR(buffers.ToArray(), 0, buffers.Count));//从串口数据看此处应该协议尾
                buffers.Insert(0, 0x02);//从串口数据看此处应该协议头
                connection.Write(buffers.ToArray(), 0, buffers.Count);
            }
            catch (InvalidOperationException)
            {
                throw new DeviceException("串口连接未打开，不允许发送数据！");

            }
            catch (TimeoutException)
            {
                throw new DeviceException("写入命令数据失败！");
            }
        }

        /// <summary>  
        /// 计算按位异或校验和（返回校验和值）  
        /// </summary>  
        /// <param name="Cmd">命令数组</param>  
        /// <returns>校验和值</returns>  
        public static byte GetXOR(byte[] Cmd, int index, int len)
        {
            byte check = (byte)(Cmd[index] ^ Cmd[index + 1]);
            for (int i = index + 2; i < len; i++)
            {
                check = (byte)(check ^ Cmd[i]);
            }
            return check;
        }
        /// <summary>
        /// 监听串口连接
        /// </summary>
        /// <param name="token">取消操作通知</param>
        /// <param name="connection">串口对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>解析接收到的结果</returns>
        internal static POSDevicePayResponse ListenToSerialPortConnection(CancellationToken token, SerialPort connection, int timeout = 60000)
        {
            var retryCounter = 0;

            if (connection == null)
            {
                throw new DeviceException("串口连接不能为null！");
            }
            var interval = 100;
            var counter = 1;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    connection.Close();
                    throw new DeviceException("已取消操作，并且关闭设备连接！");
                }
                if (HasDataReceived)
                {
                    Thread.Sleep(interval);
                    var buffers = new byte[connection.BytesToRead];
                    connection.Read(buffers, 0, connection.BytesToRead);
                    if (retryCounter < 10)//确保数据接收完整
                    {
                        if (buffers.Length < 5 || (buffers.Length > 5 && buffers[buffers.Length - 2] != 0x03))
                        {
                            retryCounter++;
                            Thread.Sleep(interval);
                            continue;
                        }
                        else
                        {
                            var len = Convert.ToInt32(BitConverter.ToString(buffers, 1, 2).Replace("-", ""));
                            if (len + 5 > buffers.Length)
                            {
                                retryCounter++;
                                Thread.Sleep(interval);
                                continue;
                            }
                        }
                    }
                    var contentLen = buffers.Length - 5;
                    if (contentLen < 1)
                        throw new DeviceException("设备返回数据失效！");
                    var content = new byte[contentLen];
                    Array.Copy(buffers, 3, content, 0, contentLen);

                    var info = Encoding.GetEncoding("gb2312").GetString(content);
                    connection.Close();
                    return FormatResponse(info);
                }
                if (interval * counter > timeout)
                {
                    connection.Close();
                    throw new DeviceException("等待设备操作完成超时，已关闭设备连接，结束操作！");
                }
                Thread.Sleep(interval);
                counter++;
            }

        }
        /// <summary>
        /// 解析接收到的结果
        /// </summary>
        /// <param name="info">接收到的字符串数据</param>
        /// <returns>解析结果</returns>
        internal static POSDevicePayResponse FormatResponse(string info)
        {
            try
            {
                var returnStr = info.ToString();
                var isSuccessCode = returnStr.Substring(2, 2);
                if (isSuccessCode == "00")
                {
                    var response = new POSDevicePayResponse();
                    var cardKind = returnStr.Substring(4, 1);
                    if (cardKind == "0" || (cardKind == " "))
                    {
                        var cardno = returnStr.Substring(252, 20).Trim();
                        response.CardNo = cardno;
                    }
                    else if (cardKind == "1")
                    {
                        var cardno = returnStr.Substring(8, 244);
                        response.CardNo = cardno.Replace(" ", "").Replace("~", "");
                    }
                    response.CardPin = returnStr.Substring(272, 12).Trim();
                    response.CardName = returnStr.Substring(284, 10).Trim();
                    response.DeviceTranDate = returnStr.Substring(294, 10).Trim();
                    response.BankTransactionCode = returnStr.Substring(304, 12).Trim();
                    response.TransactionCode = returnStr.Substring(328, 6).Trim();
                    response.AuthCode = returnStr.Substring(334, 6).Trim();
                    return response;
                }
                else
                {
                    throw new DeviceException("在设备上操作失败，或者设备取消了操作！");
                }
            }
            catch (DeviceException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new DeviceException("解析失败，无效的设备回传数据！解析错误信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 是否已经收到数据
        /// </summary>
        static bool HasDataReceived { get; set; }
        static void connection_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            HasDataReceived = true;
        }
    }
}