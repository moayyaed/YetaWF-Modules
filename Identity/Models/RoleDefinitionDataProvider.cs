﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Identity#License */

using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YetaWF.Core;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.DataProvider;

namespace YetaWF.Modules.Identity.DataProvider {

    public class RoleDefinition : IRole {

        public const int MaxName = 100;
        public const int MaxDescription = 200;

        [Data_PrimaryKey, StringLength(MaxName)]
        public string Name { get; set; }
        [Data_Identity]
        public int RoleId { get; set; } // our internal role id

        // asp.net id
        [DontSave]
        public string Id { get { return RoleId.ToString(); } set { RoleId = Convert.ToInt32(value); } }

        [StringLength(MaxDescription)]
        public string Description { get; set; }

        public RoleDefinition() { }
    }

    /// <summary>
    /// RoleDefinitionDataProvider
    /// Roles are separated by site
    /// File,SQL - A small set of roles is expected, so they're preloaded - If a large # > 100 is expected, this must be rewritten
    /// </summary>
    public class RoleDefinitionDataProvider : DataProviderImpl, IInstallableModel {

        private static object _lockObject = new object();

        public static int SuperUserId = SQLDataProviderImpl.IDENTITY_SEED - 3;
        private static int UserIdentity = SQLDataProviderImpl.IDENTITY_SEED - 2;
        private static int AnonymousIdentity = SQLDataProviderImpl.IDENTITY_SEED - 1;

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public RoleDefinitionDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }
        public RoleDefinitionDataProvider(int siteIdentity) : base(siteIdentity) { SetDataProvider(DataProvider); }

        private IDataProvider<string, RoleDefinition> DataProvider {
            get {
                if (_dataProvider == null) {
                    Package package = Package.GetPackageFromAssembly(GetType().Assembly);
                    switch (GetIOMode(package.AreaName)) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            _dataProvider = new YetaWF.DataProvider.FileDataProvider<string, RoleDefinition>(
                                Path.Combine(YetaWFManager.DataFolder, AreaName, "Roles", SiteIdentity.ToString()),
                                CurrentSiteIdentity: SiteIdentity,
                                Cacheable: true);
                            break;
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLSimpleObjectDataProvider<string, RoleDefinition>(AreaName + "_Roles", SQLDbo, SQLConn,
                                CurrentSiteIdentity: SiteIdentity,
                                NoLanguages: true,
                                Cacheable: true);
                            break;
                    }
                }
                return _dataProvider;
            }
        }
        private IDataProvider<string, RoleDefinition> _dataProvider { get; set; }

        // LOAD/SAVE
        // LOAD/SAVE
        // LOAD/SAVE

        public RoleDefinition GetItem(string key) {
            if (key == Globals.Role_Superuser)
                return MakeSuperuserRole();
            return DataProvider.Get(key);
        }
        public RoleDefinition GetRoleById(string roleId) {
            if (roleId == RoleDefinitionDataProvider.SuperUserId.ToString())
                return MakeSuperuserRole();
            List<RoleDefinition> roles = GetAllRoles();
            return (from RoleDefinition r in roles where r.Id == roleId select r).FirstOrDefault();
        }
        public RoleDefinition GetRoleById(int roleId) {
            if (roleId == RoleDefinitionDataProvider.SuperUserId)
                return MakeSuperuserRole();
            List<RoleDefinition> roles = GetAllRoles();
            return (from RoleDefinition r in roles where r.RoleId == roleId select r).FirstOrDefault();
        }
        public bool AddItem(RoleDefinition data) {
            if (data.RoleId == RoleDefinitionDataProvider.SuperUserId || data.Name == Globals.Role_Superuser)
                throw new InternalError("Can't add built-in superuser role");
            if (!DataProvider.Add(data))
                return false;
            GetAllUserRoles(true);
            return true;
        }
        public UpdateStatusEnum UpdateItem(RoleDefinition data) {
            return UpdateItem(data.Name, data);
        }
        public UpdateStatusEnum UpdateItem(string originalRole, RoleDefinition data) {
            if (data.RoleId == RoleDefinitionDataProvider.SuperUserId || data.Name == Globals.Role_Superuser)
                throw new InternalError("Can't update built-in superuser role");
            if (originalRole != data.Name && IsPredefinedRole(originalRole))
                throw new Error(this.__ResStr("cantUpdateUser", "The {0} role can't be updated", originalRole));
            UpdateStatusEnum status = DataProvider.Update(originalRole, data.Name, data);
            if (status == UpdateStatusEnum.OK)
                GetAllUserRoles(true);
            return status;
        }
        public bool RemoveItem(string role) {
            if (role == Globals.Role_Superuser)
                throw new InternalError("Can't remove built-in superuser role");
            if (IsPredefinedRole(role))
                throw new Error(this.__ResStr("cantRemoveUser", "The {0} role can't be removed", role));
            if (!DataProvider.Remove(role))
                return false;
            GetAllUserRoles(true);
            return true;
        }
        public List<RoleDefinition> GetItems() {
            int total;
            List<RoleDefinition> list = DataProvider.GetRecords(0, 0, null, null, out total);
            list.Insert(0, MakeSuperuserRole());
            return list;
        }
        public List<RoleDefinition> GetItems(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, out int total) {
            List<RoleDefinition> list = DataProvider.GetRecords(skip, take, sort, filters, out total);
            list.Insert(0, MakeSuperuserRole());
            return list;
        }
        public int RemoveItems(List<DataProviderFilterInfo> filters) {
            int count = DataProvider.RemoveRecords(filters);
            GetAllUserRoles(true);
            return count;
        }
        public int GetAdministratorRoleId() { return GetRoleId(Globals.Role_Administrator); }
        public int GetEditorRoleId() { return GetRoleId(Globals.Role_Editor); }
        public int GetUserRoleId() { return UserIdentity; }
        public int GetAnonymousRoleId() { return AnonymousIdentity; }
        public int GetRoleId(string roleName) {
            if (roleName == Globals.Role_Superuser)
                return RoleDefinitionDataProvider.SuperUserId;
            RoleDefinition role = DataProvider.Get(roleName);
            if (role == null) throw new InternalError("Required role {0} not found", roleName);
            return role.RoleId;
        }
        public bool IsPredefinedRole(string role) {
            return (role == Globals.Role_Superuser || role == Globals.Role_User || role == Globals.Role_Editor || role == Globals.Role_Anonymous || role == Globals.Role_Administrator);
        }

        // all user roles, plus User and Anonymous
        public List<RoleDefinition> GetAllRoles(bool force = false) {
            List<RoleDefinition> roles = GetAllUserRoles(force);
            roles = (from r in roles select r).ToList();
            roles.Add(MakeUserRole());
            roles.Add(MakeAnonymousRole());
            return roles;
        }

        // all roles except user and anonymous
        public List<RoleDefinition> GetAllUserRoles(bool force = false) {

            if (!DataProvider.IsInstalled())
                return new List<RoleDefinition>() { MakeSuperuserRole() };

            List<RoleDefinition> roles;
            if (!force) {
                if (PermanentManager.TryGetObject<List<RoleDefinition>>(out roles))
                    return roles;
            }
            lock (_lockObject) { // lock this so we only do this once
                // See if we already have it as a permanent object
                if (!force) {
                    if (PermanentManager.TryGetObject<List<RoleDefinition>>(out roles))
                        return roles;
                }
                // Load the roles
                roles = GetItems();

                PermanentManager.AddObject<List<RoleDefinition>>(roles);
            }
            return roles;
        }
        private RoleDefinition MakeSuperuserRole() {
            return new RoleDefinition() {
                RoleId = RoleDefinitionDataProvider.SuperUserId,
                Name = Globals.Role_Superuser,
                Description = this.__ResStr("superuserRole", "The {0} role can do EVERYTHING on all sites - no restrictions", Globals.Role_Superuser)
            };
        }
        private RoleDefinition MakeUserRole() {
            return new RoleDefinition() { RoleId = UserIdentity, Name = Globals.Role_User, Description = this.__ResStr("userRole", "The {0} role describes every authenticated user (i.e., not an anonymous user)", Globals.Role_User) };
        }
        private RoleDefinition MakeAnonymousRole() {
            return new RoleDefinition() { RoleId = AnonymousIdentity, Name = Globals.Role_Anonymous, Description = this.__ResStr("anonymousRole", "The {0} role describes every user that is not logged in (i.e., not an authenticated user)", Globals.Role_Anonymous) };
        }
        public void AddAdministratorRole() {
            DataProvider.Add(
                new RoleDefinition() {
                    Name = Globals.Role_Administrator,
                    Description = this.__ResStr("adminRole", "An administrator can do EVERYTHING on ONE site (the site where the user has the {0} role)", Globals.Role_Administrator)
                }
            );
        }
        public void AddEditorRole() {
            DataProvider.Add(
                new RoleDefinition() {
                    Name = Globals.Role_Editor,
                    Description = this.__ResStr("editorRole", "An editor is a user who can perform editing actions on the site")
                }
            );
        }
        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL

        public bool IsInstalled() {
            return DataProvider.IsInstalled();
        }
        public bool InstallModel(List<string> errorList) {
            if (!DataProvider.InstallModel(errorList))
                return false;
            AddSiteData();
            return true;
        }
        public bool UninstallModel(List<string> errorList) {
            return DataProvider.UninstallModel(errorList);
        }
        public void AddSiteData() {
            AddAdministratorRole();
            AddEditorRole();
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