"use strict";
/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */
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
var YetaWF_ComponentsHTML;
(function (YetaWF_ComponentsHTML) {
    var PageSkinEditComponent = /** @class */ (function (_super) {
        __extends(PageSkinEditComponent, _super);
        function PageSkinEditComponent(controlId, setup) {
            var _this = _super.call(this, controlId, PageSkinEditComponent.TEMPLATE, PageSkinEditComponent.SELECTOR, {
                ControlType: YetaWF_ComponentsHTML.ControlTypeEnum.Template,
                ChangeEvent: null,
                GetValue: function (control) {
                    return control.SelectFile.value;
                },
                Enable: function (control, enable, clearOnDisable) {
                    control.enable(enable);
                },
            }) || this;
            _this.Setup = setup;
            _this.SelectCollection = YetaWF.ComponentBaseDataImpl.getControlFromSelector("select[name$='.Collection']", YetaWF_ComponentsHTML.DropDownListEditComponent.SELECTOR, [_this.Control]);
            _this.SelectFile = YetaWF.ComponentBaseDataImpl.getControlFromSelector("select[name$='.FileName']", YetaWF_ComponentsHTML.DropDownListEditComponent.SELECTOR, [_this.Control]);
            _this.SelectCollection.Control.addEventListener("dropdownlist_change", function (evt) {
                var data = { SkinCollection: _this.SelectCollection.value };
                _this.SelectFile.ajaxUpdate(data, _this.Setup.AjaxUrl);
            });
            return _this;
        }
        PageSkinEditComponent.prototype.enable = function (enabled) {
            this.SelectCollection.enable(enabled);
            this.SelectFile.enable(enabled);
        };
        PageSkinEditComponent.TEMPLATE = "yt_pageskin";
        PageSkinEditComponent.SELECTOR = ".yt_pageskin.t_edit";
        return PageSkinEditComponent;
    }(YetaWF.ComponentBaseDataImpl));
    YetaWF_ComponentsHTML.PageSkinEditComponent = PageSkinEditComponent;
})(YetaWF_ComponentsHTML || (YetaWF_ComponentsHTML = {}));

//# sourceMappingURL=PageSkin.js.map
