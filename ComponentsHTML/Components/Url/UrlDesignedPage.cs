﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Localize;
using YetaWF.Core.Packages;
using YetaWF.Core.Pages;
using YetaWF.Core.Support;

namespace YetaWF.Modules.ComponentsHTML.Components {

    public abstract class UrlDesignedPageComponentBase : YetaWFComponent {

        protected static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(UrlDesignedPageComponentBase), name, defaultValue, parms); }

        public const string TemplateName = "UrlDesignedPage";

        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        public override string GetTemplateName() { return TemplateName; }
    }
    public class UrlDesignedPageEditComponent : UrlDesignedPageComponentBase, IYetaWFComponent<string> {

        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        public async Task<YHtmlString> RenderAsync(string model) {

            List<string> pages = await PageDefinition.GetDesignedUrlsAsync();

            // get list of desired pages (ignore users that are invalid, they may have been deleted)
            List<SelectionItem<string>> list = new List<SelectionItem<string>>();
            foreach (var page in pages) {
                list.Add(new SelectionItem<string> {
                    Text = page,
                    //Tooltip = __ResStr("selPage", "Select page {0}", page),
                    Value = page,
                });
            }
            list = (from l in list orderby l.Text select l).ToList();
            list.Insert(0, new SelectionItem<string> { Text = __ResStr("select", "(select)"), Value = "" });

            return await DropDownListComponent.RenderDropDownListAsync(this, model, list, "yt_urldesignedpage");
        }
    }
}