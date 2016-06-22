/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/DevTests#License */

using System.Web.Mvc;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;

namespace YetaWF.Modules.DevTests.Controllers {

    public class TemplateEmailModuleController : ControllerImpl<YetaWF.Modules.DevTests.Modules.TemplateEmailModule> {

        public TemplateEmailModuleController() { }

        [Trim]
        public class Model {

            [Caption("Email (Required)"), Description("Email (Required)")]
            [UIHint("Email"), Required, EmailValidation, Trim]
            public string EmailReq { get; set; }

            [Caption("Email"), Description("Email")]
            [UIHint("Email"), EmailValidation, Trim]
            public string Email { get; set; }

            [Caption("Email (Read/Only)"), Description("Email (read/only)")]
            [UIHint("Email"), ReadOnly]
            public string EmailRO { get; set; }

            public Model() {
                EmailRO = "mikevdm@mikevdm.com";
            }
        }

        [HttpGet]
        public ActionResult TemplateEmail() {
            Model model = new Model { };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TemplateEmail_Partial(Model model) {
            if (!ModelState.IsValid)
                return PartialView(model);
            return FormProcessed(model, this.__ResStr("ok", "OK"));
        }
    }
}