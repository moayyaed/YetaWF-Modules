/* Copyright � 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Blog#License */

using System;
using System.Web.Mvc;
using YetaWF.Core.Controllers;
using YetaWF.Core.Localize;
using YetaWF.Core.Models;
using YetaWF.Core.Models.Attributes;
using YetaWF.Modules.Blog.DataProvider;

namespace YetaWF.Modules.Blog.Controllers {
    public class EntryAddModuleController : ControllerImpl<YetaWF.Modules.Blog.Modules.EntryAddModule> {

        public EntryAddModuleController() { }

        [Trim]
        public class AddModel {

            [Caption("Category"), Description("The category for this blog entry")]
            [UIHint("YetaWF_Blog_Category"), Required]
            public int CategoryIdentity { get; set; }

            [Caption("Title"), Description("The title for this blog entry")]
            [UIHint("MultiString"), StringLength(BlogEntry.MaxTitle), Required, Trim]
            public MultiString Title { get; set; }

            [Caption("Author"), Description("The name of the blog author")]
            [UIHint("Text40"), StringLength(BlogEntry.MaxAuthor), Required, Trim]
            public string Author { get; set; }

            [Caption("Allow Comments"), Description("Defines whether comments can be entered for this blog entry")]
            [UIHint("Boolean")]
            public bool OpenForComments { get; set; }

            [Caption("Published"), Description("Defines whether this entry has been published and is viewable by everyone")]
            [UIHint("Boolean")]
            public bool Published { get; set; }
            [Caption("Date Published"), Description("The date this entry has been published")]
            [UIHint("Date"), RequiredIf("Published", true)]
            public DateTime DatePublished { get; set; }

            [Caption("Summary"), Description("The summary for this blog entry - If no summary is entered, the entire blog text is shown instead of the summary")]
            [UIHint("TextArea"), AdditionalMetadata("EmHeight", 10), StringLength(BlogEntry.MaxSummary)]
            [AdditionalMetadata("TextAreaSave", false)]
            [AllowHtml]
            public string Summary { get; set; }

            [Caption("Blog Text"), Description("The complete text for this blog entry")]
            [UIHint("TextArea"), AdditionalMetadata("EmHeight", 20), StringLength(BlogEntry.MaxText)]
            [AdditionalMetadata("TextAreaSave", false)]
            [AllowHtml]
            [RequiredIf("Published", true)]
            public string Text { get; set; }

            public AddModel() {
                Title = new MultiString();
                DatePublished = DateTime.UtcNow;
            }

            public BlogEntry GetData() {
                BlogEntry data = new BlogEntry();
                ObjectSupport.CopyData(this, data);
                return data;
            }
        }

        [HttpGet]
        public ActionResult EntryAdd(int? blogCategory) {
            AddModel model = new AddModel {
                CategoryIdentity = blogCategory??0
            };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EntryAdd_Partial(AddModel model) {
            if (!ModelState.IsValid)
                return PartialView(model);

            using (BlogEntryDataProvider dataProvider = new BlogEntryDataProvider()) {
                if (!dataProvider.AddItem(model.GetData())) {
                    ModelState.AddModelError("Name", this.__ResStr("alreadyExists", "An error occurred adding this new blog entry"));
                    return PartialView(model);
                }
                return FormProcessed(model, this.__ResStr("okSaved", "New blog entry saved"), OnPopupClose: OnPopupCloseEnum.ReloadModule);
            }
        }
    }
}