﻿/* Copyright © 2020 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/PageEdit#License */

using System.Threading.Tasks;
using YetaWF.Core.Addons;
using YetaWF.Core.Components;
using YetaWF.Core.Identity;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Packages;
using YetaWF.Core.Support;
using YetaWF.Modules.ComponentsHTML.Components;
using YetaWF.Modules.PageEdit.Addons;
using YetaWF.Modules.PageEdit.Controllers;
using YetaWF.Modules.PageEdit.Modules;

namespace YetaWF.Modules.PageEdit.Views {

    public class PageControlView : YetaWFView, IYetaWFView<PageControlModule, PageControlModuleController.PageControlModel> {

        public const string ViewName = "PageControl";

        public override Package GetPackage() { return AreaRegistration.CurrentPackage; }
        public override string GetViewName() { return ViewName; }

        internal class UI {
            [UIHint("Tabs")]
            public TabsDefinition TabsDef { get; set; }
        }

        public async Task<string> RenderViewAsync(PageControlModule module, PageControlModuleController.PageControlModel model) {

            // <div id="yPageControlDiv">
            //  action button (with id tid_pagecontrolbutton)
            //  module html...
            // </div>
            YTagBuilder tag = new YTagBuilder("div");
            tag.Attributes.Add("id", "yPageControlDiv");

            ModuleAction action = await module.GetAction_PageControlAsync();
            tag.InnerHtml = await action.RenderAsButtonIconAsync("tid_pagecontrolbutton");


            bool canEdit = model.EditAuthorized;
            bool canImportPage = false;
            bool canImportModule = false;
            bool canPageAdd = false;
            bool canModuleExistingAdd = false;
            bool canModuleNewAdd = false;
            bool canChangeSiteSkins = false;
            if (canEdit) {
                canImportPage = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_PageImport);
                canImportModule = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_ModuleImport);
                canPageAdd = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_PageAdd);
                canModuleExistingAdd = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_ModuleExistingAdd);
                canModuleNewAdd = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_ModuleNewAdd);
                canChangeSiteSkins = await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_SiteSkins);
            }
            bool canOtherUserLogin = !YetaWFManager.Deployed || await Resource.ResourceAccess.IsResourceAuthorizedAsync(CoreInfo.Resource_OtherUserLogin);

            string id = Info.PageControlMod;


            UI ui = new UI {
                TabsDef = new TabsDefinition()
            };
            if (canEdit) {
                if (canPageAdd) {
                    ui.TabsDef.Tabs.Add(new TabEntry { 
                        Caption = this.__ResStr("tabNewPage", "New Page"),
                        ToolTip = this.__ResStr("tabNewPageTT", "Add a new page to the site"),
                        PaneCssClasses = "t_addNewPage",
                        RenderPaneAsync = async (int tabIndex) => {
                            return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_AddNewPage", module, model.AddNewPageModel)).ToString();
                        },
                    }); 
                }
            }
            if (!Manager.CurrentPage.Temporary) {
                if (canEdit) {
                    if (canModuleNewAdd) {
                        ui.TabsDef.Tabs.Add(new TabEntry {
                            Caption = this.__ResStr("tabNew", "New Module"),
                            ToolTip = this.__ResStr("tabNewTT", "Add a new module to this page (creates a new module)"),
                            PaneCssClasses = "t_addNewMod",
                            RenderPaneAsync = async (int tabIndex) => {
                                return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_AddNewModule", module, model.AddNewModel)).ToString();
                            },
                        });
                    }
                    if (canModuleExistingAdd) {
                        ui.TabsDef.Tabs.Add(new TabEntry {
                            Caption = this.__ResStr("tabOld", "Existing Module"),
                            ToolTip = this.__ResStr("tabOldTT", "Add an existing module to this page (this does not copy the module)"),
                            PaneCssClasses = "t_addExistingMod",
                            RenderPaneAsync = async (int tabIndex) => {
                                return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_AddExistingModule", module, model.AddExistingModel)).ToString();
                            },
                        });
                    }
                    if (canImportPage) {
                        ui.TabsDef.Tabs.Add(new TabEntry {
                            Caption = this.__ResStr("tabImportPage", "Import Page"),
                            ToolTip = this.__ResStr("tabImportPageTT", "Import a page (creates a new page)"),
                            PaneCssClasses = "t_importPage",
                            RenderPaneAsync = async (int tabIndex) => {
                                return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_ImportPage", module, model.ImportPageModel)).ToString();
                            },
                        });
                    }
                    if (canImportModule) {
                        ui.TabsDef.Tabs.Add(new TabEntry {
                            Caption = this.__ResStr("tabImportModule", "Import Module"),
                            ToolTip = this.__ResStr("tabImportModuleTT", "Import module data into this page (creates a new module)"),
                            PaneCssClasses = "t_importMod",
                            RenderPaneAsync = async (int tabIndex) => {
                                return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_ImportModule", module, model.ImportModuleModel)).ToString();
                            },
                        });
                    }
                    if (canChangeSiteSkins) {
                        ui.TabsDef.Tabs.Add(new TabEntry {
                            Caption = this.__ResStr("tabSkins", "Skins"),
                            ToolTip = this.__ResStr("tabSkinsTT", "Change default skins used site wide"),
                            PaneCssClasses = "t_addSkins",
                            RenderPaneAsync = async (int tabIndex) => {
                                return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_SkinSelection", module, model.SkinSelectionModel)).ToString();
                            },
                        });
                    }
                }
            }
            if (canOtherUserLogin) {
                ui.TabsDef.Tabs.Add(new TabEntry {
                    Caption = this.__ResStr("tabLogin", "Login"),
                    ToolTip = this.__ResStr("tabLoginTT", "Change site or log in as another user"),
                    PaneCssClasses = "t_login",
                    RenderPaneAsync = async (int tabIndex) => {
                        return (await HtmlHelper.ForViewAsync($"{Package.AreaName}_LoginSiteSelection", module, model.LoginSiteSelectionModel)).ToString();
                    },
                });
            }

            if (ui.TabsDef.Tabs.Count == 0)
                return "&nbsp;";



            HtmlBuilder hb = new HtmlBuilder();
            hb.Append($@"
{tag.ToString(YTagRenderMode.Normal)}
<div id='{id}'>
    {await HtmlHelper.ForDisplayAsync(ui, nameof(ui.TabsDef), HtmlAttributes: new { __NoTemplate = true })}
    {await HtmlHelper.ForDisplayAsync(model, nameof(model.Actions))}
</div>");

            return hb.ToString();
        }
    }
}
