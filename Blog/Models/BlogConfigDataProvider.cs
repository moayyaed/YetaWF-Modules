﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Blog#License */

using System.Collections.Generic;
using System.IO;
using YetaWF.Core;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.Extensions;
using YetaWF.Core.Image;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Modules.Blog.Modules;

namespace YetaWF.Modules.Blog.DataProvider {

    public class BlogConfigData : IInitializeApplicationStartup {

        private string __ResStr(string name, string defaultValue, params object[] parms) {
            return ResourceAccess.GetResourceString(typeof(BlogConfigDataProvider), name, defaultValue, parms); 
        }

        // IInitializeApplicationStartup
        public const string ImageType = "YetaWF_Blog_BlogConfigData";

        public void InitializeApplicationStartup() {
            ImageSupport.AddHandler(ImageType, GetBytes: RetrieveImage);
        }
        private bool RetrieveImage(string name, string location, out byte[] content) {
            content = null;
            if (!string.IsNullOrWhiteSpace(location)) return false;
            if (string.IsNullOrWhiteSpace(name)) return false;
            BlogConfigData config = BlogConfigDataProvider.GetConfig();
            if (config.FeedImage_Data == null || config.FeedImage_Data.Length == 0) return false;
            content = config.FeedImage_Data;
            return true;
        }

        public const int MaxFeedTitle = 80;
        public const int MaxFeedSummary = 200;

        [Data_PrimaryKey]
        public int Id { get; set; }

        [StringLength(Globals.MaxUrl)]
        public string BlogUrl { get; set; }
        public int DefaultCategory { get; set; }
        public int Entries { get; set; }
        [StringLength(Globals.MaxUrl)]
        public string BlogEntryUrl { get; set; }

        public bool ShowGravatar { get; set; }
        public Gravatar.GravatarEnum GravatarDefault { get; set; }
        public Gravatar.GravatarRatingEnum GravatarRating { get; set; }
        public int GravatarSize { get; set; }

        public bool Feed { get; set; }
        [StringLength(MaxFeedTitle)]
        public string FeedTitle { get; set; }
        [StringLength(MaxFeedSummary)]
        public string FeedSummary { get; set; }

        [StringLength(Globals.MaxUrl)]
        public string FeedMainUrl { get; set; }
        [StringLength(Globals.MaxUrl)]
        public string FeedDetailUrl { get; set; }

        [UIHint("Image")]
        [DontSave]
        public string FeedImage {
            get {
                if (_feedImage == null) {
                    if (FeedImage_Data != null && FeedImage_Data.Length > 0)
                        _feedImage = ModuleDefinition.GetPermanentGuid(typeof(BlogConfigModule)).ToString() + ",FeedImage_Data";
                }
                return _feedImage;
            }
            set {
                _feedImage = value;
            }
        }
        private string _feedImage = null;

        [Data_Binary]
        public byte[] FeedImage_Data { get; set; }

        public BlogConfigData() {
            BlogUrl = null;
            Entries = 20;
            BlogEntryUrl = null;
            ShowGravatar = true;
            GravatarDefault = Gravatar.GravatarEnum.wavatar;
            GravatarRating = Gravatar.GravatarRatingEnum.G;
            GravatarSize = 40;
            FeedTitle = this.__ResStr("feedTitle", "(Blog Title)");
            Feed = false;
            FeedSummary = this.__ResStr("feedSummary", "(Blog Summary)");
            FeedMainUrl = null;
            FeedDetailUrl = null;
            FeedImage_Data = new byte[0];
        }

        internal static string GetCategoryCanonicalName(int blogCategory = 0) {
            using (BlogCategoryDataProvider categoryDP = new BlogCategoryDataProvider()) {
                BlogConfigData config = BlogConfigDataProvider.GetConfig();
                string canon = config.BlogUrl;
                if (blogCategory != 0) {
                    BlogCategory cat = categoryDP.GetItem(blogCategory);
                    if (cat != null)
                        canon = string.Format("{0}/Title/{1}/?BlogCategory={2}", config.BlogUrl, YetaWFManager.UrlEncodeSegment(cat.Category.ToString().Truncate(80)), blogCategory);
                } else {
                    canon = string.Format("{0}?BlogCategory=0", config.BlogUrl);
                }
                return canon;
            }
        }
        internal static string GetEntryCanonicalName(int blogEntry) {
            BlogConfigData config = BlogConfigDataProvider.GetConfig();
            string canon = string.Format("{0}/?BlogEntry={1}", config.BlogEntryUrl, blogEntry);
            using (BlogEntryDataProvider dataProvider = new BlogEntryDataProvider()) {
                BlogEntry data = dataProvider.GetItem(blogEntry);
                if (data != null)
                    canon = string.Format("{0}/Title/{1}/?BlogEntry={2}", config.BlogEntryUrl, YetaWFManager.UrlEncodeSegment(data.Title.ToString().Truncate(80)), blogEntry);
                return canon;
            }
        }
    }

    public class BlogConfigDataProvider : DataProviderImpl, IInstallableModel {

        private const int KEY = 1;

        private static object _lockObject = new object();

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public BlogConfigDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }
        public BlogConfigDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(DataProvider); }

        private IDataProvider<int, BlogConfigData> DataProvider {
            get {
                if (_dataProvider == null) {
                    Package package = Package.GetPackageFromAssembly(GetType().Assembly);
                    switch (GetIOMode(package.AreaName)) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            _dataProvider = new YetaWF.DataProvider.FileDataProvider<int, BlogConfigData>(
                                Path.Combine(YetaWFManager.DataFolder, AreaName + "_Config", SiteIdentity.ToString()),
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLSimpleObjectDataProvider<int, BlogConfigData>(AreaName + "_Config", SQLDbo, SQLConn,
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                    }
                }
                return _dataProvider;
            }
        }
        private IDataProvider<int, BlogConfigData> _dataProvider { get; set; }

        // LOAD/SAVE
        // LOAD/SAVE
        // LOAD/SAVE

        public static BlogConfigData GetConfig() {
            using (BlogConfigDataProvider configDP = new BlogConfigDataProvider()) {
                return configDP.GetItem();
            }
        }
        public BlogConfigData GetItem() {
            BlogConfigData config = DataProvider.Get(KEY);
            if (config == null) {
                lock (_lockObject) {
                    config = DataProvider.Get(KEY);
                    if (config == null) {
                        config = new BlogConfigData();
                        AddConfig(config);
                    }
                }
            }
            return config;
        }
        private void AddConfig(BlogConfigData data) {
            data.Id = KEY;
            SaveImages(ModuleDefinition.GetPermanentGuid(typeof(BlogConfigModule)), data);
            if (!DataProvider.Add(data))
                throw new InternalError("Unexpected error adding settings");
        }
        public void UpdateConfig(BlogConfigData data) {
            data.Id = KEY;
            SaveImages(ModuleDefinition.GetPermanentGuid(typeof(BlogConfigModule)), data);
            UpdateStatusEnum status = DataProvider.Update(data.Id, data.Id, data);
            if (status != UpdateStatusEnum.OK)
                throw new InternalError("Unexpected error saving settings {0}", status);
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
        public void AddSiteData() {
            DataProvider.AddSiteData();
        }
        public void RemoveSiteData() {
            DataProvider.RemoveSiteData();
        }
        public bool UninstallModel(List<string> errorList) {
            return DataProvider.UninstallModel(errorList);
        }
        public bool ExportChunk(int chunk, SerializableList<SerializableFile> fileList, out object obj) {
            return DataProvider.ExportChunk(chunk, fileList, out obj);
        }
        public void ImportChunk(int chunk, SerializableList<SerializableFile> fileList, object obj) {
            DataProvider.ImportChunk(chunk, fileList, obj);
        }
    }
}