﻿using Newtonsoft.Json;
using Pharos.SocketService.Retailing.Models.Pos;
using Pharos.SocketService.Retailing.Protocol;
using Pharos.SocketService.Retailing.Protocol.AppServers;
using Pharos.SocketService.Retailing.Protocol.AppSessions;
using Pharos.SocketService.Retailing.Protocol.CommandProviders;
using Pharos.SocketService.Retailing.Protocol.RequestInfos;
using System;
using System.IO;

namespace Pharos.SocketService.Retailing.Commands.V1
{
    public class StoreRegisterCommand : CommandBase
    {
        public StoreRegisterCommand()
            : base(new byte[4] { 0x00, 0x00, 0x00, 0x01 }, new PosStoreCommandNameProvider())
        {
        }

        public override void Excute(PosStoreServer server, PosStoreSession session, PosStoreRequestInfo requestInfo)
        {
            try
            {
                Stream s = new MemoryStream(requestInfo.Body);
                StreamReader sw = new StreamReader(s);
                JsonTextReader writer = new JsonTextReader(sw);
                JsonSerializer ser = new JsonSerializer();
                var store = ser.Deserialize<StoreInfo>(writer);
                writer.Close();
                sw.Close();
                s.Close();

                server.SessionManager.RegisterDeviceSession(new PosStoreSessionInfo() { StoreId = store.StoreId, CompanyId = store.CompanyId, SessionId = session.SessionID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
        }
    }
}
