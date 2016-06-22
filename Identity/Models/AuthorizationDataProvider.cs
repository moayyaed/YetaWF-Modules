﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.Identity;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Core.Views.Shared;
using YetaWF.Modules.Identity.Addons;
using YetaWF.Modules.Identity.Controllers;

namespace YetaWF.Modules.Identity.DataProvider {

    public class Authorization {

        public const int MaxResourceName = 200;
        public const int MaxResourceDescription = 500;

        [Data_PrimaryKey, StringLength(MaxResourceName)]
        public string ResourceName { get; set; }

        [StringLength(MaxResourceDescription)]
        public string ResourceDescription { get; set; }

        [Data_Binary]
        public SerializableList<User> AllowedUsers { get; set; }
        [Data_Binary]
        public SerializableList<Role> AllowedRoles { get; set; }

        [DontSave]
        public bool CanDelete { get; set; }

        public Authorization() {
            AllowedUsers = new SerializableList<User>();
            AllowedRoles = new SerializableList<Role>();
        }
    }

    public class AuthorizationDataProvider : DataProviderImpl, IInstallableModel {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public AuthorizationDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }
        public AuthorizationDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(DataProvider); }

        private IDataProvider<string, Authorization> DataProvider {
            get {
                if (_dataProvider == null) {
                    switch (GetIOMode(AreaRegistration.CurrentPackage.AreaName)) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            _dataProvider = new YetaWF.DataProvider.FileDataProvider<string, Authorization>(
                                Path.Combine(YetaWFManager.DataFolder, AreaName, "Authorization", SiteIdentity.ToString()),
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLSimpleObjectDataProvider<string, Authorization>(AreaName + "_Authorization", SQLDbo, SQLConn,
                                CurrentSiteIdentity: SiteIdentity,
                                NoLanguages: true,
                                Cacheable: true);
                            break;
                    }
                }
                return _dataProvider;
            }
        }
        private IDataProvider<string, Authorization> _dataProvider { get; set; }

        // LOAD/SAVE
        // LOAD/SAVE
        // LOAD/SAVE

        public Authorization GetItem(string resourceName) {
            Authorization data = DataProvider.Get(resourceName);
            if (data != null) {
                data.CanDelete = true;
                return data;
            }
            AuthorizationResourceDataProvider authResDP = new AuthorizationResourceDataProvider();
            using (RoleDefinitionDataProvider roleDP = new RoleDefinitionDataProvider(SiteIdentity)) {
                return GetFromAuthorizationResource(roleDP, authResDP.GetItem(resourceName));
            }
        }
        public UpdateStatusEnum UpdateItem(Authorization data) {
            CleanupUsersAndRoles(data);
            if (data.AllowedUsers.Count > Info.MAX_USERS_IN_RESOURCE)
                throw new Error(this.__ResStr("maxUsers", "Only up to {0} users can be authorized for a resource. Consider creating a role instead, and add all users to that role. There is no limit to the number of users that can be added to a role."));
            UpdateStatusEnum status = DataProvider.Update(data.ResourceName, data.ResourceName, data);
            if (status == UpdateStatusEnum.RecordDeleted) {
                if (!DataProvider.Add(data))
                    throw new Error(this.__ResStr("addFail", "Unexpected error adding new record - {0}", status));
                status = UpdateStatusEnum.OK;
            }
            if (status != UpdateStatusEnum.OK)
                throw new Error(this.__ResStr("updFail", "Unexpected error updating record - {0}", status));
            return status;
        }
        public bool RemoveItem(string resourceName) {
            DataProvider.Remove(resourceName);
            return true;
        }
        public List<Authorization> GetItems(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, out int total) {
            // get all defined authorizations
            List<Authorization> list = DataProvider.GetRecords(0, 0, null, filters, out total);
            foreach (Authorization l in list) l.CanDelete = true;
            // get all AuthorizationResource items
            using (RoleDefinitionDataProvider roleDP = new RoleDefinitionDataProvider(SiteIdentity)) {
                AuthorizationResourceDataProvider authResDP = new AuthorizationResourceDataProvider();
                List<ResourceAttribute> resAttrs = authResDP.GetItems();
                // merge in AuthorizationResource items
                foreach (ResourceAttribute resAttr in resAttrs) {
                    Authorization auth = (from l in list where l.ResourceName == resAttr.Name select l).FirstOrDefault();
                    if (auth == null) {
                        auth = GetFromAuthorizationResource(roleDP, resAttr);
                        list.Add(auth);
                    }
                }
            }
            list = DataProviderImpl<Authorization>.GetRecords(list, skip, take, sort, filters, out total);
            return list;
        }

        private Authorization GetFromAuthorizationResource(RoleDefinitionDataProvider roleDP, ResourceAttribute resAttr) {
            Authorization auth = new Authorization() {
                ResourceName = resAttr.Name,
                ResourceDescription = resAttr.Description,
            };
            auth.AllowedRoles.Add(new Role() { RoleId = RoleDefinitionDataProvider.SuperUserId });
            if (resAttr.Anonymous)
                auth.AllowedRoles.Add(new Role() { RoleId = roleDP.GetAnonymousRoleId() });
            if (resAttr.User)
                auth.AllowedRoles.Add(new Role() { RoleId = roleDP.GetUserRoleId() });
            if (resAttr.Editor)
                auth.AllowedRoles.Add(new Role() { RoleId = roleDP.GetEditorRoleId() });
            if (resAttr.Administrator)
                auth.AllowedRoles.Add(new Role() { RoleId = roleDP.GetAdministratorRoleId() });
            return auth;
        }
        public int RemoveItems(List<DataProviderFilterInfo> filters) {
            return DataProvider.RemoveRecords(filters);
        }

        private void CleanupUsersAndRoles(Authorization data) {
            // remove superuser from allowed roles as he/she's in there by default
            using (RoleDefinitionDataProvider roleDP = new RoleDefinitionDataProvider(SiteIdentity)) {
                if (data.AllowedRoles == null) {
                    data.AllowedRoles = new SerializableList<Role>();
                } else {
                    data.AllowedRoles = new SerializableList<Role>((from r in data.AllowedRoles where r.RoleId != RoleDefinitionDataProvider.SuperUserId select r).ToList());
                }
                if (data.AllowedUsers != null && data.AllowedUsers.Count > Info.MAX_USERS_IN_RESOURCE)
                    throw new Error(this.__ResStr("maxUsers", "Only up to {0} users can be authorized for a resource. Consider creating a role instead, and add all users to that role. There is no limit to the number of users that can be added to a role."));
                // remove superuser from allowed users as he/she's allowed by default
                if (data.AllowedUsers == null) {
                    data.AllowedUsers = new SerializableList<User>();
                } else {
                    data.AllowedUsers = new SerializableList<User>((from u in data.AllowedUsers where u.UserId != SuperuserDefinitionDataProvider.SuperUserId select u).ToList());
                }
            }
        }

        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL

        public bool IsInstalled() {
            return DataProvider.IsInstalled();
        }
        public bool InstallModel(List<string> errorList) {
            return DataProvider.InstallModel(errorList);
        }
        public bool UninstallModel(List<string> errorList) {
            return DataProvider.UninstallModel(errorList);
        }
        public void AddSiteData() {
            DataProvider.AddSiteData();
        }
        public void RemoveSiteData() {
            DataProvider.RemoveSiteData();
        }
        public bool ExportChunk(int chunk, SerializableList<SerializableFile> fileList, out object obj) {
            return DataProvider.ExportChunk(chunk, fileList, out obj);
        }
        public void ImportChunk(int chunk, SerializableList<SerializableFile> fileList, object obj) {
            DataProvider.ImportChunk(chunk, fileList, obj);
        }
    }
}