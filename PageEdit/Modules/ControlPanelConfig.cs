﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/PageEdit#License */

using System;
using YetaWF.Core.DataProvider;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;
using YetaWF.Modules.PageEdit.DataProvider;

namespace YetaWF.Modules.PageEdit.Modules {

    public class ControlPanelConfigModuleDataProvider : ModuleDefinitionDataProvider<Guid, ControlPanelConfigModule>, IInstallableModel { }

    [ModuleGuid("{6c41ee8f-fcba-4bbd-90cc-8cae9ccd899e}")]
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    public class ControlPanelConfigModule : ModuleDefinition {

        public ControlPanelConfigModule() {
            Title = this.__ResStr("modTitle", "Control Panel Settings");
            Name = this.__ResStr("modName", "Control Panel Settings");
            Description = this.__ResStr("modSummary", "Edits a site's Control Panel settings");
            SameAsPage = false;
            ShowHelp = true;
        }

        public override IModuleDefinitionIO GetDataProvider() { return new ControlPanelConfigModuleDataProvider(); }
        public override DataProviderImpl GetConfigDataProvider() { return new ControlPanelConfigDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AdministratorLevel_DefaultAllowedRoles; } }

        public ModuleAction GetAction_Edit(string url) {
            return new ModuleAction(this) {
                Url = string.IsNullOrWhiteSpace(url) ? ModulePermanentUrl : url,
                Image = "#Config",
                LinkText = this.__ResStr("editLink", "Control Panel Settings"),
                MenuText = this.__ResStr("editText", "Control Panel Settings"),
                Tooltip = this.__ResStr("editTooltip", "Edit the Control Panel settings"),
                Legend = this.__ResStr("editLegend", "Edits the Control Panel settings"),
                Style = ModuleAction.ActionStyleEnum.Popup,
                Category = ModuleAction.ActionCategoryEnum.Update,
                Mode = ModuleAction.ActionModeEnum.Any,
                Location = ModuleAction.ActionLocationEnum.NoAuto,
                SaveReturnUrl = true,
            };
        }
    }
}