﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Messenger#License */

namespace YetaWF_Messenger {

    export class SkinBrowserNotificationsModule {

        static readonly MODULEGUID: string = "7F60ABC1-07A1-49f1-8381-BD4276977FF0";

        static on: boolean = true;

        constructor() {

            if (!("Notification" in window)) {
                console.error("No notification support");
                return;
            }

            var $$: any = $;
            var connection: any = $$.hubConnection(YConfigs.SignalR.Url, { useDefaultPath: false });
            var hubProxy: any = connection.createHubProxy("YetaWF_Messenger_BrowserNotificationsHub");

            hubProxy.on("Message", (title: string, text: string, icon?: string, timeout?: number, url?: string): void => {

                if (SkinBrowserNotificationsModule.on) {

                    switch (Notification.permission) {
                        case "default":
                            Notification.requestPermission().then((result: NotificationPermission): void => {
                                if (result === "granted") {
                                    this.showNotification(title, text, icon, timeout, url);
                                }
                            });
                            break;
                        case "granted":
                            this.showNotification(title, text, icon, timeout, url);
                            break;
                        default:
                            console.error("No permission to show notification");
                    }

                }
            });

            connection.start().done((): void => { /*empty*/ });

            $YetaWF.registerContentChange((addonGuid: string, on: boolean): void => {
                if (addonGuid === SkinBrowserNotificationsModule.MODULEGUID) {
                    SkinBrowserNotificationsModule.on = on;
                }
            });
        }

        private showNotification(title: string, text: string, icon?: string, timeout?: number, url?: string): void {
            var notification = new Notification(title, { body: text, icon: icon, tag: "YetaWF_Messenger.BrowserNotification" });
            if (url) {
                notification.addEventListener("click", (ev: Event): void => {
                    window.open(url, "_blank");
                });
            }
            if (timeout)
                setTimeout(notification.close.bind(notification), timeout);
        }
    }
}
