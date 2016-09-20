/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Identity.DataProvider;
using YetaWF.Modules.Identity.Modules;

namespace YetaWF.Modules.Identity.Controllers {

    public class LoginConfigModuleController : ControllerImpl<YetaWF.Modules.Identity.Modules.LoginConfigModule> {

        public LoginConfigModuleController() { }

        [Trim]
        public class Model {

            [Category("Accounts"), Caption("Allow New Users"), Description("Allow registration of new users")]
            [UIHint("Boolean")]
            public bool AllowUserRegistration { get; set; }

            [Category("Accounts"), Caption("Registration Type"), Description("How a user is registered on this site")]
            [UIHint("Enum")]
            public RegistrationTypeEnum RegistrationType { get; set; }

            [Category("Accounts"), Caption("Save Password"), Description("Defines whether the user's password is saved so it can be recovered (and emailed to the user if necessary)")]
            [UIHint("Boolean")]
            public bool SavePlainTextPassword { get; set; }

            [Category("Accounts"), Caption("Verification Required"), Description("Defines whether new users need to be verified before they have access to the site - Verification is performed by sending an email to the new user with a verification code, which must be entered when logging in the first time")]
            [UIHint("Boolean")]
            public bool VerifyNewUsers { get; set; }

            [Category("Accounts"), Caption("Approval Required"), Description("Defines whether new users need to be approved before they have access to the site - Approval is typically given by the site administrator")]
            [UIHint("Boolean")]
            public bool ApproveNewUsers { get; set; }

            [Category("Accounts"), Caption("Persistent Login"), Description("Defines whether a user login is persistent (saved using cookies)")]
            [UIHint("Boolean")]
            public bool PersistentLogin { get; set; }

            [Category("Accounts"), Caption("Max. Login Failures"), Description("Defines the maximum number of login failures before the account is suspended - If 0 is specified login failures will not suspend accounts - A successful login resets the login failure count to 0")]
            [UIHint("IntValue4"), Range(0,9999), Required]
            public int MaxLoginFailures { get; set; }

            [Category("Notifications"), Caption("Notify Admin - New User"), Description("Defines whether the administrator receives email notification when a new user account is created by a new user")]
            [UIHint("Boolean")]
            public bool NotifyAdminNewUsers { get; set; }

            [Category("Notifications"), Caption("Notify Admin - Verification Emails"), Description("Defines whether the site administrator receives a copy of the verification email sent to new users")]
            [UIHint("Boolean")]
            public bool BccVerification { get; set; }

            [Category("Notifications"), Caption("Notify Admin - Forgot Password"), Description("Defines whether the site administrator receives a copy of the forgotten password email sent to users")]
            [UIHint("Boolean")]
            public bool BccForgottenPassword { get; set; }

            [Category("Captcha"), Caption("Use Captcha"), Description("Defines whether the user has to pass a \"human\" test when logging in or registering a new account (Yes), otherwise no test is performed (No)")]
            [UIHint("Boolean")]
            public bool Captcha { get; set; }

            [Category("Captcha"), Caption("Use Captcha (Forgot Pswd)"), Description("For forgotten passwords, defines whether the user has to pass a \"human\" test (Yes), otherwise no test is performed (No)")]
            [UIHint("Boolean")]
            public bool CaptchaForgotPassword { get; set; }

            [Category("Urls")]
            [Caption("Register Url"), Description("The Url where the user can register a new user account")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string RegisterUrl { get; set; }

            [Category("Urls")]
            [Caption("Forgotten Password Url"), Description("The Url where the user can retrieve a forgotten password for an existing account")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string ForgotPasswordUrl { get; set; }

            [Category("Urls")]
            [Caption("Verification Pending Url"), Description("The Url where the user is redirected when account verification is pending after registering or while logging on")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string VerificationPendingUrl { get; set; }

            [Category("Urls")]
            [Caption("Approval Pending Url"), Description("The Url where the user is redirected when account approval is pending after registering or while logging on")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string ApprovalPendingUrl { get; set; }

            [Category("Urls")]
            [Caption("Rejected Url"), Description("The Url where the user is redirected when his/her account has been rejected")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string RejectedUrl { get; set; }

            [Category("Urls")]
            [Caption("Suspended Url"), Description("The Url where the user is redirected when his/her account has been suspended")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string SuspendedUrl { get; set; }

            [Category("Urls")]
            [Caption("Logged Off Url"), Description("The Url where the user is redirected after logging off")]
            [UIHint("Url"), AdditionalMetadata("UrlType", UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote), UrlValidation(UrlValidationAttribute.SchemaEnum.Any, UrlHelperEx.UrlTypeEnum.Local | UrlHelperEx.UrlTypeEnum.Remote)]
            [StringLength(Globals.MaxUrl), Trim]
            public string LoggedOffUrl { get; set; }

            public LoginConfigData GetData(LoginConfigData data) {
                ObjectSupport.CopyData(this, data);
                return data;
            }
            public void SetData(LoginConfigData data) {
                ObjectSupport.CopyData(data, this);
            }
            public Model() { }
        }

        [HttpGet]
        public ActionResult LoginConfig() {
            using (LoginConfigDataProvider dataProvider = new LoginConfigDataProvider()) {
                Model model = new Model { };
                LoginConfigData data = dataProvider.GetItem();
                if (data == null)
                    throw new Error(this.__ResStr("notFound", "User login configuration was not found."));
                model.SetData(data);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExcludeDemoMode]
        public ActionResult LoginConfig_Partial(Model model) {
            using (LoginConfigDataProvider dataProvider = new LoginConfigDataProvider()) {
                LoginConfigData data = dataProvider.GetItem();// get the original item
                if (!ModelState.IsValid)
                    return PartialView(model);
                data = model.GetData(data); // merge new data into original
                model.SetData(data); // and all the data back into model for final display
                dataProvider.UpdateConfig(data);
                return FormProcessed(model, this.__ResStr("okSaved", "Configuration settings saved"));
            }
        }
    }
}