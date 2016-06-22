﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/TinyLogin#License */

using System;
using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.Core.Views.Shared;
using YetaWF.DataProvider;
using YetaWF.Modules.TinyLogin.Support;

namespace YetaWF.Modules.TinyLogin.Modules {

    public class TinyLoginModuleDataProvider : ModuleDefinitionDataProvider<Guid, TinyLoginModule>, IInstallableModel { }

    [ModuleGuid("{9e929bdc-8810-4710-ab3d-b7bced570e02}")] // Published Guid
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    public class TinyLoginModule : ModuleDefinition {

        public const int MaxTooltip = 100;

        public TinyLoginModule()
            : base() {
            Title = this.__ResStr("modTitle", "Tiny Login");
            Name = this.__ResStr("modName", "Tiny Login");
            Description = this.__ResStr("modSummary", "Provides Login/Register links and displays a logged on user's account name");
            AllowUserRegistration = true;
            UserTooltip = new MultiString();
            ShowTitle = false;
            WantSearch = false;
            WantFocus = false;
            Print = false;
        }

        public override IModuleDefinitionIO GetDataProvider() { return new TinyLoginModuleDataProvider(); }
        public override bool ShowActionMenu { get { return false; } }

        [Category("General"), Caption("Allow New Users"), Description("Allow registration of new users")]
        [UIHint("Boolean")]
        public bool AllowUserRegistration { get; set; }

        [Category("General")]
        [Caption("Log On Url"), Description("The Url where the user is redirected when the login link is clicked")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
        [StringLength(Globals.MaxUrl), Trim]
        public string LogonUrl { get; set; }

        [Category("General")]
        [Caption("Log Off Url"), Description("The Url where the user is redirected when the logoff link is clicked")]
        [UIHint("Text80"), LogoffRegularExpressionAttribute]
        [StringLength(Globals.MaxUrl), Trim]
        public string LogoffUrl { get; set; }

        [Category("General")]
        [Caption("Register Url"), Description("The Url where the user is redirected when the register link is clicked")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
        [StringLength(Globals.MaxUrl), Trim]
        public string RegisterUrl { get; set; }

        [Category("General")]
        [Caption("User Url"), Description("The Url where the user is redirected when the user name is clicked")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
        [StringLength(Globals.MaxUrl), Trim]
        public string UserUrl { get; set; }

        [Category("General")]
        [Caption("User Tooltip"), Description("The tooltip shown for the user name link")]
        [UIHint("MultiString80"), StringLength(MaxTooltip), Trim]
        public MultiString UserTooltip { get; set; }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }

        public ModuleAction GetAction_Login(string url) {
            return new ModuleAction(this) {
                Url = url,
                LinkText = this.__ResStr("loginLink", "Login"),
                MenuText = this.__ResStr("loginText", "Login"),
                Image = "Login.png",
                Tooltip = this.__ResStr("loginTooltip", "Click to log into this site using your existing account"),
                Legend = this.__ResStr("loginLegend", "Logs into this site using your existing account"),
                Style = ModuleAction.ActionStyleEnum.Popup,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                SaveReturnUrl = true,
            };
        }
        public ModuleAction GetAction_Register(string url) {
            if (!AllowUserRegistration) return null;
            return new ModuleAction(this) {
                Url = url,
                LinkText = this.__ResStr("registerLink", "Register"),
                MenuText = this.__ResStr("registerText", "Register"),
                Image = "Register.png",
                Tooltip = this.__ResStr("registerTooltip", "Click to register a new account for access to this site"),
                Legend = this.__ResStr("registerLegend", "register to access this site with a new account"),
                Style = ModuleAction.ActionStyleEnum.Popup,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                SaveReturnUrl = true,
            };
        }
        public ModuleAction GetAction_Logoff(string url) {
            return new ModuleAction(this) {
                Url = url,
                LinkText = this.__ResStr("logoffLink", "Logout"),
                MenuText = this.__ResStr("logoffText", "Logout"),
                Image = "Logoff.png",
                Tooltip = this.__ResStr("logoffTooltip", "Click to log off from this site"),
                Legend = this.__ResStr("logoffLegend", "Logs you out from this site"),
                Style = ModuleAction.ActionStyleEnum.Normal,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
            };
        }
        public ModuleAction GetAction_UserName(string url, string userName, string tooltip) {
            return new ModuleAction(this) {
                Url = url,
                LinkText = userName,
                MenuText = userName,
                Image = "UserName.png",
                Tooltip = tooltip,
                Style = ModuleAction.ActionStyleEnum.Normal,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                SaveReturnUrl = true,
            };
        }
    }
}