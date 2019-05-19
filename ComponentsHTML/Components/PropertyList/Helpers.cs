﻿/* Copyright © 2019 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/ComponentsHTML#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;

namespace YetaWF.Modules.ComponentsHTML.Components {

    public abstract partial class PropertyListComponentBase {

        /// <summary>
        /// Defines the appearance of a property list.
        /// </summary>
        public enum PropertyListStyleEnum {
            /// <summary>
            /// Render a tabbed property list (if there are multiple categories) or a simple list (0 or 1 category).
            /// </summary>
            Tabbed = 0,
            /// <summary>
            /// Render a boxed property list (if there are multiple categories) or a simple list (0 or 1 category), to be styled using CSS.
            /// </summary>
            Boxed = 1,
            /// <summary>
            /// Render a boxed property list with category labels (if there are multiple categories) or a simple list (0 or 1 category), to be styled using CSS.
            /// </summary>
            BoxedWithCategories = 2,
        }

        /// <summary>
        /// An instance of this class defines the property list appearance.
        /// </summary>
        public class PropertyListSetup {
            /// <summary>
            /// The style of the property list.
            /// </summary>
            public PropertyListStyleEnum Style { get; set; }
            /// <summary>
            /// For Boxed and BoxedWithCategories styles, Masonry (https://masonry.desandro.com/) is used to support a packed layout of categories.
            /// </summary>
            /// <remarks>This collection defines the number of columns depending on windows size.
            /// By providing a list of break points, Masonry can be called to recalculate the box layout, when switching between window widths which affects the number of columns.
            ///
            /// The first entry defines the minimum width of the window to use Masonry. Below this size, Masonry is not used.
            /// </remarks>
            public List<PropertyListColumnDef> ColumnStyles { get; set; }
            /// <summary>
            /// Categories (boxes) that are expandable/collapsible. May be null or an empty collection, which means no categories are expandable.
            /// </summary>
            public List<string> ExpandableList { get; set; }
            /// <summary>
            /// Category that is initially expanded. May be null which means no category is initially expanded.
            /// </summary>
            public string InitialExpanded { get; set; }

            public PropertyListSetup() {
                Style = PropertyListStyleEnum.Tabbed;
                ColumnStyles = new List<PropertyListColumnDef>();
                ExpandableList = new List<string>();
                InitialExpanded = null;
            }
        }
        /// <summary>
        /// An instance of this class defines the number of columns to display based on the defined minimum window width.
        /// </summary>
        public class PropertyListColumnDef {
            /// <summary>
            /// The minimum window size where the specified number of columns is displayed.
            /// </summary>
            public int MinWindowSize { get; set; }
            /// <summary>
            /// The number of columns to display. Valid values are 1 through 5.
            /// </summary>
            public int Columns { get; set; }
        }


        internal class PropertyListEntry {

            public PropertyListEntry(string name, object value, string uiHint, bool editable, bool restricted, string textAbove, string textBelow, bool suppressEmpty,
                    List<ExprAttribute> exprAttrs,
                    SubmitFormOnChangeAttribute.SubmitTypeEnum submit) {
                Name = name; Value = value; Editable = editable;
                Restricted = restricted;
                TextAbove = textAbove; TextBelow = textBelow;
                UIHint = uiHint;
                ExprAttrs = exprAttrs;
                SuppressEmpty = suppressEmpty;
                SubmitType = submit;
            }
            public object Value { get; private set; }
            public string Name { get; private set; }
            public string TextAbove { get; private set; }
            public string TextBelow { get; private set; }
            public bool Editable { get; private set; }
            public bool Restricted { get; private set; }
            public string UIHint { get; private set; }
            public bool SuppressEmpty { get; private set; }
            public SubmitFormOnChangeAttribute.SubmitTypeEnum SubmitType { get; private set; }
            public List<ExprAttribute> ExprAttrs { get; set; }
        };

        // returns all properties for an object that have a description, in sorted order
        private IEnumerable<PropertyData> GetProperties(Type objType) {
            return from property in ObjectSupport.GetPropertyData(objType)
                    where property.Description != null  // This means it has to be a DescriptionAttribute (not a resource redirect)
                    orderby property.Order
                    select property;
        }

        internal static List<PropertyListEntry> GetHiddenProperties(object obj) {
            List<PropertyListEntry> properties = new List<PropertyListEntry>();
            List<PropertyData> props = ObjectSupport.GetPropertyData(obj.GetType());
            foreach (var prop in props) {
                if (!prop.PropInfo.CanRead) continue;
                if (prop.UIHint != "Hidden")
                    continue;
                properties.Add(new PropertyListEntry(prop.Name, prop.GetPropertyValue<object>(obj), "Hidden", false, false, null, null, false, null, SubmitFormOnChangeAttribute.SubmitTypeEnum.None));
            }
            return properties;
        }

        internal PropertyListSetup GetPropertyListSetup(object obj) {

            // get all properties that are shown
            Type objType = obj.GetType();
            PropertyInfo propInfo = ObjectSupport.TryGetProperty(objType, "__PropertyListSetup");
            if (propInfo != null)
                return (PropertyListSetup)propInfo.GetValue(obj);
            return new PropertyListSetup();
        }

        // Returns all categories implemented by this object - these are decorated with the [CategoryAttribute]
        internal List<string> GetCategories(object obj) {

            // get all properties that are shown
            Type objType = obj.GetType();
            List<PropertyData> props = GetProperties(objType).ToList();

            // get the list of categories
            List<string> categories = new List<string>();
            foreach (PropertyData prop in props)
                categories.AddRange(prop.Categories);
            categories = categories.Distinct().ToList();

            // order (if there is a CategoryOrder property)
            PropertyInfo piCat = ObjectSupport.TryGetProperty(objType, "CategoryOrder");
            if (piCat != null) {
                List<string> orderedCategories = (List<string>)piCat.GetValue(obj);
                List<string> allCategories = new List<string>();
                // verify that all returned categories in the list of ordered categories actually exist
                foreach (var oCat in orderedCategories) {
                    if (categories.Contains(oCat))
                        allCategories.Add(oCat);
                    //else
                    //throw new InternalError("No properties exist in category {0} found in CategoryOrder for type {1}.", oCat, obj.GetType().Name);
                }
                // if any are missing, add them to the end of the list
                foreach (var cat in categories) {
                    if (!allCategories.Contains(cat))
                        allCategories.Add(cat);
                }
                categories = allCategories;
            }
            return categories;
        }
        internal List<PropertyListEntry> GetPropertiesByCategory(object obj, string category) {

            List<PropertyListEntry> properties = new List<PropertyListEntry>();
            Type objType = obj.GetType();
            var props = GetProperties(objType);
            foreach (var prop in props) {
                if (!string.IsNullOrWhiteSpace(category) && !prop.Categories.Contains(category))
                    continue;

                if (ExprAttribute.IsSuppressed(prop.ExprValidationAttributes, obj))
                    continue;// suppress this as requested

                bool editable = prop.PropInfo.CanWrite;
                if (editable) {
                    if (prop.ReadOnly)
                        editable = false;
                }
                SuppressEmptyAttribute suppressEmptyAttr = null;
                suppressEmptyAttr = prop.TryGetAttribute<SuppressEmptyAttribute>();

                SubmitFormOnChangeAttribute submitFormOnChangeAttr = null;
                submitFormOnChangeAttr = prop.TryGetAttribute<SubmitFormOnChangeAttribute>();

                bool restricted = false;
                if (Manager.IsDemo) {
                    ExcludeDemoModeAttribute exclDemoAttr = prop.TryGetAttribute<ExcludeDemoModeAttribute>();
                    if (exclDemoAttr != null)
                        restricted = true;
                }
                properties.Add(
                    new PropertyListEntry(prop.Name, prop.GetPropertyValue<object>(obj), prop.UIHint, editable, restricted, prop.TextAbove, prop.TextBelow,
                        suppressEmptyAttr != null, prop.ExprValidationAttributes,
                        submitFormOnChangeAttr != null ? submitFormOnChangeAttr.Value : SubmitFormOnChangeAttribute.SubmitTypeEnum.None)
                );
            }
            return properties;
        }
    }
}