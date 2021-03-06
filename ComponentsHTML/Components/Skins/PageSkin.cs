﻿/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System.Threading.Tasks;
using YetaWF.Core.Addons;
using YetaWF.Core.Components;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Skins;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Controllers;

namespace YetaWF.Modules.ComponentsHTML.Components {

    /// <summary>
    /// Base class for the PageSkin component implementation.
    /// </summary>
    public abstract class PageSkinComponentBase : YetaWFComponent {

        internal const string TemplateName = "PageSkin";

        /// <summary>
        /// Returns the package implementing the component.
        /// </summary>
        /// <returns>Returns the package implementing the component.</returns>
        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        /// <summary>
        /// Returns the component name.
        /// </summary>
        /// <returns>Returns the component name.</returns>
        /// <remarks>Components in packages whose product name starts with "Component" use the exact name returned by GetTemplateName when used in UIHint attributes. These are considered core components.
        /// Components in other packages use the package's area name as a prefix. E.g., the UserId component in the YetaWF.Identity package is named "YetaWF_Identity_UserId" when used in UIHint attributes.
        ///
        /// The GetTemplateName method returns the component name without area name prefix in all cases.</remarks>
        public override string GetTemplateName() { return TemplateName; }
    }

    /// <summary>
    /// Displays the selected page skin information. The model defines the skin definition and cannot be null.
    /// </summary>
    /// <example>
    /// [Category("Skin"), Caption("Page Skin"), Description("The skin used to display the page")]
    /// [UIHint("PageSkin"), ReadOnly]
    /// public SkinDefinition SelectedSkin { get; set; }
    /// </example>
    public class PageSkinDisplayComponent : PageSkinComponentBase, IYetaWFComponent<SkinDefinition> {

        /// <summary>
        /// Returns the component type (edit/display).
        /// </summary>
        /// <returns>Returns the component type.</returns>
        public override ComponentType GetComponentType() { return ComponentType.Display; }

        internal class PageSkinUI {
            [Caption("Skin Collection"), Description("The name of the skin collection")]
            [StringLength(SkinDefinition.MaxCollection)]
            [UIHint("SkinCollection")]
            public string Collection { get; set; } // may be null for site default
            [Caption("Skin Name"), Description("The name of the skin")]
            [StringLength(SkinDefinition.MaxSkinFile)]
            [UIHint("SkinNamePage")]
            public string FileName { get; set; } // may be null for site default
            public string FileName_Collection { get { return Collection; } }
        }

        /// <summary>
        /// Called by the framework when the component needs to be rendered as HTML.
        /// </summary>
        /// <param name="model">The model being rendered by the component.</param>
        /// <returns>The component rendered as HTML.</returns>
        public async Task<string> RenderAsync(SkinDefinition model) {

            HtmlBuilder hb = new HtmlBuilder();

            PageSkinUI ps = new PageSkinUI {
                Collection = model.Collection,
                FileName = model.FileName,
            };

            using (Manager.StartNestedComponent(FieldName)) {

                hb.Append($@"
<div id='{ControlId}' class='yt_pageskin t_display'>
    <div class='t_collection'>
        {await HtmlHelper.ForLabelAsync(ps, nameof(ps.Collection))}
        {await HtmlHelper.ForDisplayAsync(ps, nameof(ps.Collection))}
    </div>
    <div class='t_skin'>
        {await HtmlHelper.ForLabelAsync(ps, nameof(ps.FileName))}
        {await HtmlHelper.ForDisplayAsync(ps, nameof(ps.FileName))}
    </div>
</div>");
            }
            return hb.ToString();
        }
    }

    /// <summary>
    /// Allows selection of a page skin from all the available skin collections.
    /// </summary>
    /// <example>
    /// [Category("Skin"), Caption("Page Skin"), Description("The skin used to display the page")]
    /// [UIHint("PageSkin"), Trim]
    /// public SkinDefinition SelectedSkin { get; set; }
    /// </example>
    [UsesAdditional("NoDefault", "bool", "false", "Defines whether a \"(Site Default)\" entry is automatically added as the first entry, with a value of null")]
    public class PageSkinEditComponent : PageSkinComponentBase, IYetaWFComponent<SkinDefinition> {

        /// <summary>
        /// Returns the component type (edit/display).
        /// </summary>
        /// <returns>Returns the component type.</returns>
        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        internal class PageSkinUI {
            [Caption("Skin Collection"), Description("The name of the skin collection")]
            [StringLength(SkinDefinition.MaxCollection)]
            [UIHint("SkinCollection")]
            public string Collection { get; set; } // may be null for site default
            [Caption("Skin Name"), Description("The name of the skin")]
            [StringLength(SkinDefinition.MaxSkinFile)]
            [UIHint("SkinNamePage")]
            public string FileName { get; set; } // may be null for site default
            public string FileName_Collection { get { return Collection; } }
        }
        internal class Setup {
            public string AjaxUrl { get; set; }
        }

        /// <summary>
        /// Called by the framework when the component needs to be rendered as HTML.
        /// </summary>
        /// <param name="model">The model being rendered by the component.</param>
        /// <returns>The component rendered as HTML.</returns>
        public async Task<string> RenderAsync(SkinDefinition model) {

            HtmlBuilder hb = new HtmlBuilder();

            PageSkinUI ps = new PageSkinUI {
                Collection = model.Collection,
                FileName = model.FileName,
            };

            Setup setup = new Setup {
                AjaxUrl = Utility.UrlFor(typeof(SkinController), nameof(SkinController.GetPageSkins)),
            };

            // add dummy input field so we can find the property name in this template
            hb.Append($@"
<div id='{ControlId}' class='yt_pageskin t_edit'>
     {await HtmlHelper.ForEditComponentAsync(Container, PropertyName, "-", "Hidden", HtmlAttributes: new { __NoTemplate = true, @class = Forms.CssFormNoSubmit })}");

            using (Manager.StartNestedComponent(FieldName)) {

                hb.Append($@"
    <div class='t_collection'>
        {await HtmlHelper.ForLabelAsync(ps, nameof(ps.Collection))}
        {await HtmlHelper.ForEditAsync(ps, nameof(ps.Collection))}
    </div>
    <div class='t_skin'>
        {await HtmlHelper.ForLabelAsync(ps, nameof(ps.FileName))}
        {await HtmlHelper.ForEditAsync(ps, nameof(ps.FileName))}
    </div>
</div>");

                Manager.ScriptManager.AddLast($@"new YetaWF_ComponentsHTML.PageSkinEditComponent('{ControlId}', {Utility.JsonSerialize(setup)});");

            }
            return hb.ToString();
        }
    }
}
