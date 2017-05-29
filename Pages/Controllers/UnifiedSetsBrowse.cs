/* Copyright � 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Pages#License */

using System;
using System.Collections.Generic;
using System.Linq;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Menus;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Pages.DataProvider;
using YetaWF.Modules.Pages.Modules;
#if MVC6
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Pages.Controllers {

    public class UnifiedSetsBrowseModuleController : ControllerImpl<YetaWF.Modules.Pages.Modules.UnifiedSetsBrowseModule> {

        public class BrowseItem {

            [Caption("Actions"), Description("The available actions")]
            [UIHint("ActionIcons"), ReadOnly]
            public MenuList Commands {
                get {
                    MenuList actions = new MenuList() { RenderMode = ModuleAction.RenderModeEnum.IconsOnly };

                    UnifiedSetEditModule editMod = new UnifiedSetEditModule();
                    actions.New(editMod.GetAction_Edit(Module.EditUrl, UnifiedSetGuid), ModuleAction.ActionLocationEnum.GridLinks);

                    actions.New(Module.GetAction_Remove(UnifiedSetGuid), ModuleAction.ActionLocationEnum.GridLinks);
                    return actions;
                }
            }

            [Caption("Name"), Description("The name of this unified page set")]
            [UIHint("String"), ReadOnly]
            public string Name { get; set; }

            [Caption("Description"), Description("The description for this unified page set")]
            [UIHint("String"), ReadOnly]
            public string Description { get; set; }

            [Caption("Created"), Description("The date/time this set was created")]
            [UIHint("DateTime"), ReadOnly]
            public DateTime Created { get; set; }
            [Caption("Updated"), Description("The date/time this set was last updated")]
            [UIHint("DateTime"), ReadOnly]
            public DateTime Updated { get; set; }

            [Caption("Id"), Description("The internal id used to identify this unified page set")]
            [UIHint("Guid"), ReadOnly]
            public Guid UnifiedSetGuid { get; set; }

            private UnifiedSetsBrowseModule Module { get; set; }

            public BrowseItem(UnifiedSetsBrowseModule module, UnifiedSetData unifiedSet) {
                Module = module;
                ObjectSupport.CopyData(unifiedSet, this);
            }
        }

        [Header("A unified page set combines multiple, separately designed pages into one page. Whenever one of the pages included in the set is accessed by Url, the combined " +
            "pages will be rendered. When the user navigates between the pages in the set, no server access is required as all required portions of the pages have been preloaded. " +
            "Only modules within designated panes are combined into the resulting page, minimizing data transfer.")]
        public class BrowseModel {
            [Caption(""), Description("")] // empty entries required so property is shown in property list (but with a suppressed label)
            [UIHint("Grid"), ReadOnly]
            public GridDefinition GridDef { get; set; }
        }

        [HttpGet]
        public ActionResult UnifiedSetsBrowse() {
            BrowseModel model = new BrowseModel { };
            model.GridDef = new GridDefinition {
                AjaxUrl = GetActionUrl("UnifiedSetsBrowse_GridData"),
                ModuleGuid = Module.ModuleGuid,
                RecordType = typeof(BrowseItem),
                SettingsModuleGuid = Module.PermanentGuid,
            };
            return View(model);
        }

        [HttpPost]
        [ConditionalAntiForgeryToken]
        public ActionResult UnifiedSetsBrowse_GridData(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, Guid settingsModuleGuid) {
            using (UnifiedSetDataProvider unifiedSetDP = new UnifiedSetDataProvider()) {
                int total;
                List<UnifiedSetData> browseItems = unifiedSetDP.GetItems(skip, take, sort, filters, out total);
                GridHelper.SaveSettings(skip, take, sort, filters, settingsModuleGuid);
                return GridPartialView(new DataSourceResult {
                    Data = (from s in browseItems select new BrowseItem(Module, s)).ToList<object>(),
                    Total = total
                });
            }
        }

        [HttpPost]
        [Permission("RemoveItems")]
        public ActionResult Remove(Guid unifiedSetGuid) {
            using (UnifiedSetDataProvider unifiedSet = new UnifiedSetDataProvider()) {
                if (!unifiedSet.RemoveItem(unifiedSetGuid))
                    throw new Error(this.__ResStr("cantRemove", "Couldn't remove {0}", unifiedSetGuid));
                return Reload(null, Reload: ReloadEnum.ModuleParts);
            }
        }
    }
}