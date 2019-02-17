﻿/* Copyright © 2019 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/IVR#License */

using Softelvdm.Modules.IVR.Components;
using Softelvdm.Modules.TwilioProcessor.Models.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Support;
#if MVC6
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace Softelvdm.Modules.IVR.Controllers {

    public class ListOfPhoneNumbersController : YetaWFController {

        [AllowPost]
        [ConditionalAntiForgeryToken]
        public async Task<ActionResult> ListOfPhoneNumbersDisplay_SortFilter(string data, string fieldPrefix, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) {
            return await GridPartialViewAsync<ListOfPhoneNumbersDisplayComponent.Entry>(ListOfPhoneNumbersDisplayComponent.GetGridModel(false, false, false), data, fieldPrefix, skip, take, sorts, filters);
        }
        [AllowPost]
        [ConditionalAntiForgeryToken]
        public async Task<ActionResult> ListOfPhoneNumbersEdit_SortFilter(string data, string fieldPrefix, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) {
            return await GridPartialViewAsync<ListOfPhoneNumbersEditComponent.Entry>(ListOfPhoneNumbersEditComponent.GetGridModel(false), data, fieldPrefix, skip, take, sorts, filters);
        }
        [AllowPost]
        [ConditionalAntiForgeryToken]
        [ExcludeDemoMode]
        public async Task<ActionResult> AddPhoneNumber(string data, string fieldPrefix, string newPhoneNumber, bool sms) {
            List<ListOfPhoneNumbersEditComponent.Entry> list = YetaWFManager.JsonDeserialize<List<ListOfPhoneNumbersEditComponent.Entry>>(data);
            string phoneNumber = PhoneNumberUSAttribute.GetE164(newPhoneNumber);
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Error(this.__ResStr("invPhone", "Phone number {0} is not a valid phone number", newPhoneNumber));
            if ((from l in list where l.PhoneNumber == phoneNumber select l).FirstOrDefault() != null)
                throw new Error(this.__ResStr("dupPhone", "Phone number {0} has already been added", newPhoneNumber));
            ListOfPhoneNumbersEditComponent.Entry entry = new ListOfPhoneNumbersEditComponent.Entry(phoneNumber, sms);
            return await GridRecordViewAsync(await ListOfPhoneNumbersEditComponent.GridRecordAsync(fieldPrefix, entry));
        }
    }
}