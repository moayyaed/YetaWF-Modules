﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using YetaWF.Core.Identity;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Pages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Views;
using YetaWF.Core.Views.Shared;

namespace YetaWF.Modules.Identity.Views.Shared {

    public class Users<TModel> : RazorTemplate<TModel> { }

    public static class UsersHelper {

        public class UsersModel {
            [UIHint("Grid")]
            public GridDefinition GridDef { get; set; }
        }

        public class GridAllowedUser {

            [Caption("Delete"), Description("Click to delete a user")]
            [UIHint("GridDeleteEntry")]
            public int DeleteMe { get; set; }

            [Caption("User"), Description("User Name")]
            [UIHint("YetaWF_Identity_UserId"), ReadOnly]
            public int UserNameFromId { get; set; }

            [UIHint("RawInt"), ReadOnly]
            public int UserId { get; set; }
            [UIHint("Raw"), ReadOnly]
            public string UserName { get; set; }

            public GridAllowedUser(int userId) {
                UserId = UserNameFromId = userId;
                UserName = Resource.ResourceAccess.GetUserName(userId);
            }
        }

        public static MvcHtmlString RenderResourceAllowedUsers<TModel>(this HtmlHelper<TModel> htmlHelper, string name, SerializableList<User> model) {
            List<GridAllowedUser> users;
            if (model == null)
                users = new List<GridAllowedUser>();
            else
                users = (from u in model select new GridAllowedUser(u.UserId)).ToList();

            bool header;
            if (!htmlHelper.TryGetControlInfo<bool>("", "Header", out header))
                header = true;
            DataSourceResult data = new DataSourceResult {
                Data = users.ToList<object>(),
                Total = users.Count,
            };
            UsersModel usersModel = new UsersModel() {
                GridDef = new GridDefinition() {
                    RecordType = typeof(GridAllowedUser),
                    Data = data,
                    SupportReload = false,
                    PageSizes = new List<int>(),
                    InitialPageSize = 5,
                    ShowHeader = header,
                    ReadOnly = false,
                    CanAddOrDelete = true,
                    DeleteProperty = "UserId",
                    DisplayProperty = "UserName"
                }
            };
            return htmlHelper.DisplayFor(m => usersModel.GridDef);
        }

        public class GridAllowedUserDisplay {

            [Caption("User"), Description("User Name")]
            [UIHint("YetaWF_Identity_UserId"), ReadOnly]
            public int UserId { get; set; }

            public GridAllowedUserDisplay(int userId) {
                UserId = userId;
            }
        }

        public static MvcHtmlString RenderResourceAllowedUsersDisplay<TModel>(this HtmlHelper<TModel> htmlHelper, string name, SerializableList<User> model) {
            List<GridAllowedUserDisplay> users;
            if (model == null)
                users = new List<GridAllowedUserDisplay>();
            else
                users = (from u in model select new GridAllowedUserDisplay(u.UserId)).ToList();

            bool header;
            if (!htmlHelper.TryGetControlInfo<bool>("", "Header", out header))
                header = true;
            DataSourceResult data = new DataSourceResult {
                Data = users.ToList<object>(),
                Total = users.Count,
            };
            UsersModel usersModel = new UsersModel() {
                GridDef = new GridDefinition() {
                    RecordType = typeof(GridAllowedUserDisplay),
                    Data = data,
                    SupportReload = false,
                    PageSizes = new List<int>(),
                    InitialPageSize = 5,
                    ShowHeader = header,
                    ReadOnly = true,
                }
            };
            return htmlHelper.DisplayFor(m => usersModel.GridDef);
        }

    }
}