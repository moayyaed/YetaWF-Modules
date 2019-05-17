/* Copyright © 2019 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

// copied from @types/masonry-layout (original has exports= which doesn't work, check future updates)
declare class Masonry {
    constructor(options?: Masonry.Options);
    constructor(selector: string | Element, options?: Masonry.Options);

    masonry?(): void;
    masonry?(eventName: string, listener: any): void;

    // layout
    layout?(): void;
    layoutItems?(items: any[], isStill?: boolean): void;
    stamp?(elements: any[]): void;
    unstamp?(elements: any[]): void;

    // add and remove items
    appended?(elements: any[]): void;
    prepended?(elements: any[]): void;
    addItems?(elements: any[]): void;
    remove?(elements: any[]): void;

    // events
    on?(eventName: string, listener: any): void;
    off?(eventName: string, listener: any): void;
    once?(eventName: string, listener: any): void;

    // utilities
    reloadItems?(): void;
    destroy?(): void;
    getItemElements?(): any[];
    data?(element: Element): Masonry;
}

declare namespace Masonry {
    interface Options {
        // layout
        itemSelector?: string;
        columnWidth?: any;
        percentPosition?: boolean;
        gutter?: any;
        stamp?: string;
        fitWidth?: boolean;
        originLeft?: boolean;
        originTop?: boolean;
        horizontalOrder?: boolean;

        // setup
        containerStyle?: {};
        transitionDuration?: any;
        resize?: boolean;
        initLayout?: boolean;
    }
}

namespace YetaWF_ComponentsHTML {

    interface ControlData {
        Id: string; // id of the property list div
        Controls: string[];
        Dependents: Dependent[];
    }
    interface Dependent {
        Prop: string; // Name of property
        Disable: boolean; // defines whether the control is disabled instead of hidden

        ProcessValues: ValueEntry[];
        HideValues: ValueEntry[];
    }
    interface ValueEntry {
        ControlProp: string; // name of controlling property (ProcIf)
        ValueType: ValueTypeEnum;
        ValueObject: any;
    }
    enum ValueTypeEnum {
        EqualIntValue = 0,
        EqualStringValue = 1,
        NotEqualIntValue = 10,
        NotEqualStringValue = 11,
        EqualNull = 100,
        EqualNonNull = 101,
    }

    enum ValidityEnum {
        ControllingNotShown = 0,
        Valid = 1,
        Invalid = 2,
    }

    interface ControlItem {
        Name: string;
        ControlType: ControlTypeEnum;
        Object: DropDownListEditComponent | HTMLInputElement | HTMLSelectElement;
    }
    enum ControlTypeEnum {
        Input = 0,
        Select = 1,
        KendoSelect = 2,
        Hidden = 3,
    }
    interface PropertyListSetup {
        Style: PropertyListStyleEnum;
        ColumnStyles: PropertyListColumnDef[];
        ExpandableList: string[];
        InitialExpanded: boolean;
    }
    enum PropertyListStyleEnum {
        Tabbed = 0,
        Boxed = 1,
        BoxedWithCategories = 2,
    }
    interface PropertyListColumnDef {
        MinWindowSize: number;
        Columns: number;
    }

    export class PropertyListComponent extends YetaWF.ComponentBaseImpl {

        private ControlData: ControlData | null;
        private ControllingControls: ControlItem[] = [];
        private Setup: PropertyListSetup;
        private MasonryElem: Masonry | null = null;
        private MinWidth: number = 0;
        private CurrWidth: number = 0;
        private ColumnDefIndex: number = -1;

        constructor(controlId: string, setup: PropertyListSetup, controlData: ControlData) {
            super(controlId);

            this.ControlData = controlData;
            this.Setup = setup;

            // column handling
            if (this.Setup.Style === PropertyListStyleEnum.Boxed || this.Setup.Style === PropertyListStyleEnum.BoxedWithCategories) {
                this.MinWidth = this.Setup.ColumnStyles.length > 0 ? this.Setup.ColumnStyles[0].MinWindowSize : 0;
                if (this.Setup.InitialExpanded) {
                    let box = $YetaWF.getElement1BySelector(".t_propexpanded", [this.Control]);
                    this.expandBox(box);
                } else {
                    this.ColumnDefIndex = this.getColumnDefIndex();
                    if (this.ColumnDefIndex >= 0)
                        this.MasonryElem = this.createMasonry();
                }
                setInterval(() => {
                    if (this.MasonryElem)
                        this.MasonryElem.layout!();
                }, 1000);
            }

            // expand/collapse handling
            if (this.Setup.Style === PropertyListStyleEnum.Boxed || this.Setup.Style === PropertyListStyleEnum.BoxedWithCategories) {
                $YetaWF.registerEventHandler(this.Control, "click", ".t_boxexpcoll", (ev: Event): boolean => {
                    this.expandCollapseBox($YetaWF.elementClosest(ev.__YetaWFElem, ".t_proptable"));
                    return false;
                });
            }

            // Handle change events
            if (this.ControlData) {
            var controlData = this.ControlData;
                for (let control of controlData.Controls) {
                    var controlItem = this.getControlItem(control);
                    this.ControllingControls.push(controlItem);
                    switch (controlItem.ControlType) {
                        case ControlTypeEnum.Input:
                            $YetaWF.registerMultipleEventHandlers([(controlItem.Object as HTMLInputElement)], ["change", "input"], null, (ev: Event): boolean => {
                                this.update();
                                return false;
                            });
                            break;
                        case ControlTypeEnum.Select:
                            $YetaWF.registerEventHandler((controlItem.Object as HTMLSelectElement), "change", null, (ev: Event): boolean => {
                                this.update();
                                return false;
                            });
                            break;
                        case ControlTypeEnum.KendoSelect:
                            $YetaWF.registerCustomEventHandler(controlItem.Object as DropDownListEditComponent, "dropdownlist_change", (evt: Event): void => {
                                this.update();
                            });
                            break;
                        case ControlTypeEnum.Hidden:
                            break;
                    }
                }
            }

            // Initialize initial form
            this.update();

            $YetaWF.registerEventHandlerWindow("resize", null, (ev: UIEvent) => {
                if (this.MasonryElem) {
                    this.setLayout();
                    if (this.MasonryElem)
                        this.MasonryElem.layout!();
                }
                return true;
            });

            $YetaWF.registerCustomEventHandler(this, "propertylist_relayout", (ev: Event): boolean => {
                this.setLayout();
                if (this.MasonryElem)
                    this.MasonryElem.layout!();
                return false;
            });
            /**
             * Collapse whichever box is expanded
             */
            $YetaWF.registerCustomEventHandler(this, "propertylist_collapse", (ev: Event): boolean => {
                this.setLayout();
                let box = $YetaWF.getElement1BySelectorCond(".t_propexpanded", [this.Control]);
                if (box) {
                    this.collapseBox(box);
                }
                if (this.MasonryElem)
                    this.MasonryElem.layout!();
                return false;
            });
        }

        private setLayout(): void {
            if (window.innerWidth < this.MinWidth) {
                this.destroyMasonry();
            } else if (!this.MasonryElem || window.innerWidth !== this.CurrWidth) {
                let newIndex = this.getColumnDefIndex();
                if (this.ColumnDefIndex !== newIndex) {
                    this.destroyMasonry();
                    this.MasonryElem = this.createMasonry();
                }
            }
        }

        private expandCollapseBox(box: HTMLElement): void {

            this.destroyMasonry();

            if (!$YetaWF.elementHasClass(box, "t_propexpandable"))
                return;

            if ($YetaWF.elementHasClass(box, "t_propexpanded")) {
                // box can collapse
                this.collapseBox(box);

                this.MasonryElem = this.createMasonry();
            } else {
                // box can expand
                this.expandBox(box);
            }
        }
        private collapseBox(box: HTMLElement): void {
            let boxes = $YetaWF.getElementsBySelector(".t_proptable", [this.Control]);
            for (let b of boxes) {
                $YetaWF.elementRemoveClasses(b, "t_propexpanded t_propcollapsed t_prophide");
                $YetaWF.elementAddClass(b, "t_propcollapsed");
            }
            // show apply/save/cancel buttons again
            this.toggleFormButtons(true);
        }
        private expandBox(box: HTMLElement): void {
            let boxes = $YetaWF.getElementsBySelector(".t_proptable", [this.Control]);
            for (let b of boxes) {
                $YetaWF.elementRemoveClasses(b, "t_propexpanded t_propcollapsed");
                if (b !== box)
                    $YetaWF.elementAddClass(b, "t_prophide");
            }
            $YetaWF.elementAddClass(box, "t_propexpanded");
            // hide apply/save/cancel buttons while expanded
            this.toggleFormButtons(false);
        }

        private toggleFormButtons(show: boolean): void {
            let form = $YetaWF.Forms.getForm(this.Control);
            // make the form submit/nosubmit
            $YetaWF.elementRemoveClass(form, YConfigs.Forms.CssFormNoSubmit);
            if (!show)
                $YetaWF.elementAddClass(form, YConfigs.Forms.CssFormNoSubmit);
            // show/hide buttons
            let buttonList = $YetaWF.getElementsBySelector(".t_detailsbuttons", [form]);
            for (let buttons of buttonList)
                buttons.style.display = show ? "block" : "none";
        }

        private createMasonry(): Masonry {
            this.CurrWidth = window.innerWidth;
            this.ColumnDefIndex = this.getColumnDefIndex();
            let cols = this.Setup.ColumnStyles[this.ColumnDefIndex].Columns;
            $YetaWF.elementAddClass(this.Control, `t_col${cols}`);
            return new Masonry(this.Control, {
                itemSelector: ".t_proptable",
                horizontalOrder: true,
                transitionDuration: "0.1s",
                resize: false,
                initLayout: true
                //columnWidth: 200
            });
        }
        private destroyMasonry(): void {
            this.CurrWidth = 0;
            this.ColumnDefIndex = -1;
            if (this.MasonryElem) {
                this.MasonryElem.destroy!();
                this.MasonryElem = null;
                $YetaWF.elementRemoveClass(this.Control, "t_col1");
                $YetaWF.elementRemoveClass(this.Control, "t_col2");
                $YetaWF.elementRemoveClass(this.Control, "t_col3");
                $YetaWF.elementRemoveClass(this.Control, "t_col4");
                $YetaWF.elementRemoveClass(this.Control, "t_col5");
            }
        }
        private getColumnDefIndex(): number {
            let width = window.innerWidth;
            let index = -1;
            for (let style of this.Setup.ColumnStyles) {
                if (width < style.MinWindowSize)
                     return index;
                 ++index;
            }
            return index;
        }


        private getControlItem(control: string): ControlItem {
            let elemSel = $YetaWF.getElement1BySelectorCond(`.t_row.t_${control.toLowerCase()} select[name$='${control}']`, [this.Control]) as HTMLSelectElement | null;
            if (elemSel) {
                var kendoSelect = YetaWF.ComponentBaseDataImpl.getControlFromTagCond<DropDownListEditComponent>(elemSel, DropDownListEditComponent.SELECTOR);
                if (kendoSelect) {
                    // Kendo
                    return { Name: control, ControlType: ControlTypeEnum.KendoSelect, Object: kendoSelect };
                } else {
                    // Native
                    return { Name: control, ControlType: ControlTypeEnum.Select, Object: elemSel };
                }
            }
            let elemInp = $YetaWF.getElement1BySelectorCond(`.t_row.t_${control.toLowerCase()} input[name$='${control}']`, [this.Control]) as HTMLInputElement | null;
            if (elemInp) {
                return { Name: control, ControlType: ControlTypeEnum.Input, Object: elemInp };
            }
            let elemHid = $YetaWF.getElement1BySelectorCond(`input[name$='${control}'][type='hidden']`, [this.Control]) as HTMLInputElement | null;
            if (elemHid) {
                return { Name: control, ControlType: ControlTypeEnum.Hidden, Object: elemHid };
            }
            throw `No control found for ${control}`;
        }

        /**
         * Update all dependent fields.
         */
        private update(): void {

            if (!this.ControlData) return;

            // for each dependent, verify that all its conditions are true
            var deps = this.ControlData.Dependents;
            for (let dep of deps) {

                var depRow = $YetaWF.getElement1BySelectorCond(`.t_row.t_${dep.Prop.toLowerCase()}`, [this.Control]);// the propertylist row affected
                if (!depRow)
                    continue;

                var hidden = false;
                for (let value of dep.HideValues) {// hidden hides only, it never makes it visible (use process for that instead)
                    var validity = this.getValidity(dep, value);
                    switch (validity) {
                        case ValidityEnum.ControllingNotShown:
                        case ValidityEnum.Valid:
                            this.toggle(dep, depRow, false);
                            hidden = true;
                            break;
                        default:
                            break;
                    }
                }
                if (!hidden) {
                    var valid: boolean = false;
                    for (let value of dep.ProcessValues) {
                        var validity = this.getValidity(dep, value);
                        if (validity === ValidityEnum.Valid) {
                            valid = true;
                            break;
                        }
                    }
                    this.toggle(dep, depRow, valid);
                }
            }
        }
        private toggle(dep: Dependent, depRow: HTMLElement, valid: boolean): void {
            if (dep.Disable) {
                $YetaWF.elementAndChildrenEnableToggle(depRow, valid);
            } else {
                if (valid) {
                    depRow.style.display = "";
                    $YetaWF.processActivateDivs([depRow]);// init any controls that just became visible
                } else
                    depRow.style.display = "none";
            }
            var affected = $YetaWF.getElementsBySelector("input,select,textarea", [depRow]);
            if (valid) {
                for (let e of affected)
                    $YetaWF.elementRemoveClass(e, YConfigs.Forms.CssFormNoValidate);
            } else {
                for (let e of affected)
                    $YetaWF.elementAddClass(e, YConfigs.Forms.CssFormNoValidate);
            }
        }

        private getValidity(dep: Dependent, value: ValueEntry): ValidityEnum {
            var valid = false; // we assume not valid unless we find a matching entry
            // get the controlling control's value
            var ctrlIndex = this.ControlData!.Controls.indexOf(value.ControlProp);
            if (ctrlIndex < 0)
                throw `Dependent ${dep.Prop} references controlling control ${value.ControlProp} which doesn't exist`;
            var controlItem = this.ControllingControls[ctrlIndex];

            var controlValue;
            switch (controlItem.ControlType) {
                case ControlTypeEnum.Input: {
                    let inputElem = controlItem.Object as HTMLInputElement;
                    let controlRow = $YetaWF.elementClosest(inputElem, ".t_row");
                    if (controlRow.style.display === "") {
                        if (inputElem.type.toLowerCase() === "checkbox") {
                            controlValue = inputElem.checked ? "1" : "0";
                        } else {
                            controlValue = inputElem.value;
                        }
                        valid = true;
                    }
                    break;
                 }
                case ControlTypeEnum.Select: {
                    let selectElem = controlItem.Object as HTMLSelectElement;
                    let controlRow = $YetaWF.elementClosest(selectElem, ".t_row");
                    if (controlRow.style.display === "") {
                        controlValue = selectElem.value;
                        valid = true;
                    }
                    break;
                 }
                case ControlTypeEnum.KendoSelect: {
                    let dropdownList = controlItem.Object as DropDownListEditComponent;
                    let controlRow = $YetaWF.elementClosest(dropdownList.Control, ".t_row");
                    if (controlRow.style.display === "") {
                        controlValue = dropdownList.value;
                        valid = true;
                    }
                    break;
                 }
                case ControlTypeEnum.Hidden: {
                    let hidden = controlItem.Object as DropDownListEditComponent;
                    controlValue = hidden.value;
                    switch (value.ValueType) {
                        case ValueTypeEnum.EqualIntValue:
                        case ValueTypeEnum.NotEqualIntValue:
                            if (controlValue === "True")
                                controlValue = true;
                            else if (controlValue === "False")
                                controlValue = false;
                            break;
                        default:
                            break;
                    }
                    valid = true;
                    break;
                 }
            }
            if (!valid)
                return ValidityEnum.ControllingNotShown;

            if (valid) {
                // test condition
                switch (value.ValueType) {
                    case ValueTypeEnum.EqualIntValue:
                        // need one matching value
                        var intValues = value.ValueObject as number[];
                        var found = false;
                        for (let intValue of intValues) {
                            if (intValue === Number(controlValue)) {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            valid = false;
                        break;
                    case ValueTypeEnum.EqualStringValue:
                        // need one matching value
                        var strValues = value.ValueObject as string[];
                        var found = false;
                        for (let strValue of strValues) {
                            if (strValue === controlValue) {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            valid = false;
                        break;
                    case ValueTypeEnum.NotEqualIntValue:
                        // need one matching value
                        var intValues = value.ValueObject as number[];
                        var found = false;
                        for (let intValue of intValues) {
                            if (intValue !== Number(controlValue)) {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            valid = false;
                        break;
                    case ValueTypeEnum.NotEqualStringValue:
                        // need one matching value
                        var strValues = value.ValueObject as string[];
                        var found = false;
                        for (let strValue of strValues) {
                            if (strValue !== controlValue) {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            valid = false;
                        break;
                    case ValueTypeEnum.EqualNonNull:
                        if (!controlValue || controlValue.length === 0)
                            valid = false;
                        break;
                    case ValueTypeEnum.EqualNull:
                        if (controlValue)
                            valid = false;
                        break;
                }
            }
            return valid ? ValidityEnum.Valid : ValidityEnum.Invalid;
        }

        public static isRowVisible(tag: HTMLElement): boolean {
            var row = $YetaWF.elementClosestCond(tag, ".t_row");
            if (!row) return false;
            return row.style.display === "";
        }

        public static relayout(container:HTMLElement): void {
            let ctrls = $YetaWF.getElementsBySelector(".yt_propertylistboxedcat,.yt_propertylistboxed", [container]);
            for (let ctrl of ctrls) {
                var event = document.createEvent("Event");
                event.initEvent("propertylist_collapse", false, true);
                ctrl.dispatchEvent(event);
            }
        }

        public static tabInitjQuery(tabCtrlId: string, activeTab: number, activeTabId: string):void {
            ComponentsHTMLHelper.MUSTHAVE_JQUERYUI();
            var tabCtrl = $YetaWF.getElementById(tabCtrlId);
            $YetaWF.elementAddClass(tabCtrl, "t_jquery");
            $(tabCtrl).tabs({ //jQuery-ui use
                active: activeTab,
                activate: (ev: Event, ui: JQueryUI.TabsActivationUIParams): void => {
                    if (ui.newPanel !== undefined) {
                        $YetaWF.processActivateDivs([ui.newPanel[0]]);
                        $YetaWF.processPanelSwitched(ui.newPanel[0]);
                        if (activeTabId) {
                            $(`#${activeTabId}`).val( (ui.newTab.length > 0) ?  Number(ui.newTab.attr("data-tab")) : -1);
                        }
                    }
                }
            });
        }
        public static tabInitKendo(tabCtrlId: string, activeTab: number, activeTabId: string):void {
            // mark the active tab with .k-state-active before initializing the tabstrip
            var tabs = $YetaWF.getElementsBySelector(`#${tabCtrlId}>ul>li`);
            for (let tab of tabs) {
                $YetaWF.elementRemoveClass(tab, "k-state-active");
            }
            $YetaWF.elementAddClass(tabs[activeTab], "k-state-active");

            // init tab control
            var tabCtrl = $YetaWF.getElementById(tabCtrlId);
            $YetaWF.elementAddClass(tabCtrl, "t_kendo");
            $(tabCtrl).kendoTabStrip({
                animation: false,
                activate: (ev: kendo.ui.TabStripActivateEvent): void  => {
                    if (ev.contentElement !== undefined) {
                        $YetaWF.processActivateDivs([ev.contentElement as HTMLElement]);
                        $YetaWF.processPanelSwitched(ev.contentElement as HTMLElement);
                        if (activeTabId)
                            $(`#${activeTabId}`).val( $(ev.item as HTMLElement).attr("data-tab") as string );
                    }
                }
            }).data("kendoTabStrip");
        }
    }

    $YetaWF.registerClearDiv((tag: HTMLElement): void => {
        var list = $YetaWF.getElementsBySelector(".yt_propertylisttabbed.t_jquery", [tag]);
        for (let el of list) {
            var tabsJq = $(el);
            if (!tabsJq) throw "No jquery ui object found";/*DEBUG*/
            tabsJq.tabs("destroy");
        }
        list = $YetaWF.getElementsBySelector(".yt_propertylisttabbed.t_kendo", [tag]);
        for (let el of list) {
            var tabsKn = $(el).data("kendoTabStrip");
            if (!tabsKn) throw "No kendo object found";/*DEBUG*/
            tabsKn.destroy();
        }
    });

    // The property list needs a bit of special love when it's made visible. Because panels have no width/height
    // while the propertylist is not visible (jquery implementation), when a propertylist is made visible using show(),
    // the default panel is not sized correctly. If you explicitly show() a propertylist that has never been visible,
    // call the following to cause the propertylist to be resized correctly:
    // ComponentsHTML.processPropertyListVisible(div);
    // div is any HTML element - all items (including child items) are checked for propertylists.

    ComponentsHTMLHelper.registerPropertyListVisible((tag: HTMLElement): void => {
        // jquery tabs
        var tabsJq = $YetaWF.getElementsBySelector(".ui-tabs", [tag]);
        for (let tabJq of tabsJq) {
            var id = tabJq.id;
            if (id === undefined) throw "No id on tab control";/*DEBUG*/
            var tabidJq = Number($(tabJq).tabs("option", "active"));
            if (tabidJq >= 0) {
                var panel = $YetaWF.getElement1BySelector(`#${id}_tab${tabidJq}`, [tabJq]);
                $YetaWF.processActivateDivs([panel]);
                $YetaWF.processPanelSwitched(panel);
            }
        }
        // kendo tabs
        var tabsKn = $YetaWF.getElementsBySelector(".k-widget.k-tabstrip", [tag]);
        for (let tabKn of tabsKn) {
            var id = tabKn.id;
            if (id === undefined) throw "No id on tab control";/*DEBUG*/
            var ts = $(tabKn).data("kendoTabStrip");
            var tabidKn = Number(ts.select().attr("data-tab"));
            if (tabidKn >= 0) {
                var panel = $YetaWF.getElement1BySelector(`#${id}-tab${+tabidKn + 1}`, [tabKn]);
                $YetaWF.processActivateDivs([panel]);
                $YetaWF.processPanelSwitched(panel);
            }
        }
    });
}

