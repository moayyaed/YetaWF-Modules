﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/CurrencyConverter#License */

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using YetaWF.Core.Pages;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.CurrencyConverter.DataProvider;

namespace YetaWF.Modules.CurrencyConverter.Views.Shared {

    public class Vendor<TModel> : RazorTemplate<TModel> { }

    public static class CountryHelper {

        public static MvcHtmlString RenderCountry<TModel>(this HtmlHelper<TModel> htmlHelper, string name, string model, object HtmlAttributes = null) {

            using (ExchangeRateDataProvider dp = new ExchangeRateDataProvider()) {
                ExchangeRateData data = dp.GetItem();
                List<SelectionItem<string>> list = (from r in data.Rates orderby r.CurrencyName select new SelectionItem<string> { Text = r.CurrencyName, Value = r.Code }).ToList();
                return htmlHelper.RenderDropDownSelectionList(name, model, list);
            }
        }
    }
}