﻿/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/SyntaxHighlighter#License */

using System;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;

namespace YetaWF.Modules.SyntaxHighlighter.Modules {

    public class SkinSyntaxHighlighterModuleDataProvider : ModuleDefinitionDataProvider<Guid, SkinSyntaxHighlighterModule>, IInstallableModel { }

    [ModuleGuid("{7e3c4322-5bdb-44bf-acff-f62d498705ee}"), PublishedModuleGuid]
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    [ModuleCategory("Alex Gorbatchev")]
    public class SkinSyntaxHighlighterModule : ModuleDefinition {

        public SkinSyntaxHighlighterModule() {
            Title = this.__ResStr("modTitle", "Skin Syntax Highlighter (Alex Gorbatchev)");
            Name = this.__ResStr("modName", "Syntax Highlighter Alex Gorbatchev (Skin)");
            Description = this.__ResStr("modSummary", "Skin module supporting syntax highlighting in modules, by Alex Gorbatchev, referenced by sites, pages or modules, in which case <pre> .. </pre> sections are rendered using syntax highlighting.");
            WantFocus = false;
            ShowTitle = false;
            WantSearch = false;
            Invokable = true;
            InvokeInPopup = true;
            InvokeInAjax = false;
        }

        public override IModuleDefinitionIO GetDataProvider() { return new SkinSyntaxHighlighterModuleDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }
    }
}