﻿/* Copyright © 2019 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Addons;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Models;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Core.Support.Repository;

namespace YetaWF.Modules.ComponentsHTML.Components {

    /// <summary>
    /// This static class defines basic services offered by the Grid component.
    /// </summary>
    internal static class GridLoadSave {

        private static YetaWFManager Manager { get { return YetaWFManager.Manager; } }

        /// <summary>
        /// This class implements grid layout, sorting and filtering information, so this information can be saved when it is updated by user actions, so it can be restored later (for example, if a page is reloaded).
        /// Applications should not manipulate these settings directly.
        /// </summary>
        public class GridSavedSettings {

            /// <summary>
            /// Defines the columns and their current settings.
            /// </summary>
            public GridDefinition.ColumnDictionary Columns { get; set; }

            /// <summary>
            /// Defines the current grid page size, i.e., the maximum number of records shown per page.
            /// </summary>
            public int PageSize { get; set; }

            /// <summary>
            /// Defines the current page number shown. Page numbers are 1 based. These should be 0 based as they are 0 based in other classes, but oh well. Someday....
            /// </summary>
            public int CurrentPage { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public GridSavedSettings() {
                Columns = new GridDefinition.ColumnDictionary();
                PageSize = 10;
                CurrentPage = 1;
            }

            /// <summary>
            /// Returns the current sort order for columns.
            /// </summary>
            /// <returns>A list of columns that have a defined sort order.</returns>
            public List<DataProviderSortInfo> GetSortInfo() {
                foreach (var keyVal in Columns) {
                    string colName = keyVal.Key;
                    GridDefinition.ColumnInfo col = keyVal.Value;
                    if (col.Sort != GridDefinition.SortBy.NotSpecified) {
                        return new List<DataProviderSortInfo>() {
                            new DataProviderSortInfo {
                                Field = colName,
                                Order = col.Sort == GridDefinition.SortBy.Descending ? DataProviderSortInfo.SortDirection.Descending : DataProviderSortInfo.SortDirection.Ascending ,
                            },
                        };
                    }
                }
                return null;
            }
            /// <summary>
            /// Returns the current filter settings for columns.
            /// </summary>
            /// <returns>A list of columns that have a defined filter setting.</returns>
            public List<DataProviderFilterInfo> GetFilterInfo() {
                List<DataProviderFilterInfo> list = new List<DataProviderFilterInfo>();
                foreach (var keyVal in Columns) {
                    string colName = keyVal.Key;
                    GridDefinition.ColumnInfo col = keyVal.Value;
                    if (!string.IsNullOrWhiteSpace(col.FilterOperator)) {
                        list.Add(new DataProviderFilterInfo {
                            Field = colName,
                            Operator = col.FilterOperator,
                            ValueAsString = col.FilterValue,
                        });
                    }
                }
                return list.Count > 0 ? list : null;
            }
        }
        /// <summary>
        /// Loads grid settings that have been previously saved for a specific module.
        /// If no saved settings are available, default settings are returned.
        /// </summary>
        /// <param name="moduleGuid">The module Guid of the module for which grid settings have been saved.</param>
        /// <param name="defaultInitialPage">Defines the default initial page within the grid. This page number is 1 based.</param>
        /// <param name="defaultPageSize">Defines the default initial page size of the grid.</param>
        /// <returns>Returns grid settings for the specified module.</returns>
        /// <remarks>Grid settings that are saved on behalf of modules are used whenever the module is displayed. This means that the same settings apply even if a module is used on several pages.
        ///
        /// This method is not used by applications. It is reserved for component implementation.</remarks>
        public static GridSavedSettings LoadModuleSettings(Guid moduleGuid, int defaultInitialPage = 1, int defaultPageSize = 10) {
            SettingsDictionary modSettings = Manager.SessionSettings.GetModuleSettings(moduleGuid);
            GridSavedSettings gridSavedSettings = modSettings.GetValue<GridSavedSettings>("GridSavedSettings");
            if (gridSavedSettings == null) {
                gridSavedSettings = new GridSavedSettings() {
                    CurrentPage = defaultInitialPage,
                    PageSize = defaultPageSize,
                };
            }
            return gridSavedSettings;
        }
        /// <summary>
        /// Save grid settings for a specific module.
        /// </summary>
        /// <param name="moduleGuid">The module Guid of the module for which grid settings are saved.</param>
        /// <param name="gridSavedSettings">The grid settings to be saved.</param>
        /// <remarks>This method is not used by applications. It is reserved for component implementation.</remarks>
        public static void SaveModuleSettings(Guid moduleGuid, GridSavedSettings gridSavedSettings) {
            SettingsDictionary modSettings = Manager.SessionSettings.GetModuleSettings(moduleGuid);
            if (modSettings != null) {
                modSettings.SetValue<GridSavedSettings>("GridSavedSettings", gridSavedSettings);
                modSettings.Save();
            }
        }

        public static void SaveSettings(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filter, Guid? settingsModuleGuid = null) {

            // save the current sort order and page size
            if (settingsModuleGuid != null && settingsModuleGuid != Guid.Empty) {
                GridLoadSave.GridSavedSettings gridSavedSettings = GridLoadSave.LoadModuleSettings((Guid)settingsModuleGuid);
                gridSavedSettings.PageSize = take;
                if (take == 0)
                    gridSavedSettings.CurrentPage = 1;
                else
                    gridSavedSettings.CurrentPage = Math.Max(1, skip / take + 1);
                foreach (GridDefinition.ColumnInfo col in gridSavedSettings.Columns.Values)
                    col.Sort = GridDefinition.SortBy.NotSpecified;
                if (sort != null) {
                    foreach (var sortCol in sort) {
                        GridDefinition.SortBy sortDir = (sortCol.Order == DataProviderSortInfo.SortDirection.Ascending) ? GridDefinition.SortBy.Ascending : GridDefinition.SortBy.Descending;
                        if (gridSavedSettings.Columns.ContainsKey(sortCol.Field))
                            gridSavedSettings.Columns[sortCol.Field].Sort = sortDir;
                        else
                            gridSavedSettings.Columns.Add(sortCol.Field, new GridDefinition.ColumnInfo { Sort = sortDir });
                    }
                }
                foreach (GridDefinition.ColumnInfo col in gridSavedSettings.Columns.Values) {
                    col.FilterOperator = null;
                    col.FilterValue = null;
                }
                if (filter != null) {
                    foreach (var filterCol in filter) {
                        if (gridSavedSettings.Columns.ContainsKey(filterCol.Field)) {
                            gridSavedSettings.Columns[filterCol.Field].FilterOperator = filterCol.Operator;
                            gridSavedSettings.Columns[filterCol.Field].FilterValue = filterCol.ValueAsString;
                        } else {
                            gridSavedSettings.Columns.Add(filterCol.Field, new GridDefinition.ColumnInfo {
                                FilterOperator = filterCol.Operator,
                                FilterValue = filterCol.ValueAsString,
                            });
                        }
                    }
                }
                GridLoadSave.SaveModuleSettings((Guid)settingsModuleGuid, gridSavedSettings);
            }
        }

        /// <summary>
        /// Loads the grid column definitions for a grid.
        /// </summary>
        /// <param name="gridDef">The GridDefinition object describing the grid.</param>
        /// <returns>A GridDictionaryInfo.ReadGridDictionaryInfo object describing the grid.</returns>
        /// <remarks>This method is not used by applications. It is reserved for component implementation.</remarks>
        public static async Task<GridDictionaryInfo.ReadGridDictionaryInfo> LoadGridColumnDefinitionsAsync(GridDefinition gridDef) {
            if (gridDef.CachedData == null)
                gridDef.CachedData = await LoadGridColumnDefinitionsAsync(gridDef.RecordType);
            return (GridDictionaryInfo.ReadGridDictionaryInfo)gridDef.CachedData;
        }

        /// <summary>
        /// Loads the grid column definitions for a grid based on its record type.
        /// </summary>
        /// <param name="recordType">The record type for which grid column definitions are to be loaded.</param>
        /// <returns>A GridDictionaryInfo.ReadGridDictionaryInfo object describing the grid.</returns>
        /// <remarks>This method is not used by applications. It is reserved for component implementation.</remarks>
        private static async Task<GridDictionaryInfo.ReadGridDictionaryInfo> LoadGridColumnDefinitionsAsync(Type recordType) {
            Dictionary<string, GridColumnInfo> dict = new Dictionary<string, GridColumnInfo>();
            string className = recordType.FullName.Split(new char[] { '.' }).Last();
            string[] s = className.Split(new char[] { '+' });
            int len = s.Length;
            if (len != 2) throw new InternalError("Unexpected class {0} in record type {1}", className, recordType.FullName);
            string controller = s[0];
            string model = s[1];
            string file = controller + "." + model;
            Package package = Package.GetPackageFromType(recordType);
            string predefUrl = VersionManager.GetAddOnPackageUrl(package.AreaName) + "Grids/" + file;
            string customUrl = VersionManager.GetCustomUrlFromUrl(predefUrl);
            GridDictionaryInfo.ReadGridDictionaryInfo info;
            GridDictionaryInfo.ReadGridDictionaryInfo predefInfo = await GridDictionaryInfo.ReadGridDictionaryAsync(package, recordType, Utility.UrlToPhysical(predefUrl));
            if (!predefInfo.Success)
                throw new InternalError("No grid definition exists for {0}", file);
            info = predefInfo;
            GridDictionaryInfo.ReadGridDictionaryInfo customInfo = await GridDictionaryInfo.ReadGridDictionaryAsync(package, recordType, Utility.UrlToPhysical(customUrl));
            if (customInfo.Success)
                info = customInfo;
            if (info.ColumnInfo.Count == 0)
                throw new InternalError("No grid definition exists for {0}", file);
            return info;
        }
    }
}