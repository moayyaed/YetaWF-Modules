﻿/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ModuleEdit#License */

using YetaWF.Core.Packages;

namespace YetaWF.Modules.ModuleEdit.Controllers {
    public class AreaRegistration : YetaWF.Core.Controllers.AreaRegistrationBase {
        public AreaRegistration() : base() { CurrentPackage = this.GetCurrentPackage(); }
        public static Package CurrentPackage;
    }
}
