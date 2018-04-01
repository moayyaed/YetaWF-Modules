﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Threading.Tasks;
using YetaWF.Core.Localize;
using YetaWF.Core.Modules;
using YetaWF.Core.Packages;
using YetaWF.Core.SendEmail;
using YetaWF.Core.Support;
using YetaWF.Modules.Identity.DataProvider;
using YetaWF.Modules.Identity.Modules;

namespace YetaWF.Modules.Identity.Support {

    public class Emails {

        protected YetaWFManager Manager { get { return YetaWFManager.Manager; } }

        public const string EmailsFolder = "Emails";

        public Emails() { }

        public async Task SendForgottenEmailAsync(UserDefinition user, string ccEmail = null) {
            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
                LoginUrl = Manager.CurrentSite.MakeUrl(Manager.CurrentSite.LoginUrl),
            };
            string subject = this.__ResStr("forgotSubject", "Forgotten password for {0}", Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(user.Email, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Forgot Password.txt"), parameters: parms);
            if (!string.IsNullOrWhiteSpace(ccEmail))
                sendEmail.AddBcc(ccEmail);
            await sendEmail.SendAsync(true);
        }
        public async Task SendVerificationAsync(UserDefinition user, string ccEmail = null) {
            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
                LoginUrl = Manager.CurrentSite.MakeUrl(Manager.CurrentSite.LoginUrl),
            };
            string subject = this.__ResStr("verificationSubject", "Verification required for site {0}", Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(user.Email, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Account Verification.txt"), parameters: parms);
            if (!string.IsNullOrWhiteSpace(ccEmail))
                sendEmail.AddBcc(ccEmail);
            await sendEmail.SendAsync(true);
        }

        public async Task SendApprovalAsync(UserDefinition user, string ccEmail = null) {
            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
                LoginUrl = Manager.CurrentSite.MakeUrl(Manager.CurrentSite.LoginUrl),
            };
            string subject = this.__ResStr("approvalSubject", "Approved for site {0}!", Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(user.Email, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Account Approved.txt"), parameters: parms);
            if (!string.IsNullOrWhiteSpace(ccEmail))
                sendEmail.AddBcc(ccEmail);
            await sendEmail.SendAsync(true);
        }

        public async Task SendApprovalNeededAsync(UserDefinition user) {
            // get the registration module for some defaults
            RegisterModule regMod = (RegisterModule)await ModuleDefinition.CreateUniqueModuleAsync(typeof(RegisterModule));
            ModuleAction approve = await regMod.GetAction_ApproveAsync(user.UserName);
            ModuleAction reject = await regMod.GetAction_RejectAsync(user.UserName);

            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
                ApprovalUrl = approve.GetCompleteUrl(),
                RejectUrl = reject.GetCompleteUrl(),
            };
            string subject = this.__ResStr("approvalNeededSubject", "Approval required for user {0} - site {1}", user.UserName, Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(null, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Account Approval.txt"), parameters: parms);
            await sendEmail.SendAsync(false);
        }

        public async Task SendRejectedAsync(UserDefinition user, string ccEmail = null) {
            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
            };
            string subject = this.__ResStr("rejectedSubject", "Your account for {0}", Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(user.Email, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Account Rejected.txt"), parameters: parms);
            if (!string.IsNullOrWhiteSpace(ccEmail))
                sendEmail.AddBcc(ccEmail);
            await sendEmail.SendAsync(true);
        }

        public async Task SendSuspendedAsync(UserDefinition user, string ccEmail = null) {
            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
            };
            string subject = this.__ResStr("suspendedSubject", "Your account for {0}", Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(user.Email, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "Account Suspended.txt"), parameters: parms);
            if (!string.IsNullOrWhiteSpace(ccEmail))
                sendEmail.AddBcc(ccEmail);
            await sendEmail.SendAsync(true);
        }

        public async Task SendNewUserCreatedAsync(UserDefinition user) {
            // get the registration module for some defaults
            RegisterModule regMod = (RegisterModule)await ModuleDefinition.CreateUniqueModuleAsync(typeof(RegisterModule));
            ModuleAction reject = await regMod.GetAction_RejectAsync(user.UserName);

            SendEmail sendEmail = new SendEmail();
            object parms = new {
                User = user,
                RejectUrl = reject.GetCompleteUrl(),
            };
            string subject = this.__ResStr("notifyNewUserSubject", "New account for user {0} - site  {1}", user.UserName, Manager.CurrentSite.SiteDomain);
            await sendEmail.PrepareEmailMessageAsync(null, subject, await sendEmail.GetEmailFileAsync(Package.GetCurrentPackage(this), "New Account Created.txt"), parameters: parms);
            await sendEmail.SendAsync(false);
        }
    }
}