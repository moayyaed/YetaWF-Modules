﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Identity;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Modules.Identity.Controllers;

namespace YetaWF.Modules.ComponentsHTML.Components {

    public abstract class UserRolesComponentBase : YetaWFComponent {

        public const string TemplateName = "UserRoles";

        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        public override string GetTemplateName() { return TemplateName; }

    }
    public class UserRolesDisplayComponent : UserRolesComponentBase, IYetaWFComponent<SerializableList<YetaWF.Core.Identity.Role>> {

        public override ComponentType GetComponentType() { return ComponentType.Display; }

        public class Entry {

            [Caption("Apply"), Description("The role applies to the user if selected")]
            [UIHint("Boolean")]
            public bool InRole { get; set; }

            [Caption("Name"), Description("Role Name")]
            [UIHint("String"), ReadOnly]
            public string Name { get; set; }

            [Caption("Description"), Description("Role Description")]
            [UIHint("String"), ReadOnly]
            public string Description { get; set; }

            [Caption("Role Id"), Description("The id used internally to identify the role")]
            [UIHint("IntValue"), ReadOnly]
            public int DisplayRoleId { get { return RoleId; } }

            [UIHint("Hidden")]
            public int RoleId { get; set; }
        }
        internal static Grid2Definition GetGridModel(bool header) {
            return new Grid2Definition() {
                RecordType = typeof(Entry),
                InitialPageSize = 10,
                ShowHeader = header,
                AjaxUrl = YetaWFManager.UrlFor(typeof(UserRolesController), nameof(UserRolesController.UserRolesDisplay_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
            };
        }

        public async Task<YHtmlString> RenderAsync(SerializableList<YetaWF.Core.Identity.Role> model) {

            HtmlBuilder hb = new HtmlBuilder();

            if (model == null)
                model = new SerializableList<YetaWF.Core.Identity.Role>();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);

            Grid2Model grid = new Grid2Model() {
                GridDef = GetGridModel(header)
            };
            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<RoleInfo> allRoles = Resource.ResourceAccess.GetDefaultRoleList();
                int superuserRole = Resource.ResourceAccess.GetSuperuserRoleId();
                int userRole = Resource.ResourceAccess.GetUserRoleId();
                List<Entry> roles = (from r in allRoles orderby r.Name
                                     select new Entry {
                                         RoleId = r.RoleId,
                                         Name = r.Name,
                                         Description = r.Description,
                                         InRole = userRole == r.RoleId || (model != null && model.Contains(new Role { RoleId = r.RoleId }, new RoleComparer())),
                                     }).ToList<Entry>();
                DataSourceResult data = new DataSourceResult {
                    Data = roles.ToList<object>(),
                    Total = roles.Count,
                };
                return Task.FromResult(data);
            };

            hb.Append($@"
<div class='yt_yetawf_identity_userroles t_display'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Softelvdm_Grid_Grid2", HtmlAttributes: HtmlAttributes)}
</div>");

            return hb.ToYHtmlString();
        }
    }

    public class UserRolesEditComponent : UserRolesComponentBase, IYetaWFComponent<SerializableList<YetaWF.Core.Identity.Role>> {

        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        public class Entry {

            [Caption("Apply"), Description("Select to apply the role to the user")]
            [UIHint("Boolean")]
            public bool InRole { get; set; }

            [Caption("Name"), Description("Role Name")]
            [UIHint("String"), ReadOnly]
            public string Name { get; set; }

            [Caption("Description"), Description("Role Description")]
            [UIHint("String"), ReadOnly]
            public string Description { get; set; }

            [Caption("Role Id"), Description("The id used internally to identify the role")]
            [UIHint("IntValue"), ReadOnly]
            public int DisplayRoleId { get { return RoleId; } }

            [UIHint("Hidden")]
            public int RoleId { get; set; }

            public bool __editable { get; set; }
        }
        internal static Grid2Definition GetGridModel(bool header) {
            return new Grid2Definition() {
                RecordType = typeof(Entry),
                InitialPageSize = 10,
                ShowHeader = header,
                AjaxUrl = YetaWFManager.UrlFor(typeof(UserRolesController), nameof(UserRolesController.UserRolesEdit_SortFilter)),
                SortFilterStaticData = (List<object> data, int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                    DataProviderGetRecords<Entry> recs = DataProviderImpl<Entry>.GetRecords(data, skip, take, sorts, filters);
                    return new DataSourceResult {
                        Data = recs.Data.ToList<object>(),
                        Total = recs.Total,
                    };
                },
            };
        }

        public async Task<YHtmlString> RenderAsync(SerializableList<YetaWF.Core.Identity.Role> model) {

            HtmlBuilder hb = new HtmlBuilder();

            bool header = PropData.GetAdditionalAttributeValue("Header", true);

            Grid2Model grid = new Grid2Model() {
                GridDef = GetGridModel(header)
            };
            grid.GridDef.DirectDataAsync = (int skip, int take, List<DataProviderSortInfo> sorts, List<DataProviderFilterInfo> filters) => {
                List<RoleInfo> allRoles = Resource.ResourceAccess.GetDefaultRoleList();
                int superuserRole = Resource.ResourceAccess.GetSuperuserRoleId();
                int userRole = Resource.ResourceAccess.GetUserRoleId();
                List<Entry> roles = (from r in allRoles orderby r.Name
                                     select new Entry {
                                         RoleId = r.RoleId,
                                         Name = r.Name,
                                         Description = r.Description,
                                         InRole = userRole == r.RoleId || (model != null && model.Contains(new Role { RoleId = r.RoleId }, new RoleComparer())),
                                         __editable = (userRole != r.RoleId) // we disable the user entry
                                     }).ToList<Entry>();


                DataSourceResult data = new DataSourceResult {
                    Data = roles.ToList<object>(),
                    Total = roles.Count,
                };
                return Task.FromResult(data);
            };

            hb.Append($@"
<div class='yt_yetawf_identity_userroles t_edit' id='{DivId}'>
    {await HtmlHelper.ForDisplayAsAsync(Container, PropertyName, FieldName, grid, nameof(grid.GridDef), grid.GridDef, "Softelvdm_Grid_Grid2", HtmlAttributes: HtmlAttributes)}
</div>");

            hb.Append($"<div class='yt_yetawf_identity_userroles t_edit'>");

            return hb.ToYHtmlString();
        }
    }
}
