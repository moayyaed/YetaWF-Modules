﻿/* Copyright © 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/PageEdit#License */

using System;
using YetaWF.Core;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Pages;
using YetaWF.Core.Support;
using YetaWF.DataProvider;
using YetaWF.Modules.PageEdit.Controllers;

namespace YetaWF.Modules.PageEdit.Modules {

    public class PageEditModuleDataProvider : ModuleDefinitionDataProvider<Guid, PageEditModule>, IInstallableModel { }

    [ModuleGuid("{FBB3C6D3-FBD2-4ab1-BF0E-8716F3D1B052}")] // Published Guid
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    public class PageEditModule : ModuleDefinition {

        public PageEditModule() : base() {
            Title = this.__ResStr("modTitle", "Page Edit");
            Name = this.__ResStr("modName", "Page Edit");
            Description = this.__ResStr("modSummary", "Edit page settings");
        }

        public override IModuleDefinitionIO GetDataProvider() { return new PageEditModuleDataProvider(); }

        public override bool ShowModuleMenu { get { return false; } }

        public ModuleAction GetAction_Edit(string url, Guid? pageGuid = null) {
            Guid guid;
            if (pageGuid == null) {
                if (Manager.CurrentPage == null) return null;
                if (!Manager.CurrentPage.IsAuthorized_Edit()) return null;
                guid = Manager.CurrentPage.PageGuid;
            } else {
                guid = (Guid) pageGuid;
            }
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                QueryArgs = new { PageGuid = guid },
                QueryArgsRvd = new System.Web.Routing.RouteValueDictionary{
                    { Globals.Link_TempNoEditMode, "y" },
                },
                Image = "#Edit",
                LinkText = this.__ResStr("editLink", "Page Settings"),
                MenuText = this.__ResStr("editText", "Page Settings"),
                Tooltip = this.__ResStr("editTooltip", "Edit page settings"),
                Legend = this.__ResStr("editLegend", "Edits page settings"),

                Style = ModuleAction.ActionStyleEnum.PopupEdit,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Location = ModuleAction.ActionLocationEnum.ModuleLinks | ModuleAction.ActionLocationEnum.ModuleMenu,
                Mode = ModuleAction.ActionModeEnum.Any,

                SaveReturnUrl = true,
            };
        }
        public ModuleAction GetAction_Remove(Guid? pageGuid = null) {
            Guid guid;
            PageDefinition page;
            if (pageGuid == null) {
                page = Manager.CurrentPage;
                if (page == null) return null;
                guid = page.PageGuid;
            } else {
                guid = (Guid)pageGuid;
                page = PageDefinition.Load(guid);
            }
            if (page == null) return null;
            if (!page.IsAuthorized_Remove()) return null;
            return new ModuleAction(this) {
                Url = YetaWFManager.UrlFor(typeof(PageEditModuleController), "RemovePage"),
                QueryArgs = new { PageGuid = guid },
                Image = "#Remove",
                LinkText = this.__ResStr("delLink", "Remove Current Page"),
                MenuText = this.__ResStr("delText", "Remove Current Page"),
                Tooltip = this.__ResStr("delTooltip", "Remove the current page"),
                Legend = this.__ResStr("delLegend", "Removes the current page"),
                Style = ModuleAction.ActionStyleEnum.Post,
                Category = ModuleAction.ActionCategoryEnum.Delete,
                Location = ModuleAction.ActionLocationEnum.ModuleLinks | ModuleAction.ActionLocationEnum.ModuleMenu,
                Mode = ModuleAction.ActionModeEnum.Any,
                ConfirmationText = this.__ResStr("delConfirm", "Are you ABSOLUTELY sure you want to remove the currently displayed page \"{0}\"? This action cannot be undone.", page.Url),
                NeedsModuleContext = true,
            };
        }
    }
}