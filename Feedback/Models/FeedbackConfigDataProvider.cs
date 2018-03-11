﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Feedback#License */

using System.Threading.Tasks;
using YetaWF.Core;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.IO;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;

namespace YetaWF.Modules.Feedback.DataProvider {

    public class FeedbackConfigData {

        public const int MaxProp1 = 100;
        public const int MaxProp2 = 100;

        [Data_PrimaryKey]
        public int Id { get; set; }

        public bool Captcha { get; set; }
        public bool RequireEmail { get; set; }
        public bool BccEmails { get; set; }
        [StringLength(Globals.MaxEmail)]
        public string Email { get; set; }

        public FeedbackConfigData() {
            RequireEmail = true;
            Captcha = true;
            BccEmails = false;
        }
    }

    public class FeedbackConfigDataProvider : DataProviderImpl, IInstallableModel {

        private const int KEY = 1;

        private static AsyncLock _lockObject = new AsyncLock();

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public FeedbackConfigDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(CreateDataProvider()); }
        public FeedbackConfigDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(CreateDataProvider()); }

        private IDataProvider<int, FeedbackConfigData> DataProvider { get { return GetDataProvider(); } }

        private IDataProvider<int, FeedbackConfigData> CreateDataProvider() {
            Package package = YetaWF.Modules.Feedback.Controllers.AreaRegistration.CurrentPackage;
            return MakeDataProvider(package, package.AreaName + "_Config", SiteIdentity: SiteIdentity, Cacheable: true);
        }

        // API
        // API
        // API

        public static async Task<FeedbackConfigData> GetConfigAsync() {
            using (FeedbackConfigDataProvider configDP = new FeedbackConfigDataProvider()) {
                return await configDP.GetItemAsync();
            }
        }
        public async Task<FeedbackConfigData> GetItemAsync() {
            FeedbackConfigData config = await DataProvider.GetAsync(KEY);
            if (config == null) {
                using (await _lockObject.LockAsync()) {
                    config = await DataProvider.GetAsync(KEY);
                    if (config == null) {
                        config = new FeedbackConfigData();
                        await AddConfigAsync(config);
                    }
                }
            }
            return config;
        }
        private async Task AddConfigAsync(FeedbackConfigData data) {
            data.Id = KEY;
            if (!await DataProvider.AddAsync(data))
                throw new InternalError("Unexpected error adding settings");
        }
        public async Task UpdateConfigAsync(FeedbackConfigData data) {
            data.Id = KEY;
            UpdateStatusEnum status = await DataProvider.UpdateAsync(data.Id, data.Id, data);
            if (status != UpdateStatusEnum.OK)
                throw new InternalError("Unexpected error saving configuration {0}", status);
        }
    }
}
