"use strict";
/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Messenger#License */
var YetaWF_Messenger;
(function (YetaWF_Messenger) {
    var SkinSiteAnnouncementsModule = /** @class */ (function () {
        function SkinSiteAnnouncementsModule() {
            var _this = this;
            if (YConfigs.SignalR.Version === "MVC5") {
                var $$ = $;
                var connection = $$.hubConnection(YConfigs.SignalR.Url, { useDefaultPath: false });
                var hubProxy = connection.createHubProxy(SkinSiteAnnouncementsModule.PROXY);
                hubProxy.on("Message", function (content, title) {
                    _this.handleMessage(content, title);
                });
                connection.start().done(function () { });
            }
            else {
                var connection_1 = new signalR.HubConnectionBuilder()
                    .withUrl(YConfigs.SignalR.Url + "/" + SkinSiteAnnouncementsModule.PROXY)
                    .configureLogging(signalR.LogLevel.Information)
                    .build();
                connection_1.on("Message", function (content, title) {
                    _this.handleMessage(content, title);
                });
                connection_1.start().then(function () { });
            }
            $YetaWF.registerContentChange(function (addonGuid, on) {
                if (addonGuid === SkinSiteAnnouncementsModule.MODULEGUID) {
                    SkinSiteAnnouncementsModule.on = on;
                }
            });
        }
        SkinSiteAnnouncementsModule.prototype.handleMessage = function (content, title) {
            if (SkinSiteAnnouncementsModule.on)
                $YetaWF.alert(content, title, undefined, { encoded: true, canClose: true, autoClose: 0 });
        };
        SkinSiteAnnouncementsModule.MODULEGUID = "54F6B691-B835-4568-90AA-AA9B308D4272";
        SkinSiteAnnouncementsModule.PROXY = "YetaWF_Messenger_SiteAnnouncementsHub";
        SkinSiteAnnouncementsModule.on = true;
        return SkinSiteAnnouncementsModule;
    }());
    YetaWF_Messenger.SkinSiteAnnouncementsModule = SkinSiteAnnouncementsModule;
})(YetaWF_Messenger || (YetaWF_Messenger = {}));

//# sourceMappingURL=SkinSiteAnnouncements.js.map
