﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/DevTests#License */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.DevTests.Controllers;

namespace YetaWF.Modules.DevTests.Components {

    public abstract class ListOfEmailAddressesComponentBase : YetaWFComponent {

        protected static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(ListOfEmailAddressesComponentBase), name, defaultValue, parms); }

        public const string TemplateName = "ListOfEmailAddresses";

        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        public override string GetTemplateName() { return TemplateName; }
    }

    public class ListOfEmailAddressesDisplayComponent : ListOfEmailAddressesComponentBase, IYetaWFComponent<List<string>> {

        public override ComponentType GetComponentType() { return ComponentType.Display; }

        public class Entry {
            [Caption("Email Address"), Description("Shows all defined email addresses")]
            [UIHint("String"), ReadOnly]
            public string EmailAddress { get; set; }

            public Entry(string text) {
                EmailAddress = text;
            }
        }
        internal static Grid2Definition GetGridModel(bool header) {
            return new Grid2Definition() {
                RecordType = typeof(Entry),
                InitialPageSize = 5,
                PageSizes = new List<int> { 5, 10, 20 },
                ShowHeader = header,
                AjaxUrl = YetaWFManager.UrlFor(typeof(ListOfEmailAddressesController), nameof(ListOfEmailAddressesController.ListOfEmailAddressesDisplay_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
            };
        }
        public async Task<YHtmlString> RenderAsync(List<string> model) {

            HtmlBuilder hb = new HtmlBuilder();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);

            Grid2Model grid = new Grid2Model() {
                GridDef = GetGridModel(header)
            };
            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<Entry> list = new List<Entry>();
                if (model != null)
                    list = (from u in model select new Entry(u)).ToList();
                return Task.FromResult(new DataSourceResult {
                    Data = list.ToList<object>(),
                    Total = list.Count
                });
            };

            hb.Append($@"
<div class='yt_listofemailaddresses t_display'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Softelvdm_Grid_Grid2", HtmlAttributes: HtmlAttributes)}
</div>");

            return hb.ToYHtmlString();
        }
    }
    public class ListOfEmailAddressesEditComponent : ListOfEmailAddressesComponentBase, IYetaWFComponent<List<string>> {

        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        public class ListOfEmailAddressesSetup {
            public string GridId { get; set; }
            public string AddUrl { get; set; }
        }

        public class NewModel {
            [Caption("Email Address"), Description("Please enter a new email address and click Add")]
            [UIHint("Text80"), StringLength(80), Trim]
            public string NewValue { get; set; }
        }

        public class Entry {

            public Entry() { }

            [Caption("Delete"), Description("Click to remove this email address from the list")]
            [UIHint("Softelvdm_Grid_Grid2DeleteEntry"), ReadOnly]
            public int Delete { get; set; }

            [Caption("Email Address"), Description("Shows all defined email addresses")]
            [UIHint("String"), ReadOnly]
            public string EmailAddress { get; set; }

            [UIHint("Softelvdm_Grid_Grid2Value"), ReadOnly]
            public string Value { get { return EmailAddress; } }

            public Entry(string text) {
                EmailAddress = text;
            }
        }
        internal static Grid2Definition GetGridModel(bool header) {
            return new Grid2Definition() {
                RecordType = typeof(Entry),
                InitialPageSize = 5,
                PageSizes = new List<int> { 5, 10, 20 },
                ShowHeader = header,
                AjaxUrl = YetaWFManager.UrlFor(typeof(ListOfEmailAddressesController), nameof(ListOfEmailAddressesController.ListOfEmailAddressesEdit_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
                DeletedMessage = __ResStr("removeMsg", "Email address {0} has been removed"),
                DeleteConfirmationMessage = __ResStr("confimMsg", "Are you sure you want to remove email address {0}?"),
                DeletedColumnDisplay = nameof(Entry.EmailAddress),
            };
        }

        public async Task<YHtmlString> RenderAsync(List<string> model) {

            HtmlBuilder hb = new HtmlBuilder();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);

            Grid2Model grid = new Grid2Model() {
                GridDef = GetGridModel(header)
            };

            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<Entry> list = new List<Entry>();
                if (model != null)
                    list = (from u in model select new Entry(u)).ToList();
                return Task.FromResult(new DataSourceResult {
                    Data = list.ToList<object>(),
                    Total = list.Count
                });
            };

            hb.Append($@"
<div class='yt_listofemailaddresses t_edit' id='{DivId}'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Softelvdm_Grid_Grid2", HtmlAttributes: HtmlAttributes)}");

            using (Manager.StartNestedComponent(FieldName)) {

                NewModel newModel = new NewModel();
                hb.Append($@"
    <div class='t_newvalue'>
        {await HtmlHelper.ForLabelAsync(newModel, nameof(newModel.NewValue))}
        {await HtmlHelper.ForEditAsync(newModel, nameof(newModel.NewValue))}
        <input name='btnAdd' type='button' value='Add' disabled='disabled' />
    </div>");

            }

            ListOfEmailAddressesSetup setup = new ListOfEmailAddressesSetup {
                AddUrl = GetSiblingProperty<string>($"{PropertyName}_AjaxUrl"),
                GridId = grid.GridDef.Id,
            };

            hb.Append($@"
</div>
<script>
    new YetaWF_DevTests.ListOfEmailAddressesEditComponent('{DivId}', {YetaWFManager.JsonSerialize(setup)});
</script>");

            return hb.ToYHtmlString();
        }
        public static async Task<Grid2RecordData> Grid2RecordAsync(string fieldPrefix, object model) {
            // handle async properties
            await YetaWFController.HandlePropertiesAsync(model);
            Grid2RecordData record = new Grid2RecordData() {
                GridDef = GetGridModel(false),
                Data = model,
                FieldPrefix = fieldPrefix,
            };
            return record;
        }
    }
}
