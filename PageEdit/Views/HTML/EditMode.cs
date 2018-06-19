﻿using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.PageEdit.Controllers;
using YetaWF.Modules.PageEdit.Modules;

namespace YetaWF.Modules.PageEdit.Views {

    public class EditModeView : YetaWFView, IYetaWFView<EditModeModule, EditModeModuleController.Model> {

        public const string ViewName = "EditMode";

        public override Package GetPackage() { return AreaRegistration.CurrentPackage; }
        public override string GetViewName() { return ViewName; }

        public Task<YHtmlString> RenderViewAsync(EditModeModule module, EditModeModuleController.Model model) {

            // this view is used to include js/css only
            HtmlBuilder hb = new HtmlBuilder();
            return Task.FromResult(hb.ToYHtmlString());
        }
    }
}
