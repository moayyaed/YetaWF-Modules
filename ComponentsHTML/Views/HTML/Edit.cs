﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Modules;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.ComponentsHTML.Controllers;

namespace YetaWF.Modules.ComponentsHTML.Views {

    public class EditView : YetaWFView, IYetaWFView2<ModuleDefinition, object> {

        public const string ViewName = ModuleDefinition.StandardViews.Edit;

        public override Package GetPackage() { return AreaRegistration.CurrentPackage; }
        public override string GetViewName() { return ViewName; }

        public async Task<YHtmlString> RenderViewAsync(ModuleDefinition module, object model) {

            HtmlBuilder hb = new HtmlBuilder();

            string actionName = (string)HtmlHelper.ViewContext.RouteData.Values["action"];

            hb.Append($@"
{await RenderBeginFormAsync(ActionName: actionName)}
    {await PartialForm(async () => await RenderPartialViewAsync(module, model))}
    {await FormButtonsAsync(new FormButton[] {
        new FormButton() { ButtonType= ButtonTypeEnum.Submit, },
        new FormButton() { ButtonType= ButtonTypeEnum.Cancel, },
    })}
{await RenderEndFormAsync()}");
            return hb.ToYHtmlString();
        }

        public async Task<YHtmlString> RenderPartialViewAsync(ModuleDefinition module, object model) {

            HtmlBuilder hb = new HtmlBuilder();
            hb.Append(await HtmlHelper.ForEditContainerAsync(model, "PropertyList"));
            return hb.ToYHtmlString();

        }
    }
}
