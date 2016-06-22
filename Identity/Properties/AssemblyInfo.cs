﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Reflection;
using System.Runtime.InteropServices;
using YetaWF.Modules.Identity.Addons;
using YetaWF.Core.Identity;
using YetaWF.PackageAttributes;
using YetaWF.Core.Packages;

[assembly: AssemblyTitle("Identity")]
[assembly: AssemblyDescription("User login, registration and authentication")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Softel vdm, Inc.")]
[assembly: AssemblyProduct("Identity")]
[assembly: AssemblyCopyright("Copyright © 2016 - Softel vdm, Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0.0")]

[assembly: Package(PackageTypeEnum.Module, "YetaWF")]
[assembly: PackageInfo("http://YetaWF.com/UpdateServer",
    "http://yetawf.com/Documentation/YetaWF/Identity",
    "http://YetaWF.com/Documentation/YetaWF/Support",
    "http://yetawf.com/Documentation/YetaWF/Identity#Release%20Notice",
    "http://yetawf.com/Documentation/YetaWF/Identity#License")]

[assembly: PublicPartialViews]

[assembly: ServiceLevel(ServiceLevelEnum.LowLevelServiceProvider)]

[assembly: Resource(Info.Resource_AllowUserLogon, "Allow logon as another user", Administrator = true, Superuser = true)]

[assembly: InstallOrder(typeof(YetaWF.Modules.Identity.DataProvider.RoleDefinitionDataProvider))]
[assembly: InstallOrder(typeof(YetaWF.Modules.Identity.DataProvider.SuperuserDefinitionDataProvider))]
[assembly: InstallOrder(typeof(YetaWF.Modules.Identity.DataProvider.UserDefinitionDataProvider))]
[assembly: InstallOrder(typeof(YetaWF.Modules.Identity.DataProvider.AuthorizationDataProvider))]