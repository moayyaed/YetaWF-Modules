/* Copyright � 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Feedback#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Menus;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Feedback.DataProvider;
using YetaWF.Modules.Feedback.Modules;
#if MVC6
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Feedback.Controllers {

    public class FeedbackBrowseModuleController : ControllerImpl<YetaWF.Modules.Feedback.Modules.FeedbackBrowseModule> {

        public class BrowseItem {

            [Caption("Actions"), Description("The available actions")]
            [UIHint("ActionIcons"), ReadOnly]
            public MenuList Commands {
                get {
                    MenuList actions = new MenuList() { RenderMode = ModuleAction.RenderModeEnum.IconsOnly };

                    FeedbackDisplayModule dispMod = new FeedbackDisplayModule();
                    actions.New(dispMod.GetAction_Display(Module.DisplayUrl, Key), ModuleAction.ActionLocationEnum.GridLinks);

                    actions.New(Module.GetAction_RemoveFeedback(Key), ModuleAction.ActionLocationEnum.GridLinks);
                    return actions;
                }
            }
            [Caption("Created"), Description("The date the message was created")]
            [UIHint("DateTime"), ReadOnly]
            public DateTime Created { get; set; }

            [Caption("Email Address"), Description("The user's email address")]
            [UIHint("String"), ReadOnly]
            public string Email { get; set; }

            [Caption("Subject"), Description("The subject of the message")]
            [UIHint("String"), ReadOnly]
            public string Subject { get; set; }

            [Caption("IP Address"), Description("The IP address from which the feedback message was sent")]
            [UIHint("IPAddress"), ReadOnly]
            public string IPAddress { get; set; }

            [Caption("Message"), Description("The feedback message")]
            [UIHint("String"), ReadOnly]
            public string Message { get; set; }

            private int Key { get; set; }
            private FeedbackBrowseModule Module { get; set; }

            public BrowseItem(FeedbackBrowseModule module, FeedbackData data) {
                Module = module;
                Key = data.Key;
                ObjectSupport.CopyData(data, this);
                if (data.Message.Length > 100)
                    Message = data.Message.Substring(0, 100) + this.__ResStr("more", "...more");
            }
        }

        public class BrowseModel {
            [UIHint("Grid")]
            public GridDefinition GridDef { get; set; }
        }

        [AllowGet]
        public ActionResult FeedbackBrowse() {
            BrowseModel model = new BrowseModel { };
            model.GridDef = new GridDefinition {
                AjaxUrl = GetActionUrl("FeedbackBrowse_GridData"),
                ModuleGuid = Module.ModuleGuid,
                RecordType = typeof(BrowseItem),
                SettingsModuleGuid = Module.PermanentGuid,
            };
            return View(model);
        }

        [AllowPost]
        [ConditionalAntiForgeryToken]
        public async Task<ActionResult> FeedbackBrowse_GridData(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, Guid settingsModuleGuid) {
            using (FeedbackDataProvider dataProvider = new FeedbackDataProvider()) {
                DataProviderGetRecords<FeedbackData> browseItems = await dataProvider.GetItemsAsync(skip, take, sort, filters);
                GridHelper.SaveSettings(skip, take, sort, filters, settingsModuleGuid);
                return GridPartialView(new DataSourceResult {
                    Data = (from s in browseItems.Data select new BrowseItem(Module, s)).ToList<object>(),
                    Total = browseItems.Total
                });
            }
        }
        [AllowPost]
        [Permission("RemoveFeedback")]
        [ExcludeDemoMode]
        public async Task<ActionResult> RemoveFeedback(int key) {
            using (FeedbackDataProvider dataProvider = new FeedbackDataProvider()) {
                await dataProvider.RemoveItemAsync(key);
                return Reload(null, Reload: ReloadEnum.ModuleParts);
            }
        }
    }
}