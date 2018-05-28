﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YetaWF.Core.Components;
using YetaWF.Core.Packages;
using YetaWF.Core.Skins;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;

namespace YetaWF.Modules.ComponentsHTML.Components {

    public abstract class SkinNamePageComponentBase : YetaWFComponent {

        public const string TemplateName = "SkinNamePage";

        public override Package GetPackage() { return Controllers.AreaRegistration.CurrentPackage; }
        public override string GetTemplateName() { return TemplateName; }
    }

    public class SkinNamePageDisplayComponent : SkinNamePageComponentBase, IYetaWFComponent<string> {

        public override ComponentType GetComponentType() { return ComponentType.Display; }

        public Task<YHtmlString> RenderAsync(string model) {

            // get all available page skins for this collection
            SkinAccess skinAccess = new SkinAccess();
            string collection = GetSiblingProperty<string>($"{PropertyName}_Collection");
            PageSkinList skinList = skinAccess.GetAllPageSkins(collection);

            string desc = (from skin in skinList where skin.FileName == model select skin.Name).FirstOrDefault();
            if (desc == null)
                desc = skinList.First().Description;
            return Task.FromResult(new YHtmlString(string.IsNullOrWhiteSpace(desc) ? "&nbsp;" : desc));
        }
    }

    public class SkinNamePageEditComponent : SkinNamePageComponentBase, IYetaWFComponent<string> {

        public override ComponentType GetComponentType() { return ComponentType.Edit; }

        public async Task<YHtmlString> RenderAsync(string model) {

            // get all available page skins for this collection
            SkinAccess skinAccess = new SkinAccess();
            string collection = GetSiblingProperty<string>($"{PropertyName}_Collection");
            PageSkinList skinList = skinAccess.GetAllPageSkins(collection);
            List<SelectionItem<string>> list = (from skin in skinList orderby skin.Description select new SelectionItem<string>() {
                Text = skin.Name,
                Tooltip = skin.Description,
                Value = skin.FileName,
            }).ToList();
            // display the skins in a drop down
            return await DropDownListComponent.RenderDropDownListAsync(model, list, this, "yt_skinname");
        }
    }
}
