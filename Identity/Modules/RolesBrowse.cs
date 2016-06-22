﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Menus;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;
using YetaWF.DataProvider;
using YetaWF.Modules.Identity.Controllers;
using YetaWF.Modules.Identity.DataProvider;

namespace YetaWF.Modules.Identity.Modules {

    public class RolesBrowseModuleDataProvider : ModuleDefinitionDataProvider<Guid, RolesBrowseModule>, IInstallableModel { }

    [ModuleGuid("{cc4761a9-977c-438b-880a-3381ab78b4a3}")]
    [UniqueModule(UniqueModuleStyle.NonUnique)]
    public class RolesBrowseModule : ModuleDefinition {

        public RolesBrowseModule() : base() {
            Title = this.__ResStr("modTitle", "Roles");
            Name = this.__ResStr("modName", "Roles");
            Description = this.__ResStr("modSummary", "Displays and manages roles");
        }

        public override IModuleDefinitionIO GetDataProvider() { return new RolesBrowseModuleDataProvider(); }

        [Category("General"), Caption("Add URL"), Description("The URL to add a new role - if omitted, a default page is generated")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local), StringLength(Globals.MaxUrl), Trim]
        public string AddUrl { get; set; }
        [Category("General"), Caption("Display URL"), Description("The URL to display a role - if omitted, a default page is generated")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local), StringLength(Globals.MaxUrl), Trim]
        public string DisplayUrl { get; set; }
        [Category("General"), Caption("Edit URL"), Description("The URL to edit a role - if omitted, a default page is generated")]
        [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local), StringLength(Globals.MaxUrl), Trim]
        public string EditUrl { get; set; }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AdministratorLevel_DefaultAllowedRoles; } }
        public override List<RoleDefinition> ExtraRoles {
            get {
                return new List<RoleDefinition>() { 
                    new RoleDefinition("RemoveRoles", 
                        this.__ResStr("roleRemItemsC", "Remove Roles"), this.__ResStr("roleRemItems", "The role has permission to remove individual roles"), 
                        this.__ResStr("userRemItemsC", "Remove Roles"), this.__ResStr("userRemItems", "The user has permission to remove individual roles")),
                };
            }
        }

        public override MenuList GetModuleMenuList(ModuleAction.RenderModeEnum renderMode, ModuleAction.ActionLocationEnum location) {
            MenuList menuList = base.GetModuleMenuList(renderMode, location);
            RolesAddModule mod = new RolesAddModule();
            menuList.New(mod.GetAction_Add(AddUrl), location);
            return menuList;
        }

        public ModuleAction GetAction_Roles(string url) {
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                Image = "#Browse",
                LinkText = this.__ResStr("rolesLink", "Roles"),
                MenuText = this.__ResStr("rolesText", "Roles"),
                Tooltip = this.__ResStr("rolesTooltip", "Display and manages roles"),
                Legend = this.__ResStr("rolesLegend", "Displays and manages roles"),
                Style = ModuleAction.ActionStyleEnum.Normal,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                Category = ModuleAction.ActionCategoryEnum.Read,
                Mode = ModuleAction.ActionModeEnum.Any,
            };
        }
        public ModuleAction GetAction_RemoveLink(string name) {
            if (!IsAuthorized("RemoveRoles")) return null;
            return new ModuleAction(this) {
                Url = YetaWFManager.UrlFor(typeof(RolesBrowseModuleController), "Remove"),
                QueryArgs = new { Name = name },
                Image = "#Remove",
                NeedsModuleContext = true,
                Style = ModuleAction.ActionStyleEnum.Post,
                LinkText = this.__ResStr("removeLink", "Remove Role"),
                MenuText = this.__ResStr("removeMenu", "Remove Role"),
                Tooltip = this.__ResStr("removeTT", "Remove the role"),
                Legend = this.__ResStr("removeLegend", "Removes the role"),
                Category = ModuleAction.ActionCategoryEnum.Delete,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                ConfirmationText = this.__ResStr("removeConfirm", "Are you sure you want to remove role \"{0}\"?", name),
            };
        }

        /// <summary>
        /// Used from site template to add a site admin role
        /// </summary>
        public void AddAdministratorRole() {
            using (RoleDefinitionDataProvider dataProvider = new RoleDefinitionDataProvider()) {
                dataProvider.AddAdministratorRole();
            }
        }
    }
}