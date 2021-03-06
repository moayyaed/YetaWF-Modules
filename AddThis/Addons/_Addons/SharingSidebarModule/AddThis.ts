/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/AddThis#License */

// Handles events turning the addon on/off (used for dynamic content)
$YetaWF.registerContentChange((addonGuid: string, on: boolean) : void => {
    if (addonGuid === "d790d324-ec41-419d-abba-fdb03652cd9d") {
        var addThis = $YetaWF.getElement1BySelector("div.addthis-smartlayers");
        if (on)
            addThis.style.display = "block";
        else
            addThis.style.display = "none";
    }
});