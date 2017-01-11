/* Copyright � 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/TinyLogin#License */

using System.Web.Mvc;
using YetaWF.Core.Controllers;
using YetaWF.Core.Models.Attributes;

namespace YetaWF.Modules.TinyLogin.Controllers {

    public class TinyLoginModuleController : ControllerImpl<YetaWF.Modules.TinyLogin.Modules.TinyLoginModule> {

        public TinyLoginModuleController() { }

        [Trim]
        public class TinyLoginModel {

            public string UserName { get; set; }
            public bool LoggedOn { get; set; }

            public string LogonUrl { get; set; }
            public string LogoffUrl { get; set; }
            public string RegisterUrl { get; set; }
            public string UserUrl { get; set; }
            public string UserTooltip { get; set; }

            public TinyLoginModel() { }
        }

        [HttpGet]
        public ActionResult TinyLogin() {
            TinyLoginModel model = new TinyLoginModel {
                UserName = Manager.UserName,
                LoggedOn = Manager.HaveUser,
                LogonUrl = string.IsNullOrWhiteSpace(Module.LogonUrl) ? Manager.CurrentSite.LoginUrl : Module.LogonUrl,
                LogoffUrl = string.IsNullOrWhiteSpace(Module.LogoffUrl) ? Manager.CurrentSite.HomePageUrl : Module.LogoffUrl,
                RegisterUrl = string.IsNullOrWhiteSpace(Module.RegisterUrl) ? Manager.CurrentSite.LoginUrl : Module.RegisterUrl,
                UserUrl = string.IsNullOrWhiteSpace(Module.UserUrl) ? Manager.CurrentSite.HomePageUrl : Module.UserUrl,
                UserTooltip = Module.UserTooltip,
            };
            return View(model);
        }
    }
}