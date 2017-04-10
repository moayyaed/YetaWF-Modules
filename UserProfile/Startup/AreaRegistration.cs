/* Copyright � 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/UserProfile#License */

using YetaWF.Core.Packages;

namespace YetaWF.Modules.UserProfile.Controllers {
    public class AreaRegistration : YetaWF.Core.Controllers.AreaRegistrationBase { 
        public AreaRegistration() : base() { CurrentPackage = this.GetCurrentPackage(); }
        public static Package CurrentPackage;
    }
}