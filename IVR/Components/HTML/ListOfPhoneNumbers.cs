﻿/* Copyright © 2019 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/IVR#License */

using Softelvdm.Modules.IVR.Controllers;
using Softelvdm.Modules.IVR.DataProvider;
using Softelvdm.Modules.TwilioProcessor.Models.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core;
using YetaWF.Core.Components;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;

namespace Softelvdm.Modules.IVR.Components {

    public abstract class ListOfPhoneNumbersComponentBase : YetaWFComponent {

        protected static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(ListOfPhoneNumbersComponentBase), name, defaultValue, parms); }

        public const string TemplateName = "ListOfPhoneNumbers";

        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        public override string GetTemplateName() { return TemplateName; }
    }

    public class ListOfPhoneNumbersDisplayComponent : ListOfPhoneNumbersComponentBase, IYetaWFComponent<SerializableList<ExtensionPhoneNumber>> {

        public override ComponentType GetComponentType() { return ComponentType.Display; }

        public class Entry {

            [Caption("Phone Number"), Description("Shows all defined phone numbers")]
            [UIHint("Softelvdm_IVR_PhoneNumber"), ReadOnly]
            public string PhoneNumber { get; set; }

            [Caption("SMS"), Description("Shows whether a text message is sent to the phone number when a voice mail is received")]
            [UIHint("Boolean"), ReadOnly]
            public bool SendSMS { get; set; }

            public Entry() { }
        }
        internal static GridDefinition GetGridModel(bool header, bool pager, bool useSkin) {
            return new GridDefinition() {
                SizeStyle = GridDefinition.SizeStyleEnum.SizeToFit,
                HighlightOnClick = false,
                RecordType = typeof(Entry),
                InitialPageSize = 10,
                ShowHeader = header,
                ShowPager = pager,
                UseSkinFormatting = useSkin,
                AjaxUrl = YetaWFManager.UrlFor(typeof(ListOfPhoneNumbersController), nameof(ListOfPhoneNumbersController.ListOfPhoneNumbersDisplay_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
            };
        }

        public async Task<YHtmlString> RenderAsync(SerializableList<ExtensionPhoneNumber> model) {

            HtmlBuilder hb = new HtmlBuilder();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);
            bool pager = PropData.GetAdditionalAttributeValue("Pager", true);
            bool useSkin = PropData.GetAdditionalAttributeValue("UseSkinFormatting", true);

            GridModel grid = new GridModel() {
                GridDef = GetGridModel(header, pager, useSkin)
            };
            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<Entry> list = new List<Entry>();
                if (model != null) {
                    foreach (ExtensionPhoneNumber ext in model) {
                        Entry entry = new Entry();
                        ObjectSupport.CopyData(ext, entry);
                        list.Add(entry);
                    }
                }
                return Task.FromResult(new DataSourceResult {
                    Data = list.ToList<object>(),
                    Total = list.Count
                });
            };

            hb.Append($@"
<div class='yt_listofphonenumbers t_display'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Grid", HtmlAttributes: HtmlAttributes)}
</div>");

            return hb.ToYHtmlString();
        }
    }

    public class ListOfPhoneNumbersEditComponent : ListOfPhoneNumbersComponentBase, IYetaWFComponent<SerializableList<ExtensionPhoneNumber>> {

        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        public class ListOfPhoneNumbersSetup {
            public string GridId { get; set; }
            public string AddUrl { get; set; }
        }
        public class NewModel {

            [Caption("Phone Number"), Description("Please enter a new phone number and click Add")]
            [UIHint("Text20"), StringLength(Globals.MaxPhoneNumber), PhoneNumberUS]
            public string NewPhoneNumber { get; set; }

            [Caption("SMS"), Description("Shows whether a text message is sent to the phone number when a voice mail is received")]
            [UIHint("Boolean")]
            public bool SendSMS { get; set; }
        }

        public class Entry {

            [Caption("Delete"), Description("Click to remove this phone number from the list")]
            [UIHint("GridDeleteEntry"), ReadOnly]
            public int Delete { get; set; }

            [Caption("Phone Number"), Description("Shows all defined phone numbers")]
            [UIHint("Softelvdm_IVR_PhoneNumber"), ReadOnly]
            public string PhoneNumberDisplay { get; set; }

            [Caption("SMS"), Description("Shows whether a text message is sent to the phone number when a voice mail is received")]
            [UIHint("Boolean")]
            public bool SendSMS { get; set; }

            [UIHint("Hidden"), ReadOnly]
            public string PhoneNumber { get; set; }

            public Entry(string phoneNumber, bool sms) {
                PhoneNumberDisplay = PhoneNumber = phoneNumber;
                SendSMS = sms;
            }
        }

        internal static GridDefinition GetGridModel(bool header) {
            return new GridDefinition() {
                SizeStyle = GridDefinition.SizeStyleEnum.SizeToFit,
                RecordType = typeof(Entry),
                InitialPageSize = 10,
                ShowHeader = header,
                AjaxUrl = YetaWFManager.UrlFor(typeof(ListOfPhoneNumbersController), nameof(ListOfPhoneNumbersController.ListOfPhoneNumbersEdit_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
                DeletedMessage = __ResStr("removeMsg", "Phone number {0} has been removed"),
                DeleteConfirmationMessage = __ResStr("confimMsg", "Are you sure you want to remove phone number {0}?"),
                DeletedColumnDisplay = nameof(Entry.PhoneNumber),
            };
        }

        public async Task<YHtmlString> RenderAsync(SerializableList<ExtensionPhoneNumber> model) {

            HtmlBuilder hb = new HtmlBuilder();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);

            GridModel grid = new GridModel() {
                GridDef = GetGridModel(header)
            };
            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<Entry> list = new List<Entry>();
                if (model != null) {
                    foreach (ExtensionPhoneNumber ext in model) {
                        Entry entry = new Entry(ext.PhoneNumber, ext.SendSMS);
                        list.Add(entry);
                    }
                }
                return Task.FromResult(new DataSourceResult {
                    Data = list.ToList<object>(),
                    Total = list.Count
                });
            };

            hb.Append($@"
<div class='yt_listofphonenumbers t_edit' id='{DivId}'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Grid", HtmlAttributes: HtmlAttributes)}");

            using (Manager.StartNestedComponent(FieldName)) {

                NewModel newModel = new NewModel();
                hb.Append($@"
    <div class='t_newvalue'>
        {await HtmlHelper.ForLabelAsync(newModel, nameof(newModel.NewPhoneNumber))}
        {await HtmlHelper.ForEditAsync(newModel, nameof(newModel.NewPhoneNumber), Validation:false)}
        <input name='btnAdd' type='button' value='Add' disabled='disabled' />
    </div>");

            }

            ListOfPhoneNumbersSetup setup = new ListOfPhoneNumbersSetup {
                AddUrl = YetaWFManager.UrlFor(typeof(ListOfPhoneNumbersController), nameof(ListOfPhoneNumbersController.AddPhoneNumber)),
                GridId = grid.GridDef.Id,
            };

            hb.Append($@"
</div>
<script>
    new Softelvdm_IVR.ListOfPhoneNumbersEditComponent('{DivId}', {YetaWFManager.JsonSerialize(setup)});
</script>");

            return hb.ToYHtmlString();
        }
        public static async Task<GridRecordData> GridRecordAsync(string fieldPrefix, object model) {
            // handle async properties
            await YetaWFController.HandlePropertiesAsync(model);
            GridRecordData record = new GridRecordData() {
                GridDef = GetGridModel(false),
                Data = model,
                FieldPrefix = fieldPrefix,
            };
            return record;
        }
    }
}