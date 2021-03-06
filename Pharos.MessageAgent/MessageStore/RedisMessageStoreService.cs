﻿using Newtonsoft.Json;
using Pharos.MessageAgent.Agent;
using Pharos.MessageAgent.Data;
using Pharos.MessageAgent.Extensions;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Pharos.MessageAgent.MessageStore
{
    public class RedisMessageStoreService : IMessageStoreService
    {
        private static Dictionary<string, List<MessageSubscribeRecord>> MessageSubscribeRecords = new Dictionary<string, List<MessageSubscribeRecord>>();

        public RedisMessageStoreService()
        {
            var db = Connection.GetDatabase();
            var ranges = db.ListRange("MessageSubscribeRecord");
            List<MessageSubscribeRecord> temp = new List<MessageSubscribeRecord>();
            foreach (var item in ranges)
            {
                var topicSubscribes = db.HashGetAll(item.ToString()).Select(o => JsonConvert.DeserializeObject<MessageSubscribeRecord>(o.Value)).ToList();
                MessageSubscribeRecords[item] = topicSubscribes;
            }
        }
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return MessageServerConfiguration.GetConfig().MessageStoreConnectionString.GetConnection();
            }
        }
        public PubishInformaction GetAndRemoveFailureRecord(string subscribeId, string topic)
        {
            var db = Connection.GetDatabase();
            var key = GetFailureRecordsKey(subscribeId, topic);
            var json = db.ListLeftPop(key);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<PubishInformaction>(json);
        }

        public void SaveFailureRecords(PubishInformaction pubish, IEnumerable<MessageSubscribeRecord> subscribes)
        {
            foreach (var item in subscribes)
            {
                var db = Connection.GetDatabase();
                var key = GetFailureRecordsKey(item.SubscribeInformaction.SubscribeId, item.SubscribeInformaction.Topic);
                var json = JsonConvert.SerializeObject(pubish);
                db.ListRightPush(key, json);
            }
        }

        public MessageSubscribeRecord GetSubscribe(string subscribeId, string topic)
        {
            var db = Connection.GetDatabase();
            var json = db.HashGet(topic, subscribeId);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<MessageSubscribeRecord>(json);
        }

        public void SaveSubscribe(MessageSubscribeRecord record)
        {
            var db = Connection.GetDatabase();
            var json = JsonConvert.SerializeObject(record);
            db.ListRightPush("MessageSubscribeRecord", record.SubscribeInformaction.Topic);
            db.HashSet(record.SubscribeInformaction.Topic, record.SubscribeInformaction.SubscribeId, json);
            if (!MessageSubscribeRecords.ContainsKey(record.SubscribeInformaction.Topic))
            {
                MessageSubscribeRecords[record.SubscribeInformaction.Topic] = new List<MessageSubscribeRecord>();
            }
            MessageSubscribeRecords[record.SubscribeInformaction.Topic].Add(record);
        }


        public void UpdateSubscribe(MessageSubscribeRecord record)
        {
            var db = Connection.GetDatabase();
            var json = JsonConvert.SerializeObject(record);
            db.HashSet(record.SubscribeInformaction.Topic, record.SubscribeInformaction.SubscribeId, json);

            if (MessageSubscribeRecords.ContainsKey(record.SubscribeInformaction.Topic))
            {
                var subscribe = MessageSubscribeRecords[record.SubscribeInformaction.Topic].FirstOrDefault(o => o.SubscribeInformaction.SubscribeId == record.SubscribeInformaction.SubscribeId);
                subscribe.CurrentSessionId = record.CurrentSessionId;
            }
        }

        public void RemoveSubscribe(string subscribeId, string topic)
        {
            var db = Connection.GetDatabase();
            db.HashDelete(topic, subscribeId);
            if (MessageSubscribeRecords.ContainsKey(topic))
            {
                MessageSubscribeRecords[topic].RemoveAll(o => o.SubscribeInformaction.SubscribeId == subscribeId);
            }
        }

        public bool HasTopicSubscribe(string topic)
        {
            return MessageSubscribeRecords.ContainsKey(topic);
        }

        public IEnumerable<MessageSubscribeRecord> GetSubscribes(string topic)
        {
            if (!MessageSubscribeRecords.ContainsKey(topic))
            {
                return null;
            }
            return MessageSubscribeRecords[topic];
        }

        private string GetFailureRecordsKey(string subscribeId, string topic)
        {
            return "FailureRecords" + topic + "_" + subscribeId;
        }
    }
}
