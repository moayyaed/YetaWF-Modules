/* Copyright � 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Collections.Generic;
using YetaWF.Core.Controllers;
using YetaWF.Core.Identity;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Serializers;
using YetaWF.Modules.Identity.Modules;
#if MVC6
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Identity.Controllers {

    public class LoginSelectorModuleController : ControllerImpl<YetaWF.Modules.Identity.Modules.LoginSelectorModule> {

        public LoginSelectorModuleController() { }

        public class EditModel {
            [Caption("Active User"), Description("List of user accounts that can be used to quickly log into the site")]
            [UIHint("YetaWF_Identity_LoginUsers"), SubmitFormOnChange]
            public int UserId { get; set; }

            [Caption("Superuser"), Description("If a superuser was signed on previously in this session, the superuser status remains even if logged in as another user - Uncheck to turn off superuser mode for this session")]
            [UIHint("Boolean"), SuppressIfNotEqual("SuperuserStillActive", true), SubmitFormOnChange]
            public bool? SuperuserStillActive { get; set; }

            [Caption("Superuser"), Description("The currently logged on user is a superuser")]
            [UIHint("Boolean"), SuppressIfEqual("SuperuserCurrent", false), ReadOnly]
            public bool SuperuserCurrent { get; set; }

            public SerializableList<User> UserId_List { get; set; }

            public void UpdateData(LoginSelectorModule module) {
                UserId_List = module.Users;
            }
        }

        [AllowGet]
        public ActionResult LoginSelector() {
#if !DEBUG
            if (Manager.Deployed && !Manager.HasSuperUserRole) return new EmptyResult();
#endif
            if (Manager.RenderStaticPage) return new EmptyResult();

            EditModel model = new EditModel { UserId = Manager.UserId, };

            List<int> list = Manager.UserRoles;
            int superuserRole = Resource.ResourceAccess.GetSuperuserRoleId();
            if (Manager.UserRoles != null && Manager.UserRoles.Contains(superuserRole))
                model.SuperuserCurrent = true;// the current user is a superuser
            else if (Manager.HasSuperUserRole)
                model.SuperuserStillActive = true;
            else
                model.SuperuserStillActive = false;
            model.UpdateData(Module);
            return View(model);
        }

        [AllowPost]
        [ConditionalAntiForgeryToken]
        public ActionResult LoginSelector_Partial(EditModel model) {
            model.UpdateData(Module);
            if (!ModelState.IsValid)
                return PartialView(model);

            int userId = model.UserId;
            if (userId == 0)
                LoginModuleController.UserLogoff();
            else
                Resource.ResourceAccess.LoginAs(userId);
            if (model.SuperuserStillActive != null && !(bool)model.SuperuserStillActive)
                Manager.SetSuperUserRole(false);
            return Redirect(Manager.ReturnToUrl);
        }
    }
}