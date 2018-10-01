"use strict";
/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var YetaWF_ComponentsHTML;
(function (YetaWF_ComponentsHTML) {
    var UrlTypeEnum;
    (function (UrlTypeEnum) {
        UrlTypeEnum[UrlTypeEnum["Local"] = 1] = "Local";
        UrlTypeEnum[UrlTypeEnum["Remote"] = 2] = "Remote";
    })(UrlTypeEnum || (UrlTypeEnum = {}));
    var UrlEditComponent = /** @class */ (function (_super) {
        __extends(UrlEditComponent, _super);
        function UrlEditComponent(controlId, setup) {
            var _this = _super.call(this, controlId) || this;
            _this.selectPage = null;
            _this.inputUrl = null;
            _this.divLocal = null;
            _this.divRemote = null;
            _this.Setup = setup;
            $YetaWF.addObjectDataById(controlId, _this);
            _this.inputHidden = $YetaWF.getElement1BySelector(".t_hidden", [_this.Control]);
            _this.selectType = YetaWF_ComponentsHTML.DropDownListEditComponent.getControlFromSelector("select.yt_urltype", [_this.Control]);
            // tslint:disable-next-line:no-bitwise
            if (_this.Setup.Type & UrlTypeEnum.Local) {
                _this.selectPage = YetaWF_ComponentsHTML.DropDownListEditComponent.getControlFromSelector("select.yt_urldesignedpage", [_this.Control]);
                _this.divLocal = $YetaWF.getElement1BySelector(".t_local", [_this.Control]);
            }
            // tslint:disable-next-line:no-bitwise
            if (_this.Setup.Type & UrlTypeEnum.Remote) {
                _this.inputUrl = $YetaWF.getElement1BySelector(".yt_urlremotepage", [_this.Control]);
                _this.divRemote = $YetaWF.getElement1BySelector(".t_remote", [_this.Control]);
            }
            _this.aLink = $YetaWF.getElement1BySelector(".t_link a", [_this.Control]);
            _this.updateStatus();
            _this.selectType.Control.addEventListener("dropdownlist_change", function (evt) {
                _this.updateStatus();
            });
            if (_this.selectPage) {
                _this.selectPage.Control.addEventListener("dropdownlist_change", function (evt) {
                    _this.updateStatus();
                });
            }
            if (_this.inputUrl) {
                $YetaWF.registerMultipleEventHandlers(_this.inputUrl, ["input", "change", "click", "keyup", "paste"], null, function (ev) { _this.updateStatus(); return true; });
            }
            return _this;
        }
        UrlEditComponent.prototype.updateStatus = function () {
            var tp = Number(this.selectType.value);
            var oldValue = this.inputHidden.value;
            switch (tp) {
                case UrlTypeEnum.Local:
                    if (this.divLocal)
                        this.divLocal.style.display = "";
                    if (this.divRemote)
                        this.divRemote.style.display = "none";
                    if (this.selectPage)
                        this.inputHidden.value = this.selectPage.Control.value.trim();
                    break;
                case UrlTypeEnum.Remote:
                    if (this.divLocal)
                        this.divLocal.style.display = "none";
                    if (this.divRemote)
                        this.divRemote.style.display = "";
                    if (this.inputUrl)
                        this.inputHidden.value = this.inputUrl.value.trim();
                    break;
            }
            var url = this.inputHidden.value.trim();
            if (url && url.length > 0) {
                if (tp === UrlTypeEnum.Local) {
                    var uri = $YetaWF.parseUrl(url);
                    uri.removeSearch(YConfigs.Basics.Link_NoEditMode);
                    uri.addSearch(YConfigs.Basics.Link_NoEditMode, "y");
                    this.aLink.href = uri.toUrl();
                }
                else {
                    this.aLink.href = url;
                }
                this.aLink.style.display = "";
            }
            else {
                this.aLink.href = "";
                this.aLink.style.display = "none";
            }
            if (oldValue !== url) {
                var event = document.createEvent("Event");
                event.initEvent("url_change", true, true);
                this.Control.dispatchEvent(event);
            }
        };
        Object.defineProperty(UrlEditComponent.prototype, "value", {
            // API
            get: function () {
                return this.inputHidden.value;
            },
            set: function (url) {
                var sel = Number(this.selectType.value); // current selection
                if (this.Setup.Type === UrlTypeEnum.Local + UrlTypeEnum.Remote && this.selectPage) {
                    if (url != null && (url.startsWith("//") || url.startsWith("http"))) {
                        // remote
                        if (this.inputUrl)
                            sel = UrlTypeEnum.Remote;
                    }
                    else {
                        // try local
                        this.selectPage.value = url;
                        if (this.selectPage.value !== url)
                            sel = UrlTypeEnum.Remote; // have to use remote as there was no match in the designed pages
                        else
                            sel = UrlTypeEnum.Local;
                    }
                }
                else if (this.Setup.Type === UrlTypeEnum.Local && this.selectPage) {
                    sel = UrlTypeEnum.Local;
                }
                else {
                    sel = UrlTypeEnum.Remote;
                }
                this.inputHidden.value = url;
                this.selectType.value = sel.toString();
                if (sel === UrlTypeEnum.Local && this.selectPage) {
                    this.selectPage.value = url;
                }
                else if (sel === UrlTypeEnum.Remote && this.inputUrl) {
                    this.inputUrl.value = url;
                }
                this.updateStatus();
            },
            enumerable: true,
            configurable: true
        });
        UrlEditComponent.prototype.clear = function () {
            this.value = "";
        };
        UrlEditComponent.prototype.enable = function (enabled) {
            this.selectType.enable(enabled);
            if (this.selectPage)
                this.selectPage.enable(enabled);
            if (this.inputUrl)
                $YetaWF.elementEnableToggle(this.inputUrl, enabled);
        };
        UrlEditComponent.getControlFromTag = function (elem) { return _super.getControlBaseFromTag.call(this, elem, UrlEditComponent.SELECTOR); };
        UrlEditComponent.getControlFromSelector = function (selector, tags) { return _super.getControlBaseFromSelector.call(this, selector || UrlEditComponent.SELECTOR, UrlEditComponent.SELECTOR, tags); };
        UrlEditComponent.getControlById = function (id) { return _super.getControlBaseById.call(this, id, UrlEditComponent.SELECTOR); };
        UrlEditComponent.prototype.destroy = function () {
            $YetaWF.removeObjectDataById(this.Control.id);
        };
        UrlEditComponent.SELECTOR = ".yt_url.t_edit";
        return UrlEditComponent;
    }(YetaWF.ComponentBase));
    YetaWF_ComponentsHTML.UrlEditComponent = UrlEditComponent;
    // A <div> is being emptied. Destroy all controls the <div> may contain.
    $YetaWF.registerClearDiv(function (tag) {
        YetaWF.ComponentBase.clearDiv(tag, UrlEditComponent.SELECTOR, function (control) {
            control.destroy();
        });
    });
})(YetaWF_ComponentsHTML || (YetaWF_ComponentsHTML = {}));

//# sourceMappingURL=UrlEdit.js.map