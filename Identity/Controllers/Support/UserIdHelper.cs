﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Identity;
using YetaWF.Core.Models.Attributes;
using YetaWF.Modules.Identity.Addons;
using YetaWF.Modules.Identity.DataProvider;
using YetaWF.Core.Components;
using static YetaWF.Modules.Identity.Components.UserIdEditComponent;
#if MVC6
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Identity.Controllers {

    public class UserIdController : YetaWFController {

        [AllowPost]
        [ConditionalAntiForgeryToken]
        [ResourceAuthorize(Info.Resource_AllowUserIdAjax)]
        public async Task<ActionResult> UsersBrowse_GridData(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters /*, Guid settingsModuleGuid - not available in templates */) {
            using (UserDefinitionDataProvider dataProvider = new UserDefinitionDataProvider()) {
                DataProviderGetRecords<UserDefinition> browseItems = await dataProvider.GetItemsAsync(skip, take, sort, filters);
                //Grid.SaveSettings(skip, take, sort, filters, settingsModuleGuid);
                return await GridPartialViewAsync(new DataSourceResult {
                    Data = (from s in browseItems.Data select new GridAllEntry(s)).ToList<object>(),
                    Total = browseItems.Total
                });
            }
        }
    }
}
