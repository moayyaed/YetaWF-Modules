"use strict";
/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Panels#License */
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var YetaWF_Panels;
(function (YetaWF_Panels) {
    var PageBarInfoComponent = /** @class */ (function (_super) {
        __extends(PageBarInfoComponent, _super);
        function PageBarInfoComponent(controlId, setup) {
            var _this = _super.call(this, controlId, PageBarInfoComponent.TEMPLATE, PageBarInfoComponent.SELECTOR, {
                ControlType: YetaWF_ComponentsHTML.ControlTypeEnum.Template,
                ChangeEvent: null,
                GetValue: null,
                Enable: null,
            }) || this;
            _this.Setup = setup;
            _this.resize();
            // Link click, activate entry
            $YetaWF.registerEventHandler(_this.Control, "click", ".yt_panels_pagebarinfo_list a", function (ev) {
                var entry = $YetaWF.elementClosestCond(ev.__YetaWFElem, ".yt_panels_pagebarinfo_list .t_entry");
                if (!entry)
                    return true;
                var entries = $YetaWF.getElementsBySelector(".yt_panels_pagebarinfo_list .t_entry", [_this.Control]);
                for (var _i = 0, entries_1 = entries; _i < entries_1.length; _i++) {
                    var e = entries_1[_i];
                    $YetaWF.elementRemoveClassList(e, _this.Setup.ActiveCss);
                }
                $YetaWF.elementAddClassList(entry, _this.Setup.ActiveCss);
                return true;
            });
            return _this;
        }
        PageBarInfoComponent.prototype.resize = function () {
            if (!this.Setup.Resize)
                return;
            // Resize the page bar in height so we fill the remaining page height
            // While this is possible in css also, it can't be done without knowing the structure of the page, which we can't assume in this page bar
            // so we just do it at load time (and when the window is resized).
            var winHeight = window.innerHeight;
            var docRect = document.body.getBoundingClientRect();
            var h = docRect.height - winHeight;
            var ctrlRect = this.Control.getBoundingClientRect();
            this.Control.style.height = ctrlRect.height - h + "px";
        };
        PageBarInfoComponent.TEMPLATE = "yt_panels_pagebarinfo";
        PageBarInfoComponent.SELECTOR = ".yt_panels_pagebarinfo.t_display";
        return PageBarInfoComponent;
    }(YetaWF.ComponentBaseDataImpl));
    YetaWF_Panels.PageBarInfoComponent = PageBarInfoComponent;
    $(window).smartresize(function () {
        var ctrlDivs = $YetaWF.getElementsBySelector(PageBarInfoComponent.SELECTOR);
        for (var _i = 0, ctrlDivs_1 = ctrlDivs; _i < ctrlDivs_1.length; _i++) {
            var ctrlDiv = ctrlDivs_1[_i];
            var mod = PageBarInfoComponent.getControlFromTag(ctrlDiv, PageBarInfoComponent.SELECTOR);
            mod.resize();
        }
    });
})(YetaWF_Panels || (YetaWF_Panels = {}));

//# sourceMappingURL=PageBarInfo.js.map
