﻿using Pharos.MessageAgent.Data;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Pharos.MessageTransferAgenClient.DomainEvent
{
    public class EventAggregator : IEventAggregator
    {
        private readonly string applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Multiflora", MessageSettings.Current.SubscribeId);

        static EventAggregator()
        {

        }
        public EventAggregator()
        {

        }

        static EventAggregator current;
        public static EventAggregator Current
        {
            get
            {
                if (current == null)
                {
                    current = new EventAggregator();
                }
                return current;
            }
        }
        internal static readonly IList<IEventHandler> handlers = new List<IEventHandler>();
        public void Subscribe(string topic, IEventHandler handler)
        {
            SubscribeAsync(topic, handler).Wait();
        }

        public void Subscribe<TEvent>(string topic, Action<TEvent> handler) where TEvent : class, IEvent
        {
            SubscribeAsync(topic, handler).Wait();
        }
        public System.Threading.Tasks.Task SubscribeAsync<TEvent>(string topic, Action<TEvent> handler) where TEvent : class, IEvent
        {
            return SubscribeAsync(topic, new ActionEventHandler<TEvent>(topic, handler));

        }
        public System.Threading.Tasks.Task SubscribeAsync(string topic, IEventHandler handler)
        {
            return Task.Factory.StartNew(() =>
             {
                 handler.Topic = topic;
                 lock (handlers)
                 {
                     handlers.Add(handler);
                 }
                 var task = RemoteSubscribe(topic);
                 task.Wait();
                 if (task.Status == TaskStatus.RanToCompletion)
                 {
                     Task.Factory.StartNew(() =>
                     {
                         TryFailedItems();
                    });
                 }
             });
        }
        internal Task RemoteSubscribe(string topic)
        {
            return Task.Factory.StartNew(() =>
             {
                 MessageClient.Current.SendObjectToJsonStream(new byte[] { 0x01, 0x00, 0x00, 0x01 }, new SubscribeInformaction()
                 {
                     IsWebSiteSubscriber = MessageSettings.Current.IsWeb,
                     Topic = topic,
                     WebSiteURI = MessageSettings.Current.WebSiteURI,
                     SubscribeId = MessageSettings.Current.SubscribeId
                 });
             });
        }

        public void Publish<TEvent>(string topic, TEvent domainEvent) where TEvent : class, IEvent
        {
            var task = PublishAsync(topic, domainEvent);
            task.Wait();
        }

        public System.Threading.Tasks.Task PublishAsync<TEvent>(string topic, TEvent domainEvent) where TEvent : class, IEvent
        {
            return Task.Factory.StartNew(() =>
            {
                var topicBytes = MessageClient.TextToBytes(topic);

                var content = MessageClient.ObjectToJsonString(domainEvent);
                var contentBytes = MessageClient.TextToBytes(content);

                var lenBytes = BitConverter.GetBytes(topicBytes.Length);

                var body = new byte[topicBytes.Length + contentBytes.Length + 4];
                Array.Copy(lenBytes, 0, body, 0, 4);
                Array.Copy(topicBytes, 0, body, 4, topicBytes.Length);
                Array.Copy(contentBytes, 0, body, 4 + topicBytes.Length, contentBytes.Length);
                try
                {
                    MessageClient.Current.SendBytes(new byte[] { 0x01, 0x00, 0x00, 0x02 }, body);
                    Task.Factory.StartNew(() =>
                    {
                        TryFailedItems();
                    });
                }
                catch (Exception) //只关心发送，数据转换失败必须由调用方处理
                {
                    if (body != null && body.Length > 0)
                    {
                        SaveFailedItem(topic, body);
                    }
                }
            });
        }
        internal void TryFailedItems()
        {
            try
            {
                foreach (var handler in EventAggregator.handlers)
                {
                    RemoteSubscribe(handler.Topic);
                }

                if (!Directory.Exists(applicationDataPath)) return;
                var dirs = Directory.GetDirectories(applicationDataPath);
                foreach (var item in dirs)
                {
                    var files = Directory.GetFiles(item);
                    foreach (var file in files)
                    {
                        var body = File.ReadAllBytes(file);
                        try
                        {
                            MessageClient.Current.SendBytes(new byte[] { 0x01, 0x00, 0x00, 0x02 }, body);
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
        private void SaveFailedItem(string topic, byte[] body)
        {
            var path = Path.Combine(applicationDataPath, topic);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                var access = Directory.GetAccessControl(path);
                access.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule("EveryOne", System.Security.AccessControl.FileSystemRights.Modify, System.Security.AccessControl.AccessControlType.Allow));
                Directory.SetAccessControl(applicationDataPath, access);
                File.WriteAllBytes(Path.Combine(applicationDataPath, topic, Guid.NewGuid().ToString("N")), body);
            }
            File.WriteAllBytes(Path.Combine(path, Guid.NewGuid().ToString("N")), body);

        }
        public void Unsubscribe(string topic, IEventHandler handler = null)
        {
            lock (handlers)
            {
                var topicHandlers = handlers.Where(o => o.Topic == topic);
                if (handler != null)
                {
                    topicHandlers = topicHandlers.Where(o => o == handler);
                }
                foreach (var item in topicHandlers)
                {
                    handlers.Remove(item);
                }
            }
        }

        public void Unsubscribe<TEvent>(string topic, Action<TEvent> handler = null) where TEvent : class, IEvent
        {
            lock (handlers)
            {
                var topicHandlers = handlers.Where(o => o.Topic == topic);
                if (handler != null)
                {
                    topicHandlers = topicHandlers.Where(o => o is ActionEventHandler<TEvent> && ((ActionEventHandler<TEvent>)o).CallBack == handler);
                }
                foreach (var item in topicHandlers)
                {
                    handlers.Remove(item);
                }
            }
        }

        public System.Threading.Tasks.Task UnsubscribeAsync(string topic, IEventHandler handler = null)
        {
            return Task.Factory.StartNew(() =>
            {
                Unsubscribe(topic, handler);
            });
        }

        public void UnsubscribeAll()
        {
            lock (handlers)
            {
                handlers.Clear();
            }
        }

        public System.Threading.Tasks.Task UnsubscribeAllAsync()
        {
            return Task.Factory.StartNew(() =>
             {
                 UnsubscribeAll();
             });
        }
        public void RemotePublish(PubishInformaction info)
        {
            List<Task> tasks = new List<Task>();
            lock (EventAggregator.handlers)
            {
                var topicHandlers = EventAggregator.handlers.Where(o => o.Topic == info.Topic).ToList();
                foreach (var item in topicHandlers)
                {
                    tasks.Add(Task.Factory.StartNew((o) =>
                    {
                        item.Handle((PubishInformaction)o);
                    }, info));
                }
            }
            //Task.WaitAll(tasks.ToArray());

        }

        internal static void RefreshSubscribe(TimeSpan ts)
        {
            lock (StdSchedulerFactory.SystemPropertyAsInstanceId)
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.Start();       //开启调度器
                var tempTriggerKey = new TriggerKey("RefreshSubscribeJob", "RefreshSubscribeJob");
                if (scheduler.CheckExists(tempTriggerKey))
                {
                    scheduler.UnscheduleJob(tempTriggerKey);
                }
                IJobDetail job1 = JobBuilder.Create<RefreshSubscribeJob>()  //创建一个作业
                   .WithIdentity("RefreshSubscribeJob", "RefreshSubscribeJob")
                   .Build();
                ITrigger trigger1 = TriggerBuilder.Create()
                                           .WithIdentity("RefreshSubscribeJob", "RefreshSubscribeJob")
                                           .StartNow()                       //现在开始
                                           .WithSimpleSchedule(x => x         //触发时间
                                               .WithInterval(ts)
                                               .RepeatForever()
                                               )
                                           .Build();
                scheduler.ScheduleJob(job1, trigger1);      //把作业，触发器加入调度器。
            }

        }
    }

    public class RefreshSubscribeJob : IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            EventAggregator.Current.TryFailedItems();
        }
    }
}
