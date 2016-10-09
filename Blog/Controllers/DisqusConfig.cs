/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Blog#License */

using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Blog.DataProvider;
using YetaWF.Modules.Blog.Support;

namespace YetaWF.Modules.Blog.Controllers {

    public class DisqusConfigModuleController : ControllerImpl<YetaWF.Modules.Blog.Modules.DisqusConfigModule> {

        public DisqusConfigModuleController() { }

        [Trim]
        public class Model {

            [Caption("Shortname"), Description("The Shortname you assigned to your site (at Disqus)")]
            [UIHint("Text40"), StringLength(DisqusConfigData.MaxShortName), ShortNameValidation, Required, Trim]
            [HelpLink("https://yetawf.disqus.com/admin/settings/general/")]
            public string ShortName { get; set; }

            [Caption("Single Sign On"), Description("Defines whether SSO (Single Sign On) is enabled for your site allowing users to log in using their credentials")]
            [UIHint("Boolean")]
            public bool UseSSO { get; set; }

            [Caption("Secret Key"), Description("Defines the Secret Key used for SSO (Single Sign On) - The Secret Key is created on the Disqus site when defining the SSO application")]
            [UIHint("Text80"), StringLength(DisqusConfigData.MaxPublicKey), RequiredIf("UseSSO", true), Trim]
            public string PrivateKey { get; set; }
            [Caption("Public Key"), Description("Defines the Public Key used for SSO (Single Sign On) - The Public Key is created on the Disqus site when defining the SSO application")]
            [UIHint("Text80"), StringLength(DisqusConfigData.MaxPrivateKey), RequiredIf("UseSSO", true), Trim]
            public string PublicKey { get; set; }

            [Caption("Login Url"), Description("Defines the Url used when the user wants to log into the site to leave a comment")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local), StringLength(Globals.MaxUrl), Trim]
            [RequiredIf("UseSSO", true)]
            public string LoginUrl { get; set; }

            [Caption("Login Popup Width"), Description("Defines the width of the popup window created by Disqus to log the user into the site (using SSO)")]
            [UIHint("IntValue4"), Range(20, 9999), RequiredIf("UseSSO", true)]
            public int Width { get; set; }
            [Caption("Login Popup Height"), Description("Defines the height of the popup window created by Disqus to log the user into the site (using SSO)")]
            [UIHint("IntValue4"), Range(20, 9999), RequiredIf("UseSSO", true)]
            public int Height { get; set; }

            public DisqusConfigData GetData(DisqusConfigData data) {
                ObjectSupport.CopyData(this, data);
                return data;
            }
            public void SetData(DisqusConfigData data) {
                ObjectSupport.CopyData(data, this);
            }
            public Model() { }
        }

        [HttpGet]
        public ActionResult DisqusConfig() {
            using (DisqusConfigDataProvider dataProvider = new DisqusConfigDataProvider()) {
                Model model = new Model { };
                DisqusConfigData data = dataProvider.GetItem();
                if (data == null)
                    throw new Error(this.__ResStr("notFound", "The Disqus settings could not be found"));
                model.SetData(data);
                return View(model);
            }
        }

        [HttpPost]
        [ExcludeDemoMode]
        [ValidateAntiForgeryToken]
        public ActionResult DisqusConfig_Partial(Model model) {
            using (DisqusConfigDataProvider dataProvider = new DisqusConfigDataProvider()) {
                DisqusConfigData data = dataProvider.GetItem();// get the original item
                if (!ModelState.IsValid)
                    return PartialView(model);
                data = model.GetData(data); // merge new data into original
                model.SetData(data); // and all the data back into model for final display
                dataProvider.UpdateConfig(data);
                return FormProcessed(model, this.__ResStr("okSaved", "Disqus settings saved"), NextPage: Manager.ReturnToUrl);
            }
        }
    }
}