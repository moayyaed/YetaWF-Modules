/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/DevTests#License */

using System;
using System.Web.Mvc;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;

namespace YetaWF.Modules.DevTests.Controllers {

    public class TemplateModuleSelectionModuleController : ControllerImpl<YetaWF.Modules.DevTests.Modules.TemplateModuleSelectionModule> {

        public TemplateModuleSelectionModuleController() { }

        [Trim]
        public class EditModel {

            [Caption("Module Selection"), Description("Existing Module")]
            [UIHint("ModuleSelection"), Required, Trim]
            public Guid Module { get; set; }

            [Caption("Module Selection (New)"), Description("New Module")]
            [UIHint("ModuleSelection"), AdditionalMetadata("New", true), Required, Trim]
            public Guid ModuleNew { get; set; }

            [Caption("Module Selection (r/o)"), Description("Existing Module")]
            [UIHint("ModuleSelection")]
            [ReadOnly]
            public Guid ROModule { get; set; }

            [Caption("Module Selection (r/o)"), Description("Existing Module")]
            [UIHint("ModuleSelection"), AdditionalMetadata("New", true)]
            [ReadOnly]
            public Guid ROModuleNew { get; set; }

            public EditModel() { }
        }

        [HttpGet]
        public ActionResult TemplateModuleSelection() {
            EditModel model = new EditModel { };
            model.ROModule = Module.PermanentGuid;// use this module as displayed module
            model.ROModuleNew = Module.PermanentGuid;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TemplateModuleSelection_Partial(EditModel model) {
            if (!ModelState.IsValid)
                return PartialView(model);
            return FormProcessed(model, this.__ResStr("ok", "OK"));
        }
    }
}