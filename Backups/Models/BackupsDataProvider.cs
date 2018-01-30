﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Backups#License */

using System;
using System.Collections.Generic;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;

namespace YetaWF.Modules.Backups.DataProvider {

    public class BackupEntry {
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
    }

    public class BackupsDataProvider : DataProviderImpl {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public BackupsDataProvider() : base(0) { SetDataProvider(CreateDataProvider()); }

        private IDataProvider<string, BackupEntry> DataProvider { get { return GetDataProvider(); } }

        private IDataProvider<string, BackupEntry> CreateDataProvider() {
            Package package = YetaWF.Modules.Backups.Controllers.AreaRegistration.CurrentPackage;
            return MakeDataProvider(package, package.AreaName);
        }

        // API
        // API
        // API

        public List<BackupEntry> GetBackups(int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters, out int total) {

            File.FileDataProvider.BackupsDataProvider fileDP = DataProvider as File.FileDataProvider.BackupsDataProvider;
            if (fileDP == null)
                throw new InternalError($"{nameof(BackupsDataProvider)} only supports File I/O");

            return fileDP.GetBackups(skip, take, sorts, filters, out total);
        }
    }
}
