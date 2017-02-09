/* Copyright � 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Blog#License */

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Modules.Blog.DataProvider;
using YetaWF.Modules.Blog.Modules;

namespace YetaWF.Modules.Blog.Controllers {

    public class ArchiveModuleController : ControllerImpl<YetaWF.Modules.Blog.Modules.ArchiveModule> {

        public ArchiveModuleController() { }

        public class DisplayModel {

            [Caption("Archive"), Description("Monthly blog entries")]
            [UIHint("ModuleActions"), AdditionalMetadata("RenderAs",ModuleAction.RenderModeEnum.NormalLinks), ReadOnly]
            public List<ModuleAction> Actions { get; set; }

            public DisplayModel() {
                Actions = new List<ModuleAction>();
            }

        }

        [HttpGet]
        public ActionResult Archive(int? blogCategory) {
            int category = (int) (blogCategory ?? 0);

            BlogModule blogMod = new BlogModule();
            DisplayModel model = new DisplayModel() {};

            using (BlogEntryDataProvider entryDP = new BlogEntryDataProvider()) {
                List<DataProviderSortInfo> sort = new List<DataProviderSortInfo> {
                    new DataProviderSortInfo { Field = "DatePublished", Order = DataProviderSortInfo.SortDirection.Descending },
                };
                List<DataProviderFilterInfo> filters = new List<DataProviderFilterInfo>{
                    new DataProviderFilterInfo { Field = "Published", Operator = "==", Value = true },
                };
                if (category != 0)
                    filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "CategoryIdentity", Operator = "==", Value = category });

                int year = 0, month = 0, count = 0;

                int totalRecs = 0, start = 0, incr = 100;
                for ( ; ; ) {

                    List<BlogEntry> data = entryDP.GetItems(start, incr, sort, filters, out totalRecs);
                    if (data.Count == 0)
                        return new EmptyResult();

                    foreach (BlogEntry entry in data) {
                        if (entry.DatePublished.Month != month || entry.DatePublished.Year != year) {
                            if (count > 0) {
                                DateTime d = new DateTime(year, month, 1);
                                model.Actions.New(blogMod.GetAction_Blog(null, category, StartDate: d, Count: count));
                                count = 0;
                            }
                            month = entry.DatePublished.Month;
                            year = entry.DatePublished.Year;
                        }
                        ++count;
                    }
                    start += incr;
                    if (count > 0) {
                        DateTime d = new DateTime(year, month, 1);
                        model.Actions.New(blogMod.GetAction_Blog(null, category, StartDate: d, Count: count));
                    }
                    break;
                }
                return View(model);
            }
        }
    }
}