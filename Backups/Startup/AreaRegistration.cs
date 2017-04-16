﻿/* Copyright © 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Backups#License */

using YetaWF.Core.Packages;

namespace YetaWF.Modules.Backups.Controllers {
    public class AreaRegistration : YetaWF.Core.Controllers.AreaRegistrationBase {
        public AreaRegistration() : base() { CurrentPackage = this.GetCurrentPackage(); }
        public static Package CurrentPackage;
    }
}
