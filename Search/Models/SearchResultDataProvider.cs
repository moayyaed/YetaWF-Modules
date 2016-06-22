﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Search#License */

using System;
using System.Collections.Generic;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;

namespace YetaWF.Modules.Search.DataProvider {

    public class SearchResult {

        public int Count { get; set; }
        [UIHint("Url")]
        public string PageUrl { get; set; }
        [UIHint("String")]
        public string Description { get; set; }
        [UIHint("DateTime")]
        public DateTime DateCreated { get; set; }
        [UIHint("DateTime")]
        public DateTime? DateUpdated { get; set; }

        public SearchResult() { }
    }

    public partial class SearchResultDataProvider : DataProviderImpl {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public SearchResultDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }
        public SearchResultDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(DataProvider); }

        private IDataProviderIdentity<int, object, int, SearchResult> DataProvider {
            get {
                if (_dataProvider == null) {
                    Package package = Package.GetPackageFromAssembly(GetType().Assembly);
                    switch (GetIOMode(package.AreaName + "_Urls")) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            throw new InternalError("File I/O is not supported");
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLIdentityObjectDataProvider<int, object, int, SearchResult>(AreaName, SQLDbo, SQLConn,
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                    }
                }
                return _dataProvider;
            }
        }

        private IDataProviderIdentity<int, object, int, SearchResult> _dataProvider { get; set; }

        // API
        // API
        // API

        public List<SearchResult> GetSearchResults(string searchTerms, int maxResults, string languageId, bool haveUser, out bool haveMore, List<DataProviderFilterInfo> Filters = null) {
            haveMore = false;
            List<SearchResult> results = Parse(searchTerms, maxResults, languageId, haveUser, out haveMore, Filters);
            return results;
        }
    }
}