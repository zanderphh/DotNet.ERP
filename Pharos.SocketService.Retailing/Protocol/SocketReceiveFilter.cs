﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharos.SocketService.Retailing.Protocol
{
    public class SocketReceiveFilter : BaseDataSyncReceiveFilter<SocketRequestInfo>
    {
        /// <summary>
        /// 起始标识 '#pharos@socket@start#'
        /// </summary>
        public static readonly byte[] BeginMark;
        /// <summary>
        /// 结束标识 '#pharos@socket@end#'
        /// </summary>
        public static readonly byte[] EndMark;
        /// <summary>
        /// 静态初始化器，以初始化常量
        /// </summary>
        static SocketReceiveFilter()
        {
            BeginMark = SocketSession.defaultEncoding.GetBytes("#s.[#");
            EndMark = SocketSession.defaultEncoding.GetBytes("#s.]#");
        }
        /// <summary>
        /// 接收数据组包及过滤器
        /// </summary>
        /// <param name="commandRuleProvider">命令路由解析器</param>
        public SocketReceiveFilter(ICommandRuleProvider commandRuleProvider)
            : base(BeginMark, EndMark)
        {
            CommandRuleProvider = commandRuleProvider;
        }
        /// <summary>
        /// 命令路由解析器
        /// </summary>
        public ICommandRuleProvider CommandRuleProvider { get; private set; }

        /// <summary>
        /// 初始化会话信息
        /// </summary>
        /// <param name="readBuffer">请求的缓存数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns>组包后转化成DataSyncRequestInfo对象</returns>
        protected override SocketRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            if (length <= CommandRuleProvider.BytesLength)
            {
                return NullRequestInfo;
            }
            var startMarkLength = BeginMark.Length;
            var endMarkLength = EndMark.Length;
            var requestBody = new byte[readBuffer.Length - offset - endMarkLength - startMarkLength - CommandRuleProvider.BytesLength];
            Array.Copy(readBuffer, offset + startMarkLength + CommandRuleProvider.BytesLength, requestBody, 0, requestBody.Length);

            var entry = readBuffer[offset + startMarkLength];
            var cmdCode = new byte[CommandRuleProvider.BytesLength - 1];
            Array.Copy(readBuffer, offset + startMarkLength + 1, cmdCode, 0, cmdCode.Length);

            var routeRule = new CommandRule() { Command = cmdCode, Entry = entry };
            if (!CommandRuleProvider.Verfy(routeRule))//无效请求，路由错误
            {
                return NullRequestInfo;
            }
            var requestInfo = new SocketRequestInfo(routeRule, CommandRuleProvider, requestBody);
            return requestInfo;
        }
        /// <summary>
        /// 该方法将会在 SuperSocket 收到一块二进制数据时被执行，接收到的数据在 readBuffer 中从 offset 开始， 长度为 length 的部分
        /// </summary>
        /// <param name="readBuffer">接收缓冲区, 接收到的数据存放在此数组里</param>
        /// <param name="offset">接收到的数据在接收缓冲区的起始位置</param>
        /// <param name="length">本轮接收到的数据的长度</param>
        /// <param name="toBeCopied">表示当你想缓存接收到的数据时，是否需要为接收到的数据重新创建一个备份而不是直接使用接收缓冲区</param>
        /// <param name="rest">这是一个输出参数, 它应该被设置为当解析到一个为政的请求后，接收缓冲区还剩余多少数据未被解析</param>
        /// <returns>组包后转化成DataSyncRequestInfo对象</returns>
        public override SocketRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            try
            {
                var result = base.Filter(readBuffer, offset, length, toBeCopied, out rest);
                return result;
            }
            catch (Exception ex)
            {
                byte[] data = new byte[0];
                if (readBuffer.Length >= offset + length)
                {
                    data = readBuffer.Skip(offset).Take(length).ToArray();
                }
                return NullRequestInfo;
            }
        }
    }
}