"use strict";
/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */
var YetaWF_ComponentsHTML;
(function (YetaWF_ComponentsHTML) {
    var ClipboardSupport = /** @class */ (function () {
        function ClipboardSupport() {
        }
        ClipboardSupport.initAll = function (tag) {
            if (ClipboardSupport.clipText != null && ClipboardSupport.clipTextArea)
                return;
            ClipboardSupport.clipText = new ClipboardJS(".yt_text_copy", {
                target: function (trigger) {
                    return trigger.previousElementSibling;
                },
            });
            ClipboardSupport.clipTextArea = new ClipboardJS(".yt_textareasourceonly_copy", {
                target: function (trigger) {
                    return trigger.previousElementSibling;
                },
            });
            ClipboardSupport.clipText.on("success", function (e) {
                $YetaWF.confirm(YLocs.YetaWF_ComponentsHTML.CopyToClip);
            });
            ClipboardSupport.clipTextArea.on("success", function (e) {
                $YetaWF.confirm(YLocs.YetaWF_ComponentsHTML.CopyToClip);
            });
        };
        ClipboardSupport.clipText = null;
        ClipboardSupport.clipTextArea = null;
        return ClipboardSupport;
    }());
    YetaWF_ComponentsHTML.ClipboardSupport = ClipboardSupport;
    /* handle copy icon */
    $YetaWF.addWhenReady(ClipboardSupport.initAll);
})(YetaWF_ComponentsHTML || (YetaWF_ComponentsHTML = {}));
