/* Copyright � 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Messenger#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Controllers;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Identity;
using YetaWF.Core.Localize;
using YetaWF.Core.Site;
using YetaWF.Core.Support;
using YetaWF.Modules.Messenger.DataProvider;
using YetaWF.Core;
#if MVC6
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
#else
using Microsoft.AspNet.SignalR;
using System.Web.Mvc;
#endif

namespace YetaWF.Modules.Messenger.Controllers {

    public class SkinMessagingModuleController : ControllerImpl<YetaWF.Modules.Messenger.Modules.SkinMessagingModule> {

        public SkinMessagingModuleController() { }

        public class MessageModel { }

        [AllowGet]
        public async Task<ActionResult> SkinMessaging() {

            await SignalR.UseAsync();
            Manager.ScriptManager.AddLast($"{AreaRegistration.CurrentPackage.AreaName}_{Module.ClassName}", $"new YetaWF_Messenger.SkinMessagingModule('{YetaWFManager.JserEncode(Module.ModuleHtmlId)}');");

            return new EmptyResult();
        }
    }

    public class YetaWF_Messenger_Messaging : Hub {

        public async Task Send(string toUser, string message) {

            YetaWFManager manager = await this.SetupSignalRAsync();
            using (MessagingDataProvider msgDP = new MessagingDataProvider()) {
                if (manager.UserId == 0) throw new InternalError("No current user");

                try {

                    int toUserId = await Resource.ResourceAccess.GetUserIdAsync(toUser);
                    if (toUserId == 0) throw new Error(this.__ResStr("noUser", "User {0} doesn't exist", toUser));

                    Message msg = new Message {
                        FromUser = manager.UserId,
                        ToUser = toUserId,
                        Seen = false,
                        MessageText = message,
                    };
                    if (!await msgDP.AddItemAsync(msg)) throw new InternalError("Message not delivered - Message could not be saved");

                    Dispatch(Clients.User(toUser), "message", msg.Key, manager.UserName, message, Formatting.FormatDateTime(msg.Sent));
                    Dispatch(Clients.User(manager.UserName), "messageSent", msg.Key, toUser, message, Formatting.FormatDateTime(msg.Sent));

                } catch (Exception exc) {
                    string messageText = ErrorHandling.FormatExceptionMessage(exc);

                    Dispatch(Clients.Caller, "notifyException", messageText);
                    Message msg = new Message {
                        FromUser = manager.UserId,
                        ToUser = 0,
                        Seen = false,
                        MessageText = messageText,
                    };
                    await msgDP.AddItemAsync(msg);
                }
            }
        }
        public async Task<List<string>> GetOnlineUsers() {
            YetaWFManager manager = await this.SetupSignalRAsync();
            using (ConnectionDataProvider connDP = new ConnectionDataProvider()) {
                //%%%%%%%%%%%%%%%%%%% limit scope to friend users
                List<DataProviderFilterInfo> filters = null;
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "Name", Operator = "!=", Value = null });
                List<DataProviderSortInfo> sorts = null;
                sorts = DataProviderSortInfo.Join(sorts, new DataProviderSortInfo { Field = "Name", Order = DataProviderSortInfo.SortDirection.Ascending });
                DataProviderGetRecords<Connection> conns = await connDP.GetItemsAsync(0, 0, sorts, filters);

                return (from c in conns.Data select c.Name).Distinct().ToList();
            }
        }
        public async Task<bool> IsUserOnline(string user) {
            YetaWFManager manager = await this.SetupSignalRAsync();
            using (ConnectionDataProvider connDP = new ConnectionDataProvider()) {
                Connection conn = await connDP.GetEntryAsync(user);
                return conn != null;
            }
        }
        public async Task MessageSeen(int key) {
            YetaWFManager manager = await this.SetupSignalRAsync();
            if (manager.UserId == 0) throw new InternalError("No current user");
            using (MessagingDataProvider msgDP = new MessagingDataProvider()) {
                Message msg = await msgDP.GetItemAsync(key);
                if (msg.ToUser != manager.UserId) throw new InternalError("User mismatch");
                if (!msg.Seen) {
                    msg.Seen = true;
                    await msgDP.UpdateItemAsync(msg);
                }
                string fromUser = await Resource.ResourceAccess.GetUserNameAsync(msg.FromUser);
                if (string.IsNullOrWhiteSpace(fromUser)) throw new Error(this.__ResStr("noFromUser", "User {0} doesn't exist", msg.FromUser));
                string toUser = await Resource.ResourceAccess.GetUserNameAsync(msg.ToUser);
                if (string.IsNullOrWhiteSpace(toUser)) throw new Error(this.__ResStr("noToUser", "User {0} doesn't exist", msg.ToUser));

                Dispatch(Clients.User(fromUser), "messageSeen", msg.Key, toUser);
            }
        }
        public async Task AllMessagesSeen(string fromUser) {
            YetaWFManager manager = await this.SetupSignalRAsync();
            if (manager.UserId == 0) throw new InternalError("No current user");

            int fromUserId = await Resource.ResourceAccess.GetUserIdAsync(fromUser);
            if (fromUserId == 0) throw new Error(this.__ResStr("noFromUser", "User {0} doesn't exist", fromUser));

            using (MessagingDataProvider msgDP = new MessagingDataProvider()) {

                List<DataProviderFilterInfo> filters = null;
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "FromUser", Operator = "==", Value = fromUserId });
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "ToUser", Operator = "==", Value = manager.UserId });
                filters = DataProviderFilterInfo.Join(filters, new DataProviderFilterInfo { Field = "Seen", Operator = "==", Value = false });
                DataProviderGetRecords<Message> msgs = await msgDP.GetItemsAsync(0, 0, null, filters);
                foreach (Message msg in msgs.Data) {
                    msg.Seen = true;
                    await msgDP.UpdateItemAsync(msg);
                }
            }
            Dispatch(Clients.User(fromUser), "allMessagesSeen", manager.UserName);
        }

        private async Task UpdateConnection(string user, string ipAddress, string connectionId) {
            try {
                using (ConnectionDataProvider connDP = new ConnectionDataProvider()) {
                    await connDP.UpdateEntryAsync(user, ipAddress, connectionId);
                }
            } catch (Exception) { }
        }
        private void Dispatch(dynamic targets, string message, params object[] parms) {
            targets.Invoke(message, parms);
        }

        // Connection Management

        public override async Task
#if MVC6
            OnConnectedAsync()
#else
            OnConnected()
#endif
        {
            try {
                string host = Context.Headers["Host"];
                SiteDefinition site = await SiteDefinition.LoadSiteDefinitionAsync(host);
                if (site == null) throw new InternalError("No site definition for {0}", host);

                string name = null;
                string ipAddress = null;
                try {
                    if (Context.User != null) {
                        name = Context.User.Identity.Name;
                    }
                } catch (Exception) { }
                try {
                    ipAddress = (string)Context.Request.Environment["server.RemoteIpAddress"];
                } catch (Exception) { }
                string connectionId = Context.ConnectionId;

                try {
                    using (ConnectionDataProvider connDP = new ConnectionDataProvider(site.Identity)) {
                        await connDP.UpdateEntryAsync(name, ipAddress, connectionId);
                    }
                } catch (Exception) { }

                //%%%%%%%%%%%%%%%%%%% notify users in scope of new user
                if (!string.IsNullOrWhiteSpace(name)) {
                    Dispatch(this.Clients.Others, "userConnect", name);
                }
            } catch (Exception) { }
#if MVC6
            await base.OnConnectedAsync();
#else
            await base.OnConnected();
#endif
        }

        public override async Task
#if MVC6
            OnDisconnectedAsync(Exception exception)
#else
            OnDisconnected(bool stopCalled)
#endif
        {

            //%%%%%%%%%%%%%%%%%%% notify users in scope of user disconnect
            string name = null;
            try {
                if (Context.User != null) {
                    name = Context.User.Identity.Name;
                }
                if (!string.IsNullOrWhiteSpace(name)) {
                    Dispatch(this.Clients.Others, "userDisconnect", name);
                }
            } catch (Exception) { }

            try {
                string host = Context.Headers["Host"];
                SiteDefinition site = await SiteDefinition.LoadSiteDefinitionAsync(host);
                if (site == null) throw new InternalError("No site definition for {0}", host);
                using (ConnectionDataProvider connDP = new ConnectionDataProvider(site.Identity)) {
                    await connDP.RemoveItemAsync(Context.ConnectionId);
                }
            } catch (Exception) { }
#if MVC6
            await base.OnDisconnectedAsync(exception);
#else
            await base.OnDisconnected(stopCalled);
#endif
        }
    }
}
