﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Menus;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;
using YetaWF.Modules.Identity.DataProvider;

namespace YetaWF.Modules.Identity.Modules {

    public class ForgotPasswordModuleDataProvider : ModuleDefinitionDataProvider<Guid, ForgotPasswordModule>, IInstallableModel { }

    [ModuleGuid("{3437ee4d-747f-4bf1-aa3c-d0417751b24b}")]
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    public class ForgotPasswordModule : ModuleDefinition {

        public ForgotPasswordModule() {
            Title = this.__ResStr("modTitle", "Forgot Password?");
            Name = this.__ResStr("modName", "Forgot Password?");
            Description = this.__ResStr("modSummary", "Sends an email for a forgotten password");
        }

        public override IModuleDefinitionIO GetDataProvider() { return new ForgotPasswordModuleDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }

        public override MenuList GetModuleMenuList(ModuleAction.RenderModeEnum renderMode, ModuleAction.ActionLocationEnum location) {
            MenuList menuList = base.GetModuleMenuList(renderMode, location);
            LoginConfigData config = LoginConfigDataProvider.GetConfig();
            RegisterModule regMod = (RegisterModule)ModuleDefinition.CreateUniqueModule(typeof(RegisterModule));
            LoginModule loginMod = (LoginModule)ModuleDefinition.CreateUniqueModule(typeof(LoginModule));
            menuList.New(loginMod.GetAction_Login(Manager.CurrentSite.LoginUrl, Force: true), location);
            menuList.New(regMod.GetAction_Register(config.RegisterUrl, Force: true), location);
            return menuList;
        }

        public ModuleAction GetAction_ForgotPassword(string url) {
            LoginConfigData config = LoginConfigDataProvider.GetConfig();
            if (!config.SavePlainTextPassword) return null;
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                Image = "#Help",
                AddToOriginList = false,
                SaveReturnUrl = true,
                LinkText = this.__ResStr("regLink", "Forgot your password?"),
                MenuText = this.__ResStr("regText", "Forgot your password?"),
                Tooltip = this.__ResStr("regTooltip", "If you have an account and forgot your password, click to have an email sent to you with your password"),
                Legend = this.__ResStr("regLegend", "Used to send an email to you with your password if you have an account and forgot your password"),
                Style = Manager.IsInPopup ? ModuleAction.ActionStyleEnum.ForcePopup : ModuleAction.ActionStyleEnum.Normal,
                Location = ModuleAction.ActionLocationEnum.NoAuto | ModuleAction.ActionLocationEnum.InPopup | ModuleAction.ActionLocationEnum.ModuleLinks,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
            };
        }
    }
}