﻿/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Extensions;
using YetaWF.Core.Image;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Pages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Controllers;

namespace YetaWF.Modules.ComponentsHTML.Components {

    /// <summary>
    /// Base class for the Image component implementation.
    /// </summary>
    public abstract class ImageComponentBase : YetaWFComponent {

        internal static string __ResStr(string name, string defaultValue, params object[] parms) { return ResourceAccess.GetResourceString(typeof(ImageComponentBase), name, defaultValue, parms); }

        internal const string TemplateName = "Image";

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

        /// <summary>
        /// Renders an image &lt;img&gt; tag with specified attributes and returns HTML.
        /// </summary>
        /// <param name="imageType">The image type which must match a registered image handler. The YetaWF.Core.Image.ImageSupport.AddHandler method is used to register an image handler.</param>
        /// <param name="width">The width of the image. Both <paramref name="width"/> and <paramref name="height"/> may be 0, in which case no size attributes are rendered.</param>
        /// <param name="height">The height of the image. Both <paramref name="width"/> and <paramref name="height"/> may be 0, in which case no size attributes are rendered.</param>
        /// <param name="model">The model representing the image.</param>
        /// <param name="CacheBuster">A value that becomes part of the image URL, which can be used to defeat client-side caching. May be null.</param>
        /// <param name="Alt">The text that is rendered as part of the &lt;img&gt; tag's Alt attribute. May be null in which case the text "Image" is used.</param>
        /// <param name="ExternalUrl">Defines whether a full URL including domain is rendered (true) or whether just the path is used (false).</param>
        /// <param name="SecurityType">The security type of the rendered image URL.</param>
        /// <returns></returns>
        public static string RenderImage(string imageType, int width, int height, string model,
                string CacheBuster = null, string Alt = null, bool ExternalUrl = false, PageDefinition.PageSecurityType SecurityType = PageDefinition.PageSecurityType.Any) {
            string url = ImageHTML.FormatUrl(imageType, null, model, width, height, CacheBuster: CacheBuster, ExternalUrl: ExternalUrl, SecurityType: SecurityType);
            YTagBuilder img = new YTagBuilder("img");
            img.AddCssClass("t_preview");
            img.Attributes.Add("src", url);
            img.Attributes.Add("alt", Alt ?? __ResStr("altImg", "Image"));
            return img.ToString(YTagRenderMode.StartTag);
        }
        internal static async Task<string> RenderImageAttributesAsync(string model) {
            if (model == null) return "";
            System.Drawing.Size size = await ImageSupport.GetImageSizeAsync(model);
            if (size.IsEmpty) return "";
            return __ResStr("imgAttr", "{0} x {1} (w x h)", size.Width, size.Height);
        }
    }

    /// <summary>
    /// Displays the model as an image. The string specified as image value must have been created/saved using the Image (Edit) component or can be an absolute URL.
    /// </summary>
    /// <remarks>
    /// This component supports all image types implemented using the Image Http Handler. This is an extensible class that supports file and data based images.
    /// 
    /// The model can also specify an absolute URL in which case that URL is used to render the image (without Image Http Handler).
    /// </remarks>
    /// <example>
    /// [UIHint("Image"), ReadOnly, AdditionalMetadata("ImageType", VendorImageSupport.ImageType), AdditionalMetadata("Width", Vendor.LogoWidth), AdditionalMetadata("Height", Vendor.LogoHeight)]
    /// public string VendorLogo { get; set; }
    /// </example>
    [UsesAdditional("ImageType", "string", "null", "The image type implementing this image. Specify null if the model defines an absolute URL.")]
    [UsesAdditional("Width", "int", "0", "The width of the preview image. The aspect ratio of the original image is preserved. Specify 0 if the model defines an absolute URL.")]
    [UsesAdditional("Height", "int", "0", "The height of the preview image. The aspect ratio of the original image is preserved. Specify 0 if the model defines an absolute URL.")]
    [UsesAdditional("ShowMissing", "bool", "true", "Defines whether an image that is not defined displays a \"No Image\" placeholder (ShowMissing=true, default) or whether no image is displayed (ShowMissing=false). Ignored if the model defines an absolute URL.")]
    [UsesAdditional("LinkToImage", "bool", "false", "Defines whether clicking on the image preview will display the image on a new page in its original size (LinkToImage=true). Otherwise, clinking on the image has no effect (LinkToImage=false, default). Ignored if the model defines an absolute URL.")]
    public class ImageDisplayComponent : ImageComponentBase, IYetaWFComponent<string> {

        /// <summary>
        /// Returns the component type (edit/display).
        /// </summary>
        /// <returns>Returns the component type.</returns>
        public override ComponentType GetComponentType() { return ComponentType.Display; }

        /// <summary>
        /// Called by the framework when the component needs to be rendered as HTML.
        /// </summary>
        /// <param name="model">The model being rendered by the component.</param>
        /// <returns>The component rendered as HTML.</returns>
        public Task<string> RenderAsync(string model) {

            HtmlBuilder hb = new HtmlBuilder();
            hb.Append("<div class='yt_image t_display'>");

            string imageType = PropData.GetAdditionalAttributeValue<string>("ImageType", null);
            int width = PropData.GetAdditionalAttributeValue("Width", 0);
            int height = PropData.GetAdditionalAttributeValue("Height", 0);

            if (string.IsNullOrWhiteSpace(imageType) && model != null && (model.IsAbsoluteUrl() || model.StartsWith("/") || model.StartsWith("data:"))) {

                if (width != 0 || height != 0) throw new InternalError("Can't use Width or Height with external Urls");

                YTagBuilder img = new YTagBuilder("img");
                img.Attributes.Add("src", model);
                if (!img.Attributes.ContainsKey("alt"))
                    img.Attributes.Add("alt", __ResStr("altImage", "Image"));
                hb.Append(img.ToString(YTagRenderMode.Normal));

            } else {

                if (string.IsNullOrWhiteSpace(imageType)) throw new InternalError("No ImageType specified");

                bool showMissing = PropData.GetAdditionalAttributeValue("ShowMissing", true);
                if (string.IsNullOrWhiteSpace(model) && !showMissing)
                    return Task.FromResult<string>(null);

                string alt = null;
                if (HtmlAttributes.ContainsKey("alt"))
                    alt = (string)HtmlAttributes["alt"];
                string imgTag = ImageComponentBase.RenderImage(imageType, width, height, model, Alt: alt);

                bool linkToImage = PropData.GetAdditionalAttributeValue("LinkToImage", false);
                if (linkToImage) {
                    YTagBuilder link = new YTagBuilder("a");
                    string imgUrl = ImageHTML.FormatUrl(imageType, null, model);
                    link.MergeAttribute("href", imgUrl);
                    link.MergeAttribute("target", "_blank");
                    link.MergeAttribute("rel", "noopener noreferrer");
                    link.InnerHtml = imgTag;
                    hb.Append(link.ToString(YTagRenderMode.Normal));
                } else
                    hb.Append(imgTag);
            }

            hb.Append("</div>");
            return Task.FromResult(hb.ToString());
        }
    }

    /// <summary>
    /// Allows selection and uploading of an image.
    /// </summary>
    /// <remarks>
    /// This component supports all image types implemented using the Image Http Handler. This is an extensible class that supports file and data based images.
    /// </remarks>
    /// <example>
    /// [UIHint("Image"), AdditionalMetadata("ImageType", VendorImageSupport.ImageType), AdditionalMetadata("Width", Vendor.LogoWidth), AdditionalMetadata("Height", Vendor.LogoHeight)]
    /// public string VendorLogo { get; set; }
    /// </example>
    [UsesAdditional("ImageType", "string", "null", "The image type implementing this image. Specify null if the model defines an absolute URL.")]
    [UsesAdditional("Width", "int", "0", "The width of the preview image. The aspect ratio of the original image is preserved. Specify 0 if the model defines an absolute URL.")]
    [UsesAdditional("Height", "int", "0", "The height of the preview image. The aspect ratio of the original image is preserved. Specify 0 if the model defines an absolute URL.")]
    [UsesAdditional("ShowMissing", "bool", "true", "Defines whether an image that is not defined displays a \"No Image\" placeholder (ShowMissing=true, default) or whether no image is displayed (ShowMissing=false). Ignored if the model defines an absolute URL.")]
    [UsesAdditional("LinkToImage", "bool", "false", "Defines whether clicking on the image preview will display the image on a new page in its original size (LinkToImage=true). Otherwise, clinking on the image has no effect (LinkToImage=false, default). Ignored if the model defines an absolute URL.")]
    public class ImageEditComponent : ImageComponentBase, IYetaWFComponent<string> {

        /// <summary>
        /// Returns the component type (edit/display).
        /// </summary>
        /// <returns>Returns the component type.</returns>
        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        internal class ImageEditSetup {
            public string UploadId { get; set; }
        }

        /// <summary>
        /// Called by the framework when the component needs to be rendered as HTML.
        /// </summary>
        /// <param name="model">The model being rendered by the component.</param>
        /// <returns>The component rendered as HTML.</returns>
        public async Task<string> RenderAsync(string model) {

            // the upload control
            Core.Components.FileUpload1 setupUpload = new Core.Components.FileUpload1() {
                SaveURL = Utility.UrlFor(typeof(FileUpload1Controller), nameof(FileUpload1Controller.SaveImage), new { __ModuleGuid = Manager.CurrentModule.ModuleGuid }),
                RemoveURL = Utility.UrlFor(typeof(FileUpload1Controller), nameof(FileUpload1Controller.RemoveImage), new { __ModuleGuid = Manager.CurrentModule.ModuleGuid }),
            };

            string uploadId = ControlId + "_ul1";

            ImageEditSetup setup = new ImageEditSetup {
                UploadId = uploadId,
            };

            HtmlBuilder hb = new HtmlBuilder();
            hb.Append($@"
<div class='yt_image t_edit' id='{ControlId}'>
    {await HtmlHelper.ForEditComponentAsync(Container, PropertyName, model, "Hidden", HtmlAttributes: new { __NoTemplate = true }, Validation: true)}
    <div class='t_image'>
        {await HtmlHelper.ForDisplayComponentAsync(Container, PropertyName, model, TemplateName, HtmlAttributes: new { alt = __ResStr("imgAlt", "Preview Image") })}
    </div>
    <div class='t_info'>
        {await RenderImageAttributesAsync(model)}
    </div>
    <div class='t_haveimage' {(string.IsNullOrWhiteSpace(model) ? "style='display:none'" : "")}>
        <input type='button' class='t_clear' value='{__ResStr("btnClear", "Clear")}' title='{__ResStr("txtClear", "Click to clear the current image")}' />
    </div>
    {await HtmlHelper.ForEditContainerAsync(setupUpload, "FileUpload1", HtmlAttributes: new { id = uploadId })}
</div>");

            Manager.ScriptManager.AddLast($@"new YetaWF_ComponentsHTML.ImageEditComponent('{ControlId}', {Utility.JsonSerialize(setup)});");

            return hb.ToString();
        }
    }
}
