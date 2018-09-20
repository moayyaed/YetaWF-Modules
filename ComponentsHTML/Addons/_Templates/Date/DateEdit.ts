﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

namespace YetaWF_ComponentsHTML {

    export interface IPackageVolatiles {
        DateFormat: string;
    }

    interface DateEditSetup {
        Min: Date;
        Max: Date;
    }

    export class DateEditComponent extends YetaWF.ComponentBase<HTMLInputElement> {

        public static readonly SELECTOR: string = ".yt_date.t_edit";

        Hidden: HTMLInputElement;
        Date: HTMLInputElement;
        kendoDatePicker: kendo.ui.DatePicker;

        constructor(controlId: string, setup: DateEditSetup) {
            super(controlId);

            $YetaWF.addObjectDataById(controlId, this);

            this.Hidden = $YetaWF.getElement1BySelector("input[type=\"hidden\"]", [this.Control]) as HTMLInputElement;
            this.Date = $YetaWF.getElement1BySelector("input[name=\"dtpicker\"]", [this.Control]) as HTMLInputElement;

            $(this.Date).kendoDatePicker({
                animation: false,
                format: YVolatile.YetaWF_ComponentsHTML.DateFormat,
                min: setup.Min, max: setup.Max,
                culture: YVolatile.Basics.Language,
                change: (ev: kendo.ui.DatePickerEvent): void => {
                    var kdPicker: kendo.ui.DatePicker = ev.sender;
                    var val: Date = kdPicker.value();
                    if (val == null)
                        this.setHiddenText(kdPicker.element.val() as string);
                    else
                        this.setHidden(val);
                    FormsSupport.validateElement(this.Hidden);
                }
            });
            this.kendoDatePicker = $(this.Date).data("kendoDatePicker");
            this.setHidden(this.kendoDatePicker.value());

            this.Date.addEventListener("change", (event: Event): void => {
                var val: Date = this.kendoDatePicker.value();
                if (val == null)
                    this.setHiddenText((event.target as HTMLInputElement).value);
                else
                    this.setHidden(val);
                FormsSupport.validateElement(this.Hidden);
            }, false);
        }
        private setHidden(dateVal: Date | null): void {
            var s: string = "";
            if (dateVal != null)
                s = dateVal.toUTCString();
            this.Hidden.setAttribute("value", s);
        }
        private setHiddenText(dateVal: string | null): void {
            this.Hidden.setAttribute("value", dateVal ? dateVal : "");
        }
        get value(): Date {
            return new Date(this.Hidden.value);
        }
        get valueText(): string {
            return this.Hidden.value;
        }
        set value(val: Date) {
            this.setHidden(val);
            this.kendoDatePicker.value(val);
        }
        public clear(): void {
            this.setHiddenText("");
            this.kendoDatePicker.value("");
        }
        public enable(enabled: boolean): void {
            this.kendoDatePicker.enable(enabled);
        }
        public destroy(): void {
            this.kendoDatePicker.destroy();
            $YetaWF.removeObjectDataById(this.Control.id);
        }

        public static getControlFromTag(elem: HTMLElement): DateEditComponent { return super.getControlBaseFromTag<DateEditComponent>(elem, DateEditComponent.SELECTOR); }
        public static getControlFromSelector(selector: string, tags: HTMLElement[]): DateEditComponent { return super.getControlBaseFromSelector<DateEditComponent>(selector, DateEditComponent.SELECTOR, tags); }
        public static getControlById(id: string): DateEditComponent { return super.getControlBaseById<DateEditComponent>(id, DateEditComponent.SELECTOR); }
    }

    // A <div> is being emptied. Destroy all controls the <div> may contain.
    $YetaWF.registerClearDiv((tag: HTMLElement): void => {
        YetaWF.ComponentBase.clearDiv<DateEditComponent>(tag, DateEditComponent.SELECTOR, (control: DateEditComponent): void => {
            control.destroy();
        });
    });
}
