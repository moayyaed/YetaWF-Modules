﻿using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.Panels.Controllers;
using YetaWF.Modules.Panels.Modules;

namespace YetaWF.Modules.Panels.Views {

    public class ModulePanelView : YetaWFView, IYetaWFView2<ModulePanelModule, ModulePanelModuleController.Model> {

        public const string ViewName = "ModulePanel";

        public override Package GetPackage() { return AreaRegistration.CurrentPackage; }
        public override string GetViewName() { return ViewName; }

        public async Task<YHtmlString> RenderViewAsync(ModulePanelModule module, ModulePanelModuleController.Model model) {

            HtmlBuilder hb = new HtmlBuilder();

            if (Manager.EditMode) {

                hb.Append($@"
{await RenderBeginFormAsync()}
    {await PartialForm(async () => await RenderPartialViewAsync(module, model))}
{await RenderEndFormAsync()}");

            } else {

                hb.Append($@"
{await HtmlHelper.ForDisplayAsync(model, nameof(model.PanelInfo))}");

            }
            return hb.ToYHtmlString();
        }

        public async Task<YHtmlString> RenderPartialViewAsync(ModulePanelModule module, ModulePanelModuleController.Model model) {

            HtmlBuilder hb = new HtmlBuilder();
            hb.Append(await HtmlHelper.ForEditAsync(model, nameof(model.PanelInfo)));
            return hb.ToYHtmlString();

        }
    }
}
