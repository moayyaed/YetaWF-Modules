/* Copyright �2020 Softel vdm, Inc.. - https://yetawf.com/Documentation/YetaWF/Pages#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Audit;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.IO;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Pages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Skins;
using YetaWF.Core.Support;

namespace YetaWF.Modules.Pages.DataProvider {

    public class UnifiedSetData {

        public const int MaxUnifiedSetGuid = 50;
        public const int MaxName = 80;
        public const int MaxDescription = 1000;

        /// <summary>
        /// The internal id for this page set.
        /// </summary>
        [Data_PrimaryKey, StringLength(MaxUnifiedSetGuid)]
        public Guid UnifiedSetGuid { get; set; }

        /// <summary>
        /// User provided name for this page set.
        /// </summary>
        [StringLength(MaxName)]
        public string Name { get; set; }

        /// <summary>
        /// User provided description for this page set.
        /// </summary>
        [StringLength(MaxDescription)]
        public string Description { get; set; }

        /// <summary>
        /// Defines the master page for the unified page set. All referenced modules, skin and other attributes for all pages are defined by this page.
        /// </summary>
        public Guid MasterPageGuid { get; set; }

        /// <summary>
        /// Defines the skin that combines all pages into this Unified Page Set (used with SkinDynamicContent only).
        /// </summary>
        public SkinDefinition PageSkin { get; set; }

        /// <summary>
        /// Defines whether this Unified Page Set is currently disabled.
        /// </summary>
        [Data_NewValue]
        public bool Disabled { get; set; }

        /// <summary>
        /// Defines how combined content is rendered.
        /// </summary>
        public PageDefinition.UnifiedModeEnum UnifiedMode { get; set; }

        /// <summary>
        /// Defines whether popups are part of this Unified Page Set (used with SkinDynamicContent and DynamicContent only).
        /// </summary>
        [Data_NewValue]
        public bool Popups { get; set; }

        /// <summary>
        /// The duration of the animation (in milliseconds) when scrolling to content.
        /// </summary>
        /// <remarks>Scrolling to the end of the page would take the specified number of milliseconds.
        /// If the scrolling distance is less than the entire page, the duration is proportionally reduced so the scroll speed would be similar in all cases.
        /// </remarks>
        public int UnifiedAnimation { get; set; }

        /// <summary>
        /// Date/time this entry was created.
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Date/time this entry was last updated.
        /// </summary>
        public DateTime Updated { get; set; }

        /// <summary>
        /// The list of pages (by Url) which are part of this Unified Page Set.
        /// </summary>
        /// <remarks>They are in the order that content is rendered within a unified page.</remarks>
        [Data_DontSave]
        public List<string> PageList { get; set; }
        /// <summary>
        /// The list of pages (by Guid) which are part of this Unified Page Set.
        /// </summary>
        /// <remarks>They are in the order that content is rendered within a unified page.</remarks>
        [Data_Binary]
        public SerializableList<Guid> PageGuids { get; set; }

        public UnifiedSetData() {
            UnifiedMode = PageDefinition.UnifiedModeEnum.SkinDynamicContent;
            Popups = false;
            UnifiedAnimation = 1000;
            PageList = new List<string>();
            PageGuids = new SerializableList<Guid>();
            PageSkin = new SkinDefinition();
        }
    }

    public class UnifiedSetDataProviderStartup : IInitializeApplicationStartup {

        // IINITIALIZEAPPLICATIONSTARTUP
        // IINITIALIZEAPPLICATIONSTARTUP
        // IINITIALIZEAPPLICATIONSTARTUP

        public Task InitializeApplicationStartupAsync() {
            PageDefinition.GetUnifiedPageInfoAsync = UnifiedSetDataProvider.GetUnifiedPageInfoAsync;
            return Task.CompletedTask;
        }
    }

    public class UnifiedSetDataProvider : DataProviderImpl, IInstallableModel {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public UnifiedSetDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(CreateDataProvider()); }
        public UnifiedSetDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(CreateDataProvider()); }

        private IDataProvider<Guid, UnifiedSetData> DataProvider { get { return GetDataProvider(); } }

        private IDataProvider<Guid, UnifiedSetData> CreateDataProvider() {
            Package package = YetaWF.Modules.Pages.Controllers.AreaRegistration.CurrentPackage;
            return MakeDataProvider(package, package.AreaName + "_UnifiedSets", SiteIdentity: SiteIdentity, Cacheable: true);
        }

        // API
        // API
        // API

        public async Task<UnifiedSetData> GetItemAsync(Guid unifiedSetGuid) {
            UnifiedSetData unifiedSet = await DataProvider.GetAsync(unifiedSetGuid);
            if (unifiedSet == null) return null;
            unifiedSet.PageList = await GetPageListFromGuidsAsync(unifiedSet.PageGuids);
            return unifiedSet;
        }
        public async Task<bool> AddItemAsync(UnifiedSetData unifiedSet) {
            unifiedSet.UnifiedSetGuid = Guid.NewGuid();
            unifiedSet.Created = DateTime.UtcNow;
            if (unifiedSet.UnifiedMode == PageDefinition.UnifiedModeEnum.SkinDynamicContent) {
                unifiedSet.PageList = new List<string>();
            } else {
                unifiedSet.PageSkin = new SkinDefinition();
            }
            unifiedSet.PageGuids = await UpdatePageGuidsAsync(unifiedSet.UnifiedSetGuid, unifiedSet.PageList);
            if (!await DataProvider.AddAsync(unifiedSet)) return false;

            await Auditing.AddAuditAsync($"{nameof(UnifiedSetDataProvider)}.{nameof(AddItemAsync)}", Dataset, Guid.Empty,
                $"Add UPS {unifiedSet.Name}",
                DataBefore: null,
                DataAfter: unifiedSet
            );
            return true;
        }
        public async Task<UpdateStatusEnum> UpdateItemAsync(UnifiedSetData unifiedSet) {

            UnifiedSetData origData = Auditing.Active ? await GetItemAsync(unifiedSet.UnifiedSetGuid) : null;

            unifiedSet.Updated = DateTime.UtcNow;
            if (unifiedSet.UnifiedMode == PageDefinition.UnifiedModeEnum.SkinDynamicContent) {
                unifiedSet.PageList = new List<string>();
            } else {
                unifiedSet.PageSkin = new SkinDefinition();
            }
            unifiedSet.PageGuids = await UpdatePageGuidsAsync(unifiedSet.UnifiedSetGuid, unifiedSet.PageList);
            UpdateStatusEnum status = await DataProvider.UpdateAsync(unifiedSet.UnifiedSetGuid, unifiedSet.UnifiedSetGuid, unifiedSet);
            if (status != UpdateStatusEnum.OK) return status;

            await Auditing.AddAuditAsync($"{nameof(UnifiedSetDataProvider)}.{nameof(UpdateItemAsync)}", Dataset, Guid.Empty,
                $"Update UPS {unifiedSet.Name}",
                DataBefore: origData,
                DataAfter: unifiedSet
            );
            return status;
        }
        public async Task<bool> RemoveItemAsync(Guid unifiedSetGuid) {

            UnifiedSetData origData = Auditing.Active ? await GetItemAsync(unifiedSetGuid) : null;

            if (!await DataProvider.RemoveAsync(unifiedSetGuid)) return false;
            await RemoveGuidAsync(unifiedSetGuid);

            await Auditing.AddAuditAsync($"{nameof(UnifiedSetDataProvider)}.{nameof(RemoveItemAsync)}", Dataset, Guid.Empty,
                $"Remove UPS {unifiedSetGuid}",
                DataBefore: origData,
                DataAfter: null
            );
            return true;
        }
        public async Task<DataProviderGetRecords<UnifiedSetData>> GetItemsAsync(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters) {
            return await DataProvider.GetRecordsAsync(skip, take, sort, filters);
        }

        // PageList
        // PageList
        // PageList

        static internal async Task<PageDefinition.UnifiedInfo> GetUnifiedPageInfoAsync(Guid? unifiedSetGuid, string collectionName, string skinName) {
            using (UnifiedSetDataProvider unifiedSetDP = new UnifiedSetDataProvider()) {
                UnifiedSetData unifiedSet;
                if (unifiedSetGuid != null) {
                    unifiedSet = await unifiedSetDP.GetItemAsync((Guid)unifiedSetGuid);
                    if (unifiedSet != null) {
                        return new PageDefinition.UnifiedInfo {
                            UnifiedSetGuid = (Guid)unifiedSetGuid,
                            Disabled = unifiedSet.Disabled,
                            MasterPageGuid = unifiedSet.MasterPageGuid,
                            PageGuids = unifiedSet.PageGuids,
                            Popups = unifiedSet.Popups,
                            Animation = unifiedSet.UnifiedAnimation,
                            Mode = unifiedSet.UnifiedMode,
                        };
                    }
                }
                // some pages (created with earlier versions of YetaWF) have a null skin name, which defaults to SkinAccess.FallbackPageFileName
                if (string.IsNullOrWhiteSpace(skinName))
                    skinName = SkinAccess.FallbackPageFileName;
                // find a unified page set that uses the matching skin
                List<DataProviderFilterInfo> filters = null;
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = $"{nameof(UnifiedSetData.PageSkin)}.{nameof(SkinDefinition.Collection)}", Operator = "==", Value = collectionName });
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = nameof(UnifiedSetData.UnifiedMode), Operator = "==", Value = PageDefinition.UnifiedModeEnum.SkinDynamicContent });

                List<DataProviderFilterInfo> subFilters = null;
                // we need to check for Default and Default.cshtml in case we still have old data (pre razor removal)
                subFilters = DataProviderFilterInfo.Join(subFilters, new DataProviderFilterInfo { Field = $"{nameof(UnifiedSetData.PageSkin)}.{nameof(SkinDefinition.FileName)}", Operator = "==", Value = skinName }, SimpleLogic: "||");
                subFilters = DataProviderFilterInfo.Join(subFilters, new DataProviderFilterInfo { Field = $"{nameof(UnifiedSetData.PageSkin)}.{nameof(SkinDefinition.FileName)}", Operator = "==", Value = $"{skinName}.cshtml" }, SimpleLogic: "||" );

                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Logic = "&&", Filters = subFilters });

                DataProviderGetRecords<UnifiedSetData> recs = await unifiedSetDP.GetItemsAsync(0, 1, null, filters);
                unifiedSet = recs.Data.FirstOrDefault();
                if (unifiedSet != null) {
                    return new PageDefinition.UnifiedInfo {
                        UnifiedSetGuid = unifiedSet.UnifiedSetGuid,
                        MasterPageGuid = unifiedSet.MasterPageGuid,
                        PageGuids = new List<Guid>(),
                        Disabled = unifiedSet.Disabled,
                        Popups = unifiedSet.Popups,
                        Animation = 0,
                        Mode = unifiedSet.UnifiedMode,
                        PageSkinCollectionName = collectionName,
                        PageSkinFileName = skinName,
                    };
                }
            }
            return null;
        }
        private async Task<List<string>> GetPageListFromGuidsAsync(SerializableList<Guid> pageGuids) {
            List<string> pages = new List<string>();
            using (PageDefinitionDataProvider pageDP = new PageDefinitionDataProvider()) {
                foreach (Guid pageGuid in pageGuids) {
                    PageDefinition page = await pageDP.LoadPageDefinitionAsync(pageGuid);
                    if (page != null)
                        pages.Add(page.Url);
                }
            }
            return pages;
        }
        private async Task<SerializableList<Guid>> UpdatePageGuidsAsync(Guid unifiedSetGuid, List<string> pageList) {
            pageList = pageList != null ? pageList : new List<string>();
            SerializableList<Guid> pageGuids = new SerializableList<Guid>();
            using (PageDefinitionDataProvider pageDP = new PageDefinitionDataProvider()) {
                // Get all pages that are currently part of the unified page set
                List<DataProviderFilterInfo> filters = null;
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = nameof(PageDefinition.UnifiedSetGuid), Operator = "==", Value = unifiedSetGuid });
                DataProviderGetRecords<PageDefinition> pageDefs = await pageDP.GetItemsAsync(0, 0, null, filters);
                // translate page list to guid list (preserving order)
                foreach (string page in pageList) {
                    PageDefinition pageDef = await pageDP.LoadPageDefinitionAsync(page);
                    if (pageDef != null) {
                        pageGuids.Add(pageDef.PageGuid);
                        // check if it's already in the list
                        PageDefinition pageFound = (from p in pageDefs.Data where p.Url == page select p).FirstOrDefault();
                        if (pageFound == null) {
                            // page not in list, add it
                            pageDef.UnifiedSetGuid = unifiedSetGuid;
                            await pageDP.SavePageDefinitionAsync(pageDef);
                        } else if (pageFound.UnifiedSetGuid != unifiedSetGuid) {
                            // page in list but with the wrong unifiedSetGuid
                            pageDef.UnifiedSetGuid = unifiedSetGuid;
                            await pageDP.SavePageDefinitionAsync(pageDef);
                            pageDefs.Data.Remove(pageFound);
                        } else {
                            // page already in list
                            pageDefs.Data.Remove(pageFound);
                        }
                    }
                }
                // remove all remaining pages from unified page set, they're no longer in the list
                foreach (PageDefinition pageDef in pageDefs.Data) {
                    pageDef.UnifiedSetGuid = null;
                    await pageDP.SavePageDefinitionAsync(pageDef);
                }
            }
            return pageGuids;
        }
        private async Task RemoveGuidAsync(Guid unifiedSetGuid) {
            using (PageDefinitionDataProvider pageDP = new PageDefinitionDataProvider()) {
                // Get all pages that are part of the unified page set
                List<DataProviderFilterInfo> filters = null;
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = nameof(PageDefinition.UnifiedSetGuid), Operator = "==", Value = unifiedSetGuid });
                DataProviderGetRecords<PageDefinition> pageDefs = await pageDP.GetItemsAsync(0, 0, null, filters);
                // remove all pages from unified page set if they're not within the page list
                foreach (PageDefinition pageDef in pageDefs.Data) {
                    pageDef.UnifiedSetGuid = null;
                    await pageDP.SavePageDefinitionAsync(pageDef);
                }
            }
        }
    }
}
