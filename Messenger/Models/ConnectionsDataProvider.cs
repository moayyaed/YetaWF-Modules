/* Copyright � 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Messenger#License */

using System;
using System.Collections.Generic;
using System.Linq;
using YetaWF.Core;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.Identity;
using YetaWF.Core.IO;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;

namespace YetaWF.Modules.Messenger.DataProvider {

    public class Connection {

        public const int MaxConnectionId = 100;

        [Data_PrimaryKey]
        [StringLength(MaxConnectionId)]
        public string ConnectionId { get; set; }

        [Data_Index, StringLength(Globals.MaxUser)]
        public string Name { get; set; }
        [Data_Index, StringLength(Globals.MaxIP)]
        public string IpAddress { get; set; }
        public DateTime LastSeen { get; set; }

        public Connection() {
            LastSeen = DateTime.UtcNow;
        }
    }

    public class ConnectionDataProvider : DataProviderImpl, IInstallableModel, IInitializeApplicationStartup {

        // Startup

        public void InitializeApplicationStartup() {
            // clear all connections from db
            // removes all sites
            RemoveItems(null);
        }

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public ConnectionDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }
        public ConnectionDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(DataProvider); }

        private IDataProvider<string, Connection> DataProvider {
            get {
                if (_dataProvider == null) {
                    Package package = Package.GetPackageFromAssembly(GetType().Assembly);
                    switch (GetIOMode(package.AreaName)) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            throw new InternalError("File I/O is not supported");
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLSimpleObjectDataProvider<string, Connection>(AreaName + "_Connections", SQLDbo, SQLConn,
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                    }
                }
                return _dataProvider;
            }
        }
        private IDataProvider<string, Connection> _dataProvider { get; set; }

        // LOAD/SAVE
        // LOAD/SAVE
        // LOAD/SAVE

        private static object lockObject = new object();

        public Connection GetItem(string key) {
            return DataProvider.Get(key);
        }
        public bool AddItem(Connection data) {
            return DataProvider.Add(data);
        }
        public UpdateStatusEnum UpdateItem(Connection data) {
            return DataProvider.Update(data.ConnectionId, data.ConnectionId, data);
        }
        public bool RemoveItem(string key) {
            return DataProvider.Remove(key);
        }
        public List<Connection> GetItems(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, out int total) {
            return DataProvider.GetRecords(skip, take, sort, filters, out total);
        }
        public int RemoveItems(List<DataProviderFilterInfo> filters) {
            return DataProvider.RemoveRecords(filters);
        }
        public Connection GetEntry(string name) {
            int total;
            List<DataProviderFilterInfo> filters = null;
            filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "Name", Operator = "==", Value = name });
            List<Connection> conns = GetItems(0, 1, null, filters, out total);
            return conns.FirstOrDefault();
        }

        public void UpdateEntry(string name, string ipAddress, string connectionId) {
            lock (lockObject) {
                try {
                    Connection conn = GetItem(connectionId);
                    if (conn == null) {
                        conn = new Connection {
                            ConnectionId = connectionId,
                            IpAddress = ipAddress,
                            Name = name,
                        };
                        AddItem(conn);
                    } else {
                        conn.IpAddress = ipAddress;
                        conn.Name = name;
                        conn.LastSeen = DateTime.UtcNow;
                        UpdateItem(conn);
                    }
                } catch (Exception) { }
            }
        }

        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL

        public bool IsInstalled() {
            return DataProvider.IsInstalled();
        }
        public bool InstallModel(List<string> errorList) {
            return DataProvider.InstallModel(errorList);
        }
        public bool UninstallModel(List<string> errorList) {
            return DataProvider.UninstallModel(errorList);
        }
        public void AddSiteData() {
            DataProvider.AddSiteData();
        }
        public void RemoveSiteData() {
            DataProvider.RemoveSiteData();
        }
        public bool ExportChunk(int chunk, SerializableList<SerializableFile> fileList, out object obj) {
            return DataProvider.ExportChunk(chunk, fileList, out obj);
        }
        public void ImportChunk(int chunk, SerializableList<SerializableFile> fileList, object obj) {
            DataProvider.ImportChunk(chunk, fileList, obj);
        }
    }
}