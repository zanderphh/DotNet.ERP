﻿using System;
using System.Collections.Generic;
using System.Linq;
using Pharos.SyncService.Helpers;
using System.Text;
using Pharos.Logic.ApiData.Pos.DAL;
using Pharos.Logic.DAL;
using Pharos.SyncService.SyncEntities;
using Pharos.SyncService.Exceptions;

namespace Pharos.SyncService.RemoteDataServices
{
    public class MemberIntegralSyncRemoteService : ISyncDataService
    {
        public TimeSpan SyncInterval
        {
            get
            {
                return new TimeSpan(0, 30, 0);
            }
        }
        public string Name
        {
            get
            {
                return this.GetType().ToString();
            }

        }
        public IEnumerable<ISyncDataObject> GetSyncObjects(int companyId, string storeId)
        {
            try
            {
                using (var db = SyncDbContextFactory.Factory<EFDbContext>())
                {
                    var result = db.MemberIntegrals.Where(o => o.CompanyId == companyId).Select(o => new SyncDataObject() { SyncItemId = o.SyncItemId, SyncItemVersion = o.SyncItemVersion }).ToList();
                    return result;
                }
            }
            catch
            {
                return new List<ISyncDataObject>();
            }
        }

        public ISyncDataObject GetItem(Guid guid, int companyId, string storeId)
        {
            using (var db = SyncDbContextFactory.Factory<EFDbContext>())
            {
                var entity = db.MemberIntegrals.Where(o => o.SyncItemId == guid && o.CompanyId == companyId).FirstOrDefault();
                if (entity != null)
                    return new Pharos.SyncService.SyncEntities.MemberIntegral().InitEntity(entity);
                return null;
            }
        }
        public IDictionary<Microsoft.Synchronization.SyncId, ISyncDataObject> GetItems(IEnumerable<Microsoft.Synchronization.SyncId> ids, int companyId, string StoreId)
        {
            using (var db = SyncDbContextFactory.Factory<EFDbContext>())
            {
                var syncidsdict = ids.ToDictionary(o => o.GetGuidId(), o => o);
                var syncids = syncidsdict.Keys;
                var datas = db.MemberIntegrals.Where(o => syncids.Contains(o.SyncItemId)).ToList();
                return datas.ToDictionary(o => syncidsdict[o.SyncItemId], o => (ISyncDataObject)new Pharos.SyncService.SyncEntities.MemberIntegral().InitEntity(o));
            }
        }
        public byte[] CreateItem(ISyncDataObject data, Guid guid, int companyId, string storeId)
        {
            var temp = data as MemberIntegral;
            using (var db = SyncDbContextFactory.Factory<EFDbContext>())
            {
                Pharos.Logic.Entity.MemberIntegral entity;
                if (!db.MemberIntegrals.Any(o => o.SyncItemId == guid))
                {
                    entity = new Pharos.Logic.Entity.MemberIntegral();
                    entity.InitEntity(temp, false);
                    db.MemberIntegrals.Add(entity);
                    db.SaveChanges();
                }
                else
                {
                    entity = db.MemberIntegrals.FirstOrDefault(o => o.SyncItemId == guid);
                }

                return entity.SyncItemVersion;
            }
        }

        public byte[] UpdateItem(Guid guid, ISyncDataObject mergedData, int companyId, string storeId)
        {
            //var temp = mergedData as MemberIntegral;
            //using (var db = SyncDbContextFactory.Factory<EFDbContext>())
            //{
            //    var dbEntity = db.MemberIntegrals.FirstOrDefault(o => o.SyncItemId == guid && o.CompanyId == companyId);
            //    dbEntity.InitEntity(temp, false);
            //    db.SaveChanges();
            //    var newEntity = db.MemberIntegrals.FirstOrDefault(o => o.SyncItemId == guid);
            //    return newEntity.SyncItemVersion;
            //}
            throw new SyncException("DeviceRegInfo表不允许远程修改！");
        }

        public void DeleteItem(Guid syncItemId, int companyId, string storeId)
        {
            throw new SyncException("DeviceRegInfo表不允许远程删除！");
        }

        public ISyncDataObject Merge(ISyncDataObject syncDataObject1, ISyncDataObject syncDataObject2, int companyId, string storeId)
        {
            return syncDataObject1;
        }
        public Microsoft.Synchronization.SyncDirectionOrder SyncDirectionOrder
        {
            get { return Microsoft.Synchronization.SyncDirectionOrder.UploadAndDownload; }
        }
    }
}
