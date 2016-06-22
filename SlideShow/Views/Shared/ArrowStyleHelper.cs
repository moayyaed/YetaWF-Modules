﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/SlideShow#License */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using YetaWF.Core.Addons;
using YetaWF.Core.Localize;
using YetaWF.Core.Packages;
using YetaWF.Core.Pages;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;

namespace YetaWF.Modules.SlideShow.Views.Shared {

    public class ArrowStyle<TModel> : RazorTemplate<TModel> { }

    public static class ArrowStyleHelper {

        private static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(ArrowStyleHelper), name, defaultValue, parms); }

        public static MvcHtmlString RenderArrowStyle(this HtmlHelper htmlHelper, string name, string selection, object HtmlAttributes = null) {

            List<SelectionItem<string>> list = (from a in Arrows select new SelectionItem<string>() {
                Text = a.Name,
                Value = a.Value,
            }).ToList();
            return htmlHelper.RenderDropDownSelectionList(name, selection, list, HtmlAttributes: HtmlAttributes);
        }
        public class Arrow {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public static List<Arrow> Arrows {
            get {
                List<Arrow> list = new List<Arrow>();
                Package package = YetaWF.Modules.SlideShow.Controllers.AreaRegistration.CurrentPackage;
                string rootUrl = VersionManager.GetAddOnModuleUrl(package.Domain, package.Product);
                string[] files = Directory.GetFiles(YetaWFManager.UrlToPhysical(rootUrl + "jssor/skins/arrow"), "*.css");
                foreach (var file in files) {
                    string name = Path.GetFileNameWithoutExtension(file);
                    list.Add(new Arrow {
                        Name = string.Format("Arrow {0}", name.Substring(1)),
                        Value = name,
                    });
                }
                return list;
            }
        }
    }
}