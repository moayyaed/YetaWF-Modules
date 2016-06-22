/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Feedback#License */

using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Support;
using YetaWF.Modules.Feedback.DataProvider;

namespace YetaWF.Modules.Feedback.Controllers {

    public class FeedbackConfigModuleController : ControllerImpl<YetaWF.Modules.Feedback.Modules.FeedbackConfigModule> {

        public FeedbackConfigModuleController() { }

        [Trim]
        public class Model {

            [Caption("Use Captcha"), Description("The user has to pass a \"human\" test by entering a code shown")]
            [UIHint("Boolean")]
            public bool Captcha { get; set; }

            [Caption("Email Address Required"), Description("Defines whether the user must enter his/her email address to be able to send feedback")]
            [UIHint("Boolean")]
            public bool RequireEmail { get; set; }

            [Caption("Email Address"), Description("The email address where all feedback messages are sent")]
            [UIHint("Email"), StringLength(Globals.MaxEmail), EmailValidation, Trim]
            public string Email { get; set; }

            [Caption("Copy Feedback Emails"), Description("Defines whether the site administrator receives a copy of all feedback messages")]
            [UIHint("Boolean")]
            public bool BccEmails { get; set; }

            public FeedbackConfigData GetData(FeedbackConfigData data) {
                ObjectSupport.CopyData(this, data);
                return data;
            }
            public void SetData(FeedbackConfigData data) {
                ObjectSupport.CopyData(data, this);
            }
            public Model() { }
        }

        [HttpGet]
        public ActionResult FeedbackConfig() {
            using (FeedbackConfigDataProvider dataProvider = new FeedbackConfigDataProvider()) {
                Model model = new Model { };
                FeedbackConfigData data = dataProvider.GetItem();
                if (data == null)
                    throw new Error(this.__ResStr("notFound", "The feedback settings were not found."));
                model.SetData(data);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeedbackConfig_Partial(Model model) {
            using (FeedbackConfigDataProvider dataProvider = new FeedbackConfigDataProvider()) {
                FeedbackConfigData data = dataProvider.GetItem();// get the original item
                if (!ModelState.IsValid)
                    return PartialView(model);
                data = model.GetData(data); // merge new data into original
                model.SetData(data); // and all the data back into model for final display
                dataProvider.UpdateConfig(data);
                return FormProcessed(model, this.__ResStr("okSaved", "Feedback settings saved"), NextPage: Manager.ReturnToUrl);
            }
        }
    }
}