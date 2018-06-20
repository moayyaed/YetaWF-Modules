﻿using System.Threading.Tasks;
using YetaWF.Core.Packages;
using System.Collections.Generic;
using YetaWF.Core.Components;
using YetaWF.Core.Support;
using YetaWF.Core.Localize;
using System.Linq;
using System;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Models;
#if MVC6
#else
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
#endif

namespace YetaWF.Modules.ComponentsHTML.Components {

    public abstract class YetaWFComponent : YetaWFComponentBase {

        public enum FieldType {
            Normal, // with name, not validated
            Anonymous, // no name - no validation
            Validated, // with name, validated
        }
        public class GridModel {
            [UIHint("Grid")]
            public GridDefinition GridDef { get; set; }
        }

        /// <summary>
        /// Include required JavaScript, Css files when displaying a component, for all components in this package.
        /// </summary>
        public override async Task IncludeStandardDisplayAsync() {
            await Manager.AddOnManager.AddAddOnGlobalAsync("jquery.com", "jquery");
            await Manager.AddOnManager.AddAddOnGlobalAsync("jqueryui.com", "jqueryui");
        }
        /// <summary>
        /// Include required JavaScript, Css files when editing a component, for all components in this package.
        /// </summary>
        public override async Task IncludeStandardEditAsync() {
            await Manager.AddOnManager.AddAddOnGlobalAsync("bassistance.de", "jquery-validation");
            await Manager.AddOnManager.AddAddOnGlobalAsync("microsoft.com", "jquery_unobtrusive_validation");
            await Manager.AddOnManager.AddAddOnGlobalAsync("gist.github.com_remi_957732", "jquery_validate_hooks");
            await Manager.AddOnManager.AddAddOnGlobalAsync("jquery.com", "jquery");
            await Manager.AddOnManager.AddAddOnGlobalAsync("jqueryui.com", "jqueryui");
        }

        /// <summary>
        /// Include required JavaScript, Css files for this component.
        /// </summary>
        public virtual async Task IncludeAsync() {
            await Manager.AddOnManager.AddTemplateAsync(Package.Domain, Package.Product, GetTemplateName());
        }

        public string MakeId(YTagBuilder tag) {
            string id = (from a in tag.Attributes where string.Compare(a.Key, "id", true) == 0 select a.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(id)) {
                id = Manager.UniqueId();
                tag.Attributes.Add("id", id);
            }
            return id;
        }

        /// <summary>
        /// Add HTML attributes and name= attribute to tag.
        /// </summary>
        /// <remarks>This is used for the main tag of a template.
        /// Also adds validation attributes.</remarks>
        public void FieldSetup(YTagBuilder tag, FieldType fieldType) {
            if (HtmlAttributes != null)
                tag.MergeAttributes(HtmlAttributes, false);
            switch (fieldType) {
                case FieldType.Anonymous:
                    break;
                case FieldType.Normal:
                    tag.MergeAttribute("name", FieldName, false);
                    break;
                case FieldType.Validated:
                    tag.MergeAttribute("name", FieldName, false);
                    // error state
                    AddErrorClass(tag);
                    // client side validation
                    AddValidation(tag);
                    break;
            }
        }
        private void AddErrorClass(YTagBuilder tagBuilder) {
            string cls = GetErrorClass();
            if (!string.IsNullOrWhiteSpace(cls))
                tagBuilder.AddCssClass(Manager.AddOnManager.CheckInvokedCssModule(cls));
        }
        public string GetErrorClass() {
#if MVC6
            ModelStateEntry modelState;
#else
            ModelState modelState;
#endif
            if (HtmlHelper.ViewData.ModelState.TryGetValue(FieldName, out modelState)) {
                if (modelState.Errors.Count > 0)
                    return HtmlHelper.ValidationInputCssClassName;
            }
            return null;
        }
        private void AddValidation(YTagBuilder tagBuilder) {
#if MVC6
            ModelMetadata metadata = metadataProvider.GetMetadataForProperty(Container, PropertyName);
#else
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => PropData.GetPropertyValue<object>(Container), Container.GetType(), PropertyName);
#endif
            IDictionary<string, object> attrs = HtmlHelper.GetUnobtrusiveValidationAttributes(PropertyName, metadata);

#if MVC6
            //$$$ ??
#else
            // mvc5 won't render a field with the same name. This conflicts with out notion of nested components,
            // which easily can have fields with the same name, except the prefix is different. But mvc5 doesn't know about our prefix
            FormContext formContext = HtmlHelper.ViewContext.FormContext;
            formContext.RenderedField(FieldName, false);
            formContext.RenderedField(PropertyName, false);
#endif
            tagBuilder.MergeAttributes(attrs, replaceExisting: false);

            // patch up auto-generated "required" validation (added by MVC) and rename our own customrequired validation to required
            if (tagBuilder.Attributes.ContainsKey("data-val-required")) {
                tagBuilder.Attributes.Remove("data-val-required");
            }
            if (tagBuilder.Attributes.ContainsKey("data-val-customrequired")) {
                tagBuilder.Attributes.Add("data-val-required", tagBuilder.Attributes["data-val-customrequired"]);
                tagBuilder.Attributes.Remove("data-val-customrequired");
            }
            // replace type dependent messages (MVC, please, who asked for this?)
            if (tagBuilder.Attributes.ContainsKey("data-val-number"))
                tagBuilder.Attributes["data-val-number"] = this.__ResStr("valNumber", "Please enter a valid number for field '{0}'", PropData.GetCaption(Container));
            if (tagBuilder.Attributes.ContainsKey("data-val-date"))
                tagBuilder.Attributes["data-val-date"] = this.__ResStr("valDate", "Please enter a valid date for field '{0}'", PropData.GetCaption(Container));
        }
        protected IHtmlString ValidationMessage(string fieldName) {
            // ValidationMessage is always called for a child component within the context of the PARENT
            // component, so we need to prefix the child component field name with the parent field name
            if (!IsContainerComponent)
                fieldName = FieldName + "." + fieldName;
            if (!string.IsNullOrWhiteSpace(FieldNamePrefix))
                fieldName = FieldNamePrefix + "." + fieldName;
            return HtmlHelper.ValidationMessage(fieldName);
        }
        internal static IHtmlString ValidationMessage(
#if MVC6
            IHtmlHelper htmlHelper,
#else
            HtmlHelper htmlHelper,
#endif
                 string containerFieldPrefix, string fieldName) {
            // ValidationMessage is always called for a child component within the context of the PARENT
            // component, so we need to prefix the child component field name with the parent field name
            if (!string.IsNullOrEmpty(containerFieldPrefix))
                fieldName = containerFieldPrefix + "." + fieldName;
            return htmlHelper.ValidationMessage(fieldName);
        }

        protected class JSDocumentReady : IDisposable {

            public JSDocumentReady(HtmlBuilder hb) {
                this.HB = hb;
                DisposableTracker.AddObject(this);
            }
            public void Dispose() { Dispose(true); }
            protected virtual void Dispose(bool disposing) {
                if (disposing) DisposableTracker.RemoveObject(this);
                while (CloseParen > 0) {
                    HB.Append("}");
                    CloseParen = CloseParen - 1;
                }
                HB.Append("}});");
            }
            //~JSDocumentReady() { Dispose(false); }
            public HtmlBuilder HB { get; set; }
            public int CloseParen { get; internal set; }
        }
        protected JSDocumentReady DocumentReady(HtmlBuilder hb, string id) {
            hb.Append($@"YetaWF_Basics.whenReadyOnce.push({{callback: function ($tag) {{ if ($tag.has('#{id}').length > 0) {{");
            return new JSDocumentReady(hb) { CloseParen = 1 };
        }
        protected JSDocumentReady DocumentReady(HtmlBuilder hb) {
            hb.Append("YetaWF_Basics.whenReadyOnce.push({callback: function ($tag) {\n");
            return new JSDocumentReady(hb);
        }
    }
}
