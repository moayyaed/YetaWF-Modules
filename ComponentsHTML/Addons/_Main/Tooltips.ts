﻿/* Copyright © 2018 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

declare var YetaWF_TemplateDropDownList: any;// %%%%%%%%%%%%%%%%%%% TODO: update once dropdown is ts

namespace YetaWF_ComponentsHTML {

    export class Tooltips {

        public static init(): void {

            const a1 = YVolatile.Basics.CssNoTooltips;
            const a2 = YConfigs.Basics.CssTooltip;
            const a3 = YConfigs.Basics.CssTooltipSpan;
            const selectors = `img:not("${a1}"),label,input:not(".ui-button-disabled"),a:not("${a1},.ui-button-disabled"),i,.ui-jqgrid span[${a2}],span[${a3}],li[${a2}],div[${a2}]`;
            const ddsel = ".k-list-container.k-popup li[data-offset-index]";
            $("body").tooltip({
                items: (selectors + "," + ddsel),
                content: function (a: any, b: any, c: any) : string|null {
                    var $this = $(this as HTMLElement);
                    if ($this.is(ddsel)) {
                        // dropdown list - find who owns this and get the matching tooltip
                        // this is a bit hairy - we save all the tooltips for a dropdown list in a variable
                        // named ..id.._tooltips. The popup/dropdown is named ..id..-list so we deduce the
                        // variable name from the popup/dropdown. This is going to break at some point...
                        const ttindex = $this.attr("data-offset-index");
                        if (ttindex === undefined) return null;
                        const $container = $this.closest(".k-list-container.k-popup");
                        if ($container.length !== 1) return null;
                        var id = $container.attr("id");
                        if (!id) return null;
                        id = id.replace("-list", "");
                        const tip = YetaWF_TemplateDropDownList.getTitleFromId(id, ttindex);
                        if (tip == null) return null;
                        return $YetaWF.htmlEscape(tip);
                    }
                    for (; ;) {
                        if (!$this.is(":hover") && $this.is(":focus"))
                            return null;
                        if ($this.attr("disabled") !== undefined)
                            return null;
                        var s: string | undefined = $this.attr(YConfigs.Basics.CssTooltip);
                        if (s)
                            return $YetaWF.htmlEscape(s);
                        s = $this.attr(YConfigs.Basics.CssTooltipSpan as string);
                        if (s)
                            return $YetaWF.htmlEscape(s);
                        s = $this.attr("title");
                        if (s !== undefined)
                            return $YetaWF.htmlEscape(s);
                        if ($this[0].tagName !== "IMG" && $this[0].tagName !== "I")
                            break;
                        // we're in an IMG or I tag, find enclosing A (if any) and try again
                        $this = $this.closest(`a:not("${YVolatile.Basics.CssNoTooltips}")`);
                        if ($this.length === 0) return null;
                        // if the a link is a menu, don't show a tooltip for the image because the tooltip would be in a bad location
                        if ($this.closest(".k-menu").length > 0) return null;
                    }
                    if ($this[0].tagName === "A") {
                        const href = ($this[0] as HTMLAnchorElement).href;
                        if (href === undefined || href.startsWith("javascript") || href.startsWith("#") || href.startsWith("mailto:"))
                            return null;
                        const target = ($this[0] as HTMLAnchorElement).target;
                        if (target === "_blank") {
                            const uri = $YetaWF.parseUrl(href);
                            return $YetaWF.htmlEscape(YLocs.Basics.OpenNewWindowTT.format(uri.getDomain()));
                        }
                    }
                    return null;
                },
                position: { my: "left top", at: "right bottom", collision: "flipfit" }
            });
        }
        public static removeTooltips(): void {
            $(".ui-tooltip").remove();
        }
    }
}

$(document).ready(() => {
    YetaWF_ComponentsHTML.Tooltips.init();
});

$("body").on("mousedown", "a", () => {
    // when we click on an <a> link, we don't want the next tooltip
    // this may be a bug because after clicking an a link, the tooltip will be created (again?) so we want to suppress this
    // Repro steps (without hack): right click on an a link (that COULD have a tooltip) and open a new tab/window. On return to this page we'll get a tooltip
    YetaWF_ComponentsHTML.Tooltips.removeTooltips();
});
