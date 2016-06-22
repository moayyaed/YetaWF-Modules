/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YetaWF.Core;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Identity;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Support;
using YetaWF.Modules.Identity.DataProvider;
using YetaWF.Modules.Identity.Models;
using YetaWF.Modules.Identity.Modules;
using YetaWF.Modules.Identity.Support;

namespace YetaWF.Modules.Identity.Controllers {

    public class UserAccountModuleController : ControllerImpl<YetaWF.Modules.Identity.Modules.UserAccountModule> {

        public UserAccountModuleController() { }

        [Trim]
        public class EditModel {

            [Caption("Name"), Description("Your user name")]
            [UIHint("Text40"), SuppressIfEqual("RegistrationType", RegistrationTypeEnum.EmailOnly), StringLength(Globals.MaxUser), UserNameValidation, Required, Trim]
            public string UserName { get; set; }

            [Caption("Email Address"), Description("Your email address this site uses to communicate with you")]
            [UIHint("Email"), SuppressIfEqual("RegistrationType", RegistrationTypeEnum.NameOnly), StringLength(Globals.MaxEmail), EmailValidation, Required, Trim]
            public string Email { get; set; }

            [Caption("Last Login"), Description("The last time you logged into your account")]
            [UIHint("DateTime"), ReadOnly]
            public DateTime? LastLoginDate { get; set; }

            [Caption("Last Login IP"), Description("The IP address from which the user last logged on this site")]
            [UIHint("IPAddress"), StringLength(Globals.MaxIP), ReadOnly]
            public string LastLoginIP { get; set; }

            [Caption("Last Password Change"), Description("The last time you changed your password")]
            [UIHint("DateTime"), ReadOnly]
            public DateTime? LastPasswordChangedDate { get; set; }
            [Caption("Password Change IP"), Description("The IP address from which the user last changed the password on this site")]
            [UIHint("IPAddress"), StringLength(Globals.MaxIP), ReadOnly]
            public string PasswordChangeIP { get; set; }

            [UIHint("Hidden")]
            public RegistrationTypeEnum RegistrationType { get; set; }
            [UIHint("Hidden")]
            public string OriginalUserName { get; set; }

            public UserDefinition GetData() {
                UserDefinition data = new UserDefinition();
                ObjectSupport.CopyData(this, data);
                return data;
            }

            public void SetData(UserDefinition data) {
                ObjectSupport.CopyData(data, this);
            }
        }

        [HttpGet]
        public ActionResult UserAccount() {
            using (UserDefinitionDataProvider dataProvider = new UserDefinitionDataProvider()) {
                LoginConfigData config = LoginConfigDataProvider.GetConfig();
                EditModel model = new EditModel {
                    RegistrationType = config.RegistrationType,
                };

                // make sure this user exists
                IAuthenticationManager authManager = Manager.CurrentRequest.GetOwinContext().Authentication;
                if (!Manager.CurrentRequest.IsAuthenticated)
                    throw new Error(this.__ResStr("noUser", "There is no logged on user."));
                string userName = User.Identity.Name;
                UserManager<UserDefinition> userManager = Managers.GetUserManager();
                UserDefinition user = userManager.FindByName(userName);
                if (user == null)
                    throw new Error(this.__ResStr("notFound", "User \"{0}\" not found."), userName);
                model.SetData(user);
                model.OriginalUserName = user.UserName;

                Module.Title = this.__ResStr("modEditTitle", "User Account");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccount_Partial(EditModel model) {
            // make sure this user exists
            UserManager<UserDefinition> userManager = Managers.GetUserManager();
            UserDefinition user = userManager.FindByName(model.OriginalUserName);
            if (user == null)
                ModelState.AddModelError("Key", this.__ResStr("alreadyDeleted", "The user named \"{0}\" has been removed and can no longer be updated.", model.OriginalUserName));
            if (!ModelState.IsValid)
                return PartialView(model);

            // update email/user name - can't use an existing email address
            // get the registration module for some defaults
            LoginConfigData config = LoginConfigDataProvider.GetConfig();
            switch (config.RegistrationType) {
                default:
                case RegistrationTypeEnum.NameAndEmail: {
                    using (UserDefinitionDataProvider dataProvider = new UserDefinitionDataProvider()) {
                        List<DataProviderFilterInfo> filters = DataProviderFilterInfo.Join(null, new DataProviderFilterInfo { Field = "Email", Operator = "==", Value = model.Email, });
                        UserDefinition userExists = dataProvider.GetItem(filters);
                        if (userExists != null && user.UserName != userExists.UserName) {
                            ModelState.AddModelError("Email", this.__ResStr("emailUsed", "An account using email address {0} already exists.", model.Email));
                            return PartialView(model);
                        }
                    }
                    break;
                 }
                case RegistrationTypeEnum.EmailOnly:
                    model.UserName = model.Email;
                    break;
                case RegistrationTypeEnum.NameOnly:
                    model.Email = model.UserName;
                    break;
            }

            // save new user info
            ObjectSupport.CopyData(model, user); // merge new data into original
            model.SetData(user); // and all the data back into model for final display

            if (model.OriginalUserName != user.UserName) {
                // user name changed - change data through data provider directly
                using (UserDefinitionDataProvider dataProvider = new UserDefinitionDataProvider()) {
                    UpdateStatusEnum status = dataProvider.UpdateItem(model.OriginalUserName, user);
                    switch (status) {
                        default:
                        case UpdateStatusEnum.RecordDeleted:
                            ModelState.AddModelError("Name", this.__ResStr("alreadyDeleted", "The user named \"{0}\" has been removed and can no longer be updated.", model.OriginalUserName));
                            return PartialView(model);
                        case UpdateStatusEnum.NewKeyExists:
                            ModelState.AddModelError("Name", this.__ResStr("alreadyExists", "A user named \"{0}\" already exists.", model.UserName));
                            return PartialView(model);
                        case UpdateStatusEnum.OK:
                            break;
                    }
                }
                // log the user off and back on so new name takes effect
                IAuthenticationManager authManager = HttpContext.GetOwinContext().Authentication;
                //deleted, done in UserLogff authManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalBearer);
                LoginModuleController.UserLogoff();
                await LoginModuleController.UserLoginAsync(user);
            } else {
                IdentityResult result = userManager.Update(user);
                if (!result.Succeeded) {
                    foreach (string err in result.Errors)
                        ModelState.AddModelError("OldPassword", err);
                    return PartialView(model);
                }
            }

            return FormProcessed(model, this.__ResStr("okSaved", "Your account information has been saved"));
        }
    }
}