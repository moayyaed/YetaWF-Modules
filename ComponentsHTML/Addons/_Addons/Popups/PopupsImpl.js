"use strict";
/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */
var YetaWF_ComponentsHTML;
(function (YetaWF_ComponentsHTML) {
    var PopupsImpl = /** @class */ (function () {
        function PopupsImpl() {
        }
        /**
         * Close the popup - this can only be used by code that is running within the popup (not the parent document/page)
         */
        PopupsImpl.prototype.closePopup = function (forceReload) {
            if (YVolatile.Basics.IsInPopup) {
                var forced = (forceReload === true);
                if (forced)
                    Y_ReloadWindowPage(window.parent, true);
                // with unified page sets there may actually not be a parent, but window.parent returns itself in this case anyway
                var popup = window.parent.document.YPopupWindowActive;
                PopupsImpl.internalClosePopup(popup);
            }
            document.YPopupWindowActive = null;
            YVolatile.Basics.IsInPopup = false; // we're no longer in a popup
        };
        /**
         * Close the popup - this can only be used by code that is running on the main page (not within the popup)
         */
        PopupsImpl.prototype.closeInnerPopup = function () {
            if (YVolatile.Basics.IsInPopup) {
                var popup = document.YPopupWindowActive;
                PopupsImpl.internalClosePopup(popup);
            }
            document.YPopupWindowActive = null;
            YVolatile.Basics.IsInPopup = false; // we're no longer in a popup
        };
        PopupsImpl.internalClosePopup = function (popup) {
            if (popup) {
                popup.close();
                popup.destroy();
            }
        };
        /**
         * Opens a dynamic popup, usually a div added to the current document.
         */
        PopupsImpl.prototype.openDynamicPopup = function (result) {
            // we're already in a popup
            if (YetaWF_Basics.isInPopup())
                PopupsImpl.closeDynamicPopup();
            // insert <div id="ypopup" class='yPopupDyn'></div> at top of page for the popup window
            // this is automatically removed when destroy() is called
            $("body").prepend("<div id='ypopup' class='yPopupDyn'></div>");
            var $popupwin = $("#ypopup");
            $popupwin.addClass(YVolatile.Skin.PopupCss);
            // add pane content
            var contentLength = result.Content.length;
            for (var i = 0; i < contentLength; i++) {
                // add the pane
                var $pane = $("<div class='yPane'></div>").addClass(result.Content[i].Pane);
                $pane.append(result.Content[i].HTML);
                $popupwin.append($pane);
            }
            var popup = null;
            var acts = [];
            if (YVolatile.Skin.PopupMaximize)
                acts.push("Maximize");
            acts.push("Close");
            // Create the window
            $popupwin.kendoWindow({
                actions: acts,
                width: YVolatile.Skin.PopupWidth,
                height: YVolatile.Skin.PopupHeight,
                draggable: true,
                iframe: false,
                modal: true,
                resizable: false,
                title: result.PageTitle,
                visible: false,
                close: function () {
                    PopupsImpl.closeDynamicPopup();
                },
                animation: false,
                refresh: function () {
                    YetaWF_Basics.setLoading(false);
                },
                error: function (e) {
                    YetaWF_Basics.setLoading(false);
                    YetaWF_Basics.Y_Error("Request failed with status " + e.status);
                }
            });
            // show and center the window
            popup = $popupwin.data("kendoWindow");
            popup.center().open();
            // mark that a popup is active
            document.expando = true;
            document.YPopupWindowActive = popup;
            YVolatile.Basics.IsInPopup = true; // we're in a popup
            YetaWF_Basics.setCondense($popupwin, YVolatile.Skin.PopupWidth);
            return $popupwin;
        };
        PopupsImpl.closeDynamicPopup = function () {
            var $popup = $("#ypopup");
            if ($popup.length > 0) {
                YetaWF_Basics.processClearDiv($popup[0]);
                var popup = $popup.data("kendoWindow");
                // don't call internalClosePopup, otherwise we get close event
                popup.destroy(); // don't close, just destroy
            }
            document.YPopupWindowActive = null;
            YVolatile.Basics.IsInPopup = false; // we're no longer in a popup
        };
        /**
         * Open a static popup, usually a popup based on iframe.
         */
        PopupsImpl.prototype.openStaticPopup = function (url) {
            // we're already in a popup
            if (YetaWF_Basics.isInPopup()) {
                // we handle links within a popup by replacing the current popup page with the new page
                YetaWF_Basics.setLoading(true);
                var $popupwin = $("#ypopup", $(window.parent.document));
                if ($popupwin.length == 0)
                    throw "Couldn't find popup window"; /*DEBUG*/
                var iframeDomElement = $popupwin.children("iframe")[0];
                iframeDomElement.src = url;
                return;
            }
            // insert <div id="ypopup"></div> at top of page for the popup window
            // this is automatically removed when destroy() is called
            $("body").prepend("<div id='ypopup'></div>");
            var $popupwin = $("#ypopup");
            var popup = null;
            var acts = [];
            acts.push("Maximize"); // always show the maximize button - hide it based on popup skin options
            acts.push("Close");
            // Create the window
            $popupwin.kendoWindow({
                actions: acts,
                width: YConfigs.Popups.DefaultPopupWidth,
                height: YConfigs.Popups.DefaultPopupHeight,
                draggable: true,
                iframe: true,
                modal: true,
                resizable: false,
                title: " ",
                visible: false,
                content: url,
                close: function () {
                    var popup = $popupwin.data("kendoWindow");
                    popup.destroy();
                    popup = null;
                    document.YPopupWindowActive = null;
                    YVolatile.Basics.IsInPopup = false;
                },
                animation: false,
                refresh: function () {
                    var iframeDomElement = $popupwin.children("iframe")[0];
                    if (iframeDomElement && iframeDomElement.contentDocument && popup) {
                        var iframeDocumentObject = iframeDomElement.contentDocument;
                        popup.title(iframeDocumentObject.title);
                    }
                    YetaWF_Basics.setLoading(false);
                },
                error: function (e) {
                    YetaWF_Basics.setLoading(false);
                    YetaWF_Basics.Y_Error("Request failed with status " + e.status);
                }
            });
            // show and center the window
            popup = $popupwin.data("kendoWindow");
            //do not open the window here - the loaded content opens it because it knows the desired size
            //popup.center().open();
            // mark that a popup is active
            document.expando = true;
            document.YPopupWindowActive = popup;
            YVolatile.Basics.IsInPopup = true; // we're in a popup
        };
        return PopupsImpl;
    }());
    YetaWF_ComponentsHTML.PopupsImpl = PopupsImpl;
})(YetaWF_ComponentsHTML || (YetaWF_ComponentsHTML = {}));
var YetaWF_PopupsImpl = new YetaWF_ComponentsHTML.PopupsImpl();

//# sourceMappingURL=PopupsImpl.js.map
