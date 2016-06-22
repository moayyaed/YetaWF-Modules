﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Search#License */

using System;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;

namespace YetaWF.Modules.Search.Modules {

    public class SearchResultsModuleDataProvider : ModuleDefinitionDataProvider<Guid, SearchResultsModule>, IInstallableModel { }

    [ModuleGuid("{5f786472-884b-47db-9704-d50690003dc9}")]
    [UniqueModule(UniqueModuleStyle.NonUnique)]
    public class SearchResultsModule : ModuleDefinition {

        public SearchResultsModule() {
            Title = this.__ResStr("modTitle", "Search Results");
            Name = this.__ResStr("modName", "Search Results");
            Description = this.__ResStr("modSummary", "Displays search results");
            WantFocus = false;
            WantSearch = false;
        }

        public override IModuleDefinitionIO GetDataProvider() { return new SearchResultsModuleDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }

        public ModuleAction GetAction_GetResults(string url, string searchTerms) {
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                QueryArgs = new { SearchTerms = searchTerms },
                Image = "SearchResults.png",
                LinkText = this.__ResStr("resultsLink", "Search Results"),
                MenuText = this.__ResStr("resultsText", "Search Results"),
                Tooltip = this.__ResStr("resultsTooltip", "Display the search results for \"{0}\"", searchTerms),
                Legend = this.__ResStr("resultsLegend", "Displays the search results for \"{0}\"", searchTerms),
                Style = ModuleAction.ActionStyleEnum.Normal,
                Category = ModuleAction.ActionCategoryEnum.Read,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
            };
        }
    }
}