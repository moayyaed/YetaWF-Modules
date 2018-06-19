﻿using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.SiteProperties.Controllers;
using YetaWF.Modules.SiteProperties.Modules;

namespace YetaWF.Modules.SiteProperties.Views {

    public class SitePropertiesView : YetaWFView, IYetaWFView2<SitePropertiesModule, SitePropertiesModuleController.SitePropertiesModel> {

        public const string ViewName = "SiteProperties";

        public override Package GetPackage() { return AreaRegistration.CurrentPackage; }
        public override string GetViewName() { return ViewName; }

        public async Task<YHtmlString> RenderViewAsync(SitePropertiesModule module, SitePropertiesModuleController.SitePropertiesModel model) {

            HtmlBuilder hb = new HtmlBuilder();

            hb.Append($@"
{await RenderBeginFormAsync()}
    {await PartialForm(async () => await RenderPartialViewAsync(module, model))}
    {await FormButtonsAsync(new FormButton[] {
        new FormButton() { ButtonType= ButtonTypeEnum.Submit, },
        new FormButton() { ButtonType= ButtonTypeEnum.Cancel, },
    })}
{await RenderEndFormAsync()}");
            return hb.ToYHtmlString();
        }

        public async Task<YHtmlString> RenderPartialViewAsync(SitePropertiesModule module, SitePropertiesModuleController.SitePropertiesModel model) {

            HtmlBuilder hb = new HtmlBuilder();

            hb.Append($@"
{await HtmlHelper.ForDisplayAsync(model, nameof(model.SiteHost))}
{await HtmlHelper.ForEditContainerAsync(model.Site, "PropertyList")}");
            return hb.ToYHtmlString();
        }
    }
}
