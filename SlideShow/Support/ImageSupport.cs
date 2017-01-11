﻿/* Copyright © 2017 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/SlideShow#License */

using System;
using System.IO;
using YetaWF.Core.Modules;
using YetaWF.Core.Support;

namespace YetaWF.Modules.SlideShow.Support {

    public class ImageSupport : IInitializeApplicationStartup {

        // IInitializeApplicationStartup
        // IInitializeApplicationStartup
        // IInitializeApplicationStartup

        public const string ImageType = "YetaWF_SlideShow";

        public void InitializeApplicationStartup() {
            YetaWF.Core.Image.ImageSupport.AddHandler(ImageType, GetAsFile: RetrieveImage);
        }
        private bool RetrieveImage(string name, string location, out string fileName) {
            fileName = null;
            if (!string.IsNullOrWhiteSpace(location)) return false;
            if (string.IsNullOrWhiteSpace(name)) return false;
            string[] parts = name.Split(new char[] { ',' });
            if (parts.Length != 3) return false;
            string folderGuid = parts[0];
            string propertyName = parts[1];
            string fileGuid = parts[2];
            string path = ModuleDefinition.GetModuleDataFolder(new Guid(folderGuid));
            fileName = Path.Combine(path, propertyName, fileGuid);
            return true;
        }
    }
}
