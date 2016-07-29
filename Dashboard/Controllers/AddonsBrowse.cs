/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Dashboard#License */

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using YetaWF.Core.Addons;
using YetaWF.Core.Controllers;
using YetaWF.Core.Menus;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Dashboard.Modules;
using static YetaWF.Core.Addons.VersionManager;

namespace YetaWF.Modules.Dashboard.Controllers {

    public class AddonsBrowseModuleController : ControllerImpl<YetaWF.Modules.Dashboard.Modules.AddonsBrowseModule> {

        public class BrowseItem {

            [Caption("Actions"), Description("The available actions")]
            [UIHint("ActionIcons"), ReadOnly]
            public MenuList Commands {
                get {
                    MenuList actions = new MenuList() { RenderMode = ModuleAction.RenderModeEnum.IconsOnly };

                    AddonDisplayModule dispMod = new AddonDisplayModule();
                    actions.New(dispMod.GetAction_Display(Module.DisplayUrl, AddonKey), ModuleAction.ActionLocationEnum.GridLinks);

                    return actions;
                }
            }

            public string AddonKey { get; set; }

            [Caption("Type"), Description("The AddOn type")]
            [UIHint("Enum"), ReadOnly]
            public AddOnType Type { get; set; }
            [Caption("Domain"), Description("The domain owning this AddOn")]
            [UIHint("String"), ReadOnly]
            public string Domain { get; set; }
            [Caption("Product"), Description("The AddOn's product name")]
            [UIHint("String"), ReadOnly]
            public string Product { get; set; }
            [Caption("Version"), Description("The AddOn's version")]
            [UIHint("String"), ReadOnly]
            public string Version { get; set; }
            [Caption("Name"), Description("The AddOn's internal name")]
            [UIHint("String"), ReadOnly]
            public string Name { get; set; }
            [Caption("Url"), Description("The AddOn's Url where its files are located")]
            [UIHint("String"), ReadOnly]
            public string Url { get; set; }

            private AddonsBrowseModule Module { get; set; }

            public BrowseItem(AddonsBrowseModule module, AddOnProduct data) {
                Module = module;
                ObjectSupport.CopyData(data, this);
            }
        }

        public class BrowseModel {

            [Caption("AddOns Url"), Description("The Url containing all AddOns")]
            [UIHint("String"), ReadOnly]
            public string AddOnsUrl { get; set; }

            [Caption("Custom AddOns Url"), Description("The Url containing all customized AddOns (if any)")]
            [UIHint("String"), ReadOnly]
            public string AddOnsCustomUrl { get; set; }

            [Caption("Script Url"), Description("The Url containing javascript files installed using Nuget")]
            [UIHint("String"), ReadOnly]
            public string NugetScriptsUrl { get; set; }

            [Caption("Installed AddOns"), Description("Displays all installed AddOns")]
            [UIHint("Grid"), ReadOnly]
            public GridDefinition GridDef { get; set; }
        }

        [HttpGet]
        public ActionResult AddonsBrowse() {

            List<AddOnProduct> list = VersionManager.GetAvailableAddOns();
            DataSourceResult data = new DataSourceResult {
                Data = (from l in list select new BrowseItem(Module, l)).ToList<object>(),
                Total = list.Count,
            };
            BrowseModel model = new BrowseModel {
                AddOnsUrl = VersionManager.AddOnsUrl,
                AddOnsCustomUrl = VersionManager.AddOnsCustomUrl,
                NugetScriptsUrl = VersionManager.NugetScriptsUrl,
            };
            model.GridDef = new GridDefinition {
                SupportReload = false,
                Data = data,
                RecordType = typeof(BrowseItem),
                InitialPageSize = 20,
            };
            return View(model);
        }
    }
}
