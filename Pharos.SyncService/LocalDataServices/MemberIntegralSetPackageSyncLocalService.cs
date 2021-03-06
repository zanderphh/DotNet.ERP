﻿using Pharos.Logic.ApiData.Pos.DAL;
using Pharos.SyncService.SyncEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pharos.SyncService.Helpers;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace Pharos.SyncService.LocalDataServices
{
    public class MemberIntegralSetPackageSyncLocalService : ISyncDataService, ILocalDescribe
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
        public Microsoft.Synchronization.SyncDirectionOrder SyncDirectionOrder
        {
            get { return Microsoft.Synchronization.SyncDirectionOrder.Download; }
        }
        public string Describe
        {
            get { return "会员规则数据包"; }
        }
        public IEnumerable<ISyncDataObject> GetSyncObjects(int companyId, string storeId)
        {
            try
            {
                using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
                {
                    var result = db.Database.SqlQuery<SyncDataObject>(@"  select 'MemberIntegralSetPackage' as  EntityType, SyncItemId,max (SyncItemVersion)  as SyncItemVersion from (
select  s.syncitemid,s.SyncItemVersion from [MemberIntegralSet] s 
union all
select  s.syncitemid,d.SyncItemVersion from [MemberIntegralSet] s,[MemberIntegralSetList] d where s.Id = d.IntegralId  
) as t group by SyncItemId").ToList();

                    return result;
                }
            }
            catch
            {
                return new List<ISyncDataObject>();
            }
        }
        private ISyncDataObject GetVersion(Guid syncId, int companyId, string storeId, LocalCeDbContext db)
        {
            var result = db.Database.SqlQuery<SyncDataObject>(@"  select 'MemberIntegralSetPackage' as  EntityType, SyncItemId,max (SyncItemVersion)  as SyncItemVersion from (
select  s.syncitemid,s.SyncItemVersion from [MemberIntegralSet] s where s.syncitemid=@p0
union all
select  s.syncitemid,d.SyncItemVersion from [MemberIntegralSet] s,[MemberIntegralSetList] d where s.Id = d.IntegralId and s.syncitemid=@p0
) as t group by SyncItemId",syncId).ToList();

            return result.FirstOrDefault();
        }
        public ISyncDataObject GetItem(Guid guid, int companyId, string storeId)
        {
            using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
            {
                var memberIntegralSet = db.MemberIntegralSets.FirstOrDefault(o => o.SyncItemId == guid);
                var package = new Package() { SyncItemId = guid, EntityType = "MemberIntegralSetPackage" };
                var orderItem = new List<MemberIntegralSet>();
                orderItem.Add(new MemberIntegralSet().InitEntity(memberIntegralSet));
                package.Init(orderItem);
                package.Init(db.MemberIntegralSetLists.Where(o => o.CompanyId == companyId).ToList().Select(o => new MemberIntegralSetList().InitEntity(o, true)));
                return package;
            }
        }
        public IDictionary<Microsoft.Synchronization.SyncId, ISyncDataObject> GetItems(IEnumerable<Microsoft.Synchronization.SyncId> ids, int companyId, string StoreId)
        {
            using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
            {
                var syncidsdict = ids.ToDictionary(o => o.GetGuidId(), o => o);
                var syncids = syncidsdict.Keys;
                var query = db.MemberIntegralSets.Where(o => syncids.Contains(o.SyncItemId)).Include(o => o.ProductList).ToList();
                return query.ToDictionary(o => syncidsdict[o.SyncItemId], o =>
                {
                    var package = new Package() { SyncItemId = o.SyncItemId, EntityType = "MemberIntegralSetPackage" };
                    package.Init(new MemberIntegralSet[] { new MemberIntegralSet().InitEntity(o) });
                    package.Init(o.ProductList.Select(p => new MemberIntegralSetList().InitEntity(p)).ToList());
                    return package as ISyncDataObject;
                });
            }
        }
        public byte[] CreateItem(ISyncDataObject data, Guid guid, int companyId, string storeId)
        {
            var temp = data as Package;
            using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
            {
                try
                {
                    var memberImterSet = temp.GetEntities<MemberIntegralSet>();
                    var memberImterSetList = temp.GetEntities<MemberIntegralSetList>();
                    db.MemberIntegralSets.AddRange(memberImterSet.Select(o => new Pharos.Logic.ApiData.Pos.Entity.LocalCeEntity.MemberIntegralSet().InitEntity(o)));
                    db.MemberIntegralSetLists.AddRange(memberImterSetList.Select(o => new Pharos.Logic.ApiData.Pos.Entity.LocalCeEntity.MemberIntegralSetList().InitEntity(o)));
                    db.SaveChanges();
                    var version = GetVersion(guid, companyId, storeId, db);
                    return version.SyncItemVersion;
                }
                catch (DbEntityValidationException dbEx)
                {
                    throw dbEx;
                }
            }
        }

        public byte[] UpdateItem(Guid guid, ISyncDataObject mergedData, int companyId, string storeId)
        {
            var temp = mergedData as Package;

            using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
            {
                var integralSets = temp.GetEntities<MemberIntegralSet>();
                var integralSetsSyncIds = integralSets.Select(o => o.SyncItemId).ToList();
                var integralSetLists = temp.GetEntities<MemberIntegralSetList>();
                var integralSetListsSyncIds = integralSetLists.Select(o => o.SyncItemId).ToList();

                db.MemberIntegralSets.Where(o => integralSetsSyncIds.Contains(o.SyncItemId)).ToList().ForEach(o => o.InitEntity(integralSets.FirstOrDefault(p => o.SyncItemId == p.SyncItemId)));
                db.MemberIntegralSetLists.Where(o => integralSetListsSyncIds.Contains(o.SyncItemId)).ToList().ForEach(o => o.InitEntity(integralSetLists.FirstOrDefault(p => o.SyncItemId == p.SyncItemId)));
                db.SaveChanges();

                var version = GetVersion(guid, companyId, storeId, db);
                return version.SyncItemVersion;
            }
        }

        public void DeleteItem(Guid syncItemId, int companyId, string storeId)
        {
            using (var db = SyncDbContextFactory.Factory<LocalCeDbContext>())
            {
                var integralSet = db.MemberIntegralSets.FirstOrDefault(o => o.SyncItemId == syncItemId);
                db.MemberIntegralSets.Remove(integralSet);
                db.MemberIntegralSetLists.RemoveRange(db.MemberIntegralSetLists.Where(o => o.IntegralId == integralSet.Id).ToList());
                db.SaveChanges();
            }
        }

        public ISyncDataObject Merge(ISyncDataObject syncDataObject1, ISyncDataObject syncDataObject2, int companyId, string storeId)
        {
            throw new NotImplementedException();
        }
    }
}
