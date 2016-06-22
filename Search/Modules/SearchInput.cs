﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Search#License */

using System;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;

namespace YetaWF.Modules.Search.Modules {

    public class SearchInputModuleDataProvider : ModuleDefinitionDataProvider<Guid, SearchInputModule>, IInstallableModel { }

    [ModuleGuid("{c7991e91-c691-449a-a911-e5feacfba8a4}")]
    [UniqueModule(UniqueModuleStyle.NonUnique)]
    public class SearchInputModule : ModuleDefinition {

        public SearchInputModule() {
            Title = this.__ResStr("modTitle", "Search");
            Name = this.__ResStr("modName", "Search");
            Description = this.__ResStr("modSummary", "Search terms entry");
            WantFocus = false;
            WantSearch = false;
            Print = false;
            SearchButtonText = this.__ResStr("searchButton", "Search");
            SearchButtonTT = this.__ResStr("searchButtonTT", "Enter search keywords and click the Search button to search this site - You can use AND and OR to combine multiple terms");
        }

        public override IModuleDefinitionIO GetDataProvider() { return new SearchInputModuleDataProvider(); }

        [Category("General"), Caption("Search Button"), Description("The text shown on the search button")]
        [UIHint("MultiString20"), StringLength(20), Required]
        public MultiString SearchButtonText { get; set; }

        [Category("General"), Caption("Search Button Tooltip"), Description("The tooltip shown for the search button")]
        [UIHint("MultiString80"), StringLength(500)]
        public MultiString SearchButtonTT { get; set; }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }

        public ModuleAction GetAction_Search(string url, string searchTerms) {
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                QueryArgs = new { SearchTerms = searchTerms },
                Image = "SearchInput.png",
                LinkText = this.__ResStr("editLink", "Search"),
                MenuText = this.__ResStr("editText", "Search"),
                Tooltip = this.__ResStr("editTooltip", "Search for keywords on this site"),
                Legend = this.__ResStr("editLegend", "Allows you to search this site for keywords"),
                Style = ModuleAction.ActionStyleEnum.Normal,
                Category = ModuleAction.ActionCategoryEnum.Read,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
            };
        }
    }
}