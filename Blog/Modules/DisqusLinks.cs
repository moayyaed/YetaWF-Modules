﻿/* Copyright © 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Blog#License */

using System;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Serializers;
using YetaWF.DataProvider;

namespace YetaWF.Modules.Blog.Modules {

    public class DisqusLinksModuleDataProvider : ModuleDefinitionDataProvider<Guid, DisqusLinksModule>, IInstallableModel { }

    [ModuleGuid("{776adfcd-da5f-4926-b29d-4c06353266c0}")]
    [UniqueModule(UniqueModuleStyle.UniqueOnly)]
    public class DisqusLinksModule : ModuleDefinition {

        public DisqusLinksModule() {
            Title = this.__ResStr("modTitle", "Disqus Links");
            Name = this.__ResStr("modName", "Disqus Links (Skin)");
            Description = this.__ResStr("modSummary", "Adds number of comments to links to pages with Disqus comments");
            WantFocus = false;
            WantSearch = false;
            ShowTitle = false;
            Invokable = true;
        }

        public override IModuleDefinitionIO GetDataProvider() { return new DisqusLinksModuleDataProvider(); }

        public override SerializableList<AllowedRole> DefaultAllowedRoles { get { return AnonymousLevel_DefaultAllowedRoles; } }
    }
}