﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Modules#License */

using System;
using System.Collections.Generic;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.DataProvider;
using YetaWF.Modules.Modules.Controllers;

namespace YetaWF.Modules.Modules.Modules {

    public class ModulesBrowseModuleDataProvider : ModuleDefinitionDataProvider<Guid, ModulesBrowseModule>, IInstallableModel { }

    [ModuleGuid("{276dcecf-f890-4c54-b010-679ee58f0034}")]
    [UniqueModule(UniqueModuleStyle.NonUnique)]
    public class ModulesBrowseModule : ModuleDefinition {

        public ModulesBrowseModule() {
            Title = this.__ResStr("modTitle", "Modules");
            Name = this.__ResStr("modName", "Modules");
            Description = this.__ResStr("modSummary", "Displays and manages modules");
        }

        public override IModuleDefinitionIO GetDataProvider() { return new ModulesBrowseModuleDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AdministratorLevel_DefaultAllowedRoles; } }
        public override List<RoleDefinition> ExtraRoles {
            get {
                return new List<RoleDefinition>() { 
                    new RoleDefinition("RemoveItems", this.__ResStr("roleRemItemsC", "Remove Modules"), this.__ResStr("roleRemItems", "The role has permission to remove individual modules"), this.__ResStr("userRemItemsC", "Remove Modules"), this.__ResStr("userRemItems", "The user has permission to remove individual modules")),
                };
            }
        }

        public ModuleAction GetAction_Modules(string url) {
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                Image = "#Browse",
                LinkText = this.__ResStr("browseLink", "Modules"),
                MenuText = this.__ResStr("browseText", "Modules"),
                Tooltip = this.__ResStr("browseTooltip", "Display and manage modules"),
                Legend = this.__ResStr("browseLegend", "Displays and manages modules"),
                Style = ModuleAction.ActionStyleEnum.Normal,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                Category = ModuleAction.ActionCategoryEnum.Read,
                Mode = ModuleAction.ActionModeEnum.Any,
            };
        }
        public ModuleAction GetAction_ShowModule(Guid modGuid) {
            return new ModuleAction(this) {
                Url = ModuleDefinition.GetModulePermanentUrl(modGuid),
                Image = "#Display",
                Style = ModuleAction.ActionStyleEnum.NewWindow,
                LinkText = this.__ResStr("displayLink", "Show Module"),
                MenuText = this.__ResStr("displayMenu", "Show Module"),
                Tooltip = this.__ResStr("displayTT", "Display the module in a new window"),
                Legend = this.__ResStr("displayLegend", "Displays the module in a new window"),
                Category = ModuleAction.ActionCategoryEnum.Read,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                SaveReturnUrl = true,
            };
        }
        public ModuleAction GetAction_Remove(Guid moduleGuid) {
            if (!IsAuthorized("RemoveItems")) return null;
            return new ModuleAction(this) {
                Url = YetaWFManager.UrlFor(typeof(ModulesBrowseModuleController), "Remove"),
                NeedsModuleContext = true,
                QueryArgs = new { ModuleGuid = moduleGuid },
                Image = "#Remove",
                Style = ModuleAction.ActionStyleEnum.Post,
                LinkText = this.__ResStr("removeLink", "Remove Module"),
                MenuText = this.__ResStr("removeMenu", "Remove Module"),
                Tooltip = this.__ResStr("removeTT", "Remove the module"),
                Legend = this.__ResStr("removeLegend", "Removes the module"),
                Category = ModuleAction.ActionCategoryEnum.Delete,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                ConfirmationText = this.__ResStr("removeConfirm", "Are you sure you want to remove this module?"),
            };
        }
    }
}