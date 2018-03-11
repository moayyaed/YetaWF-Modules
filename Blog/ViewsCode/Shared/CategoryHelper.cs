﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Blog#License */

using System.Collections.Generic;
using System.Linq;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Views;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Blog.DataProvider;
using System.Threading.Tasks;
#if MVC6
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#else
using System.Web;
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Blog.Views.Shared {

    public static class CategoryHelper {

        private static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(CategoryHelper), name, defaultValue, parms); }
#if MVC6
        public static async Task<HtmlString> RenderCategoryAsync<TModel>(this IHtmlHelper<TModel> htmlHelper, string name, int model, object HtmlAttributes = null)
#else
        public static async Task<HtmlString> RenderCategoryAsync<TModel>(this HtmlHelper<TModel> htmlHelper, string name, int model, object HtmlAttributes = null)
#endif
        {
            using (BlogCategoryDataProvider categoryDP = new BlogCategoryDataProvider()) {
                DataProviderGetRecords<BlogCategory> data = await categoryDP.GetItemsAsync(0, 0, null, null);
                List<SelectionItem<int>> list = (from c in data.Data orderby c.Category.ToString() select new SelectionItem<int> {
                    Text = c.Category.ToString(),
                    Tooltip = c.Description,
                    Value = c.Identity
                }).ToList();

                if (list.Count == 0) {
                    list.Insert(0, new SelectionItem<int> {
                        Text = __ResStr("none", "(None Available)"),
                        Tooltip = __ResStr("noneTT", "There are no blog categories"),
                        Value = 0,
                    });
                } else {
                    bool showSelect = false;
                    bool showAll = htmlHelper.GetControlInfo<bool>("", "ShowAll", false);
                    if (model == 0 && !showAll)
                        showSelect = htmlHelper.GetControlInfo<bool>("", "ShowSelectIfNone", false);
                    if (!showAll && !showSelect)
                        showSelect = htmlHelper.GetControlInfo<bool>("", "ShowSelect", false);
                    if (showAll) {
                        list.Insert(0, new SelectionItem<int> {
                            Text = __ResStr("all", "(All)"),
                            Tooltip = __ResStr("allTT", "Displays blogs from all available blog categories"),
                            Value = 0,
                        });
                    } else if (showSelect) {
                        list.Insert(0, new SelectionItem<int> {
                            Text = __ResStr("select", "(Select)"),
                            Tooltip = __ResStr("selectTT", "Please select one of the available blog categories"),
                            Value = 0,
                        });
                    }
                }
                return await htmlHelper.RenderDropDownSelectionListAsync(name, model, list, HtmlAttributes: HtmlAttributes);
            }
        }
    }
}