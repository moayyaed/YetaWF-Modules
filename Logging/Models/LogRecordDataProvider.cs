﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Logging#License */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using YetaWF.Core;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.Extensions;
using YetaWF.Core.IO;
using YetaWF.Core.Log;
using YetaWF.Core.Models.Attributes;
using YetaWF.Core.Modules;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using static YetaWF.Core.Log.Logging;

// The logging data provider uses a simple sequential (flat) file for logging so the entire file implementation is in this code (not FileDataProvider)
// For SQL we're using the regular SQL data provider

namespace YetaWF.Modules.Logging.DataProvider {
    public class LogRecord {

        public const int MaxMethod = 100;
        public const int MaxNamespace = 100;

        [Data_PrimaryKey, Data_Identity]
        public int Key { get; set; }

        [Data_Index]
        public DateTime TimeStamp { get; set; }
        public LevelEnum Level { get; set; }
        [StringLength(ModuleDefinition.MaxName)]
        public string ModuleName { get; set; }
        [StringLength(ModuleDefinition.MaxCssClass)]
        public string Class { get; set; }
        [StringLength(MaxMethod)]
        public string Method { get; set; }
        [StringLength(MaxNamespace)]
        public string Namespace { get; set; }
        [StringLength(Globals.MaxIP)]
        public string IPAddress { get; set; }
        [StringLength(Globals.MaxUser)]
        public string UserName { get; set; }
        public int UserId { get; set; }
        [StringLength(Globals.MaxUrl)]
        public string RequestedUrl { get; set; }
        [StringLength(Globals.MaxUrl)]
        public string ReferrerUrl { get; set; }

        public int SiteIdentity { get; set; }
        [StringLength(0)]
        public string Info { get; set; }

        public LogRecord() { }
    }

    public class LogRecordDataProvider : DataProviderImpl, IInstallableModel, ILogging {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        static object lockObject = new object();

        private readonly string LogfileName = "Logfile.txt";
        public string LogFile { get; private set; }// File IO
        private const int MAXRECORDS = 1000;// cache # of records

        List<string> LogCache { get; set; }
        List<Action<string>> Callbacks { get; set; }

        public LogRecordDataProvider() : base(0) { SetDataProvider(DataProvider); }

        private IDataProvider<int, LogRecord> DataProvider {
            get {
                if (_dataProvider == null) {
                    Package package = Package.GetPackageFromAssembly(GetType().Assembly);
                    switch (GetIOMode(package.AreaName)) {
                        default:
                        case WebConfigHelper.IOModeEnum.File:
                            _dataProvider = new YetaWF.DataProvider.FileDataProvider<int, LogRecord>(
                                Path.Combine(YetaWFManager.DataFolder, AreaName));
                            LogFile = Path.Combine(YetaWFManager.DataFolder, AreaName, LogfileName);
                            break;
                        case WebConfigHelper.IOModeEnum.Sql:
                            _dataProvider = new YetaWF.DataProvider.SQLSimpleObjectDataProvider<int, LogRecord>(AreaName, SQLDbo, SQLConn,
                                Logging: false);
                            break;
                    }
                }
                return _dataProvider;
            }
        }
        private IDataProvider<int, LogRecord> _dataProvider { get; set; }


        // API
        // API
        // API

        public void Clear() {
            switch (IOMode) {
                default:
                    throw new InternalError("IOMode undetermined - this means we don't have a valid data provider");
                case WebConfigHelper.IOModeEnum.File:
                    lock (lockObject) {
                        try {
                            File.Delete(LogFile);
                        } catch (Exception) { }
                        Directory.CreateDirectory(Path.GetDirectoryName(LogFile));
                        LogCache = new List<string>();
                    }
                    break;
                case WebConfigHelper.IOModeEnum.Sql:
                    // nothing
                    break;
            }
        }

        public void Flush() {
            if (IOMode == WebConfigHelper.IOModeEnum.File) {
                lock (lockObject) {
                    if (LogCache != null)
                        File.AppendAllLines(LogFile, LogCache);
                    LogCache = new List<string>();
                }
            }
        }

        static bool WriteInProgess = false;

        public void WriteToLogFile(LevelEnum level, int relStack, string text) {

            if (WriteInProgess) return;
            WriteInProgess = true;

            if (level != LevelEnum.Info)
                text += "\n" + GetCallStack(relStack + 1);
            text = text.Truncate(2000); // limit max text

            string moduleName;
            int siteIdentity = 0;

            try {

                int userId = 0;
                string userName = "";
                string ipAddress = "";
                string referrer = "";
                string requestedUrl = "";
                if (HaveManager) {
                    if (Manager.HaveCurrentSite)
                        siteIdentity = Manager.CurrentSite.Identity;
                    userId = Manager.UserId;
                    userName = Manager.UserName ?? "";
                    userName = userName.Truncate(Globals.MaxUser);
                    if (Manager.HaveCurrentRequest) {
                        requestedUrl = Manager.CurrentRequest.Url != null ? Manager.CurrentRequest.Url.ToString() : "";
                        requestedUrl = requestedUrl.Truncate(Globals.MaxUrl);
                        referrer = Manager.CurrentRequest.UrlReferrer != null ? Manager.CurrentRequest.UrlReferrer.ToString() : "";
                        referrer = referrer.Truncate(Globals.MaxUrl);
                        ipAddress = Manager.UserHostAddress ?? "";
                        ipAddress = ipAddress.Truncate(Globals.MaxIP);
                    }
                } else if (HttpContext.Current.Request != null) {
                    // We don't have a Manager for certain log records (particularly during startup)
                    HttpRequest req = HttpContext.Current.Request;
                    requestedUrl = req.Url != null ? req.Url.ToString() : "";
                    requestedUrl = requestedUrl.Truncate(Globals.MaxUrl);
                    referrer = req.UrlReferrer != null ? req.UrlReferrer.ToString() : "";
                    referrer = referrer.Truncate(Globals.MaxUrl);
                    ipAddress = req.UserHostAddress ?? "";
                    ipAddress = ipAddress.Truncate(Globals.MaxIP);
                }

                MethodBase methBase = GetCallInfo(relStack + 1, out moduleName);

                switch (IOMode) {
                    default:
                        throw new InternalError("IOMode undetermined - this means we don't have a valid data provider");
                    case WebConfigHelper.IOModeEnum.File:
                        text = string.Format("{0}-{1}-{2}-{3}-{4}({5})-{6}: {7},{8},{9},{10} - {11}:{12}",
                            DateTime.Now/*Local Time*/, siteIdentity, ipAddress, requestedUrl, userName, userId, referrer,
                                moduleName,
                                (methBase.DeclaringType != null) ? methBase.DeclaringType.Name : "",
                                methBase.Name,
                                (methBase.DeclaringType != null) ? methBase.DeclaringType.Namespace : "",
                                level, text);
                        text = text.Replace("\n", "\r\n");
                        lock (lockObject) {
                            LogCache.Add(text);
                            if (LogCache.Count >= MAXRECORDS)
                                Flush();
                        }
                        break;
                    case WebConfigHelper.IOModeEnum.Sql:
                        LogRecord record = new LogRecord {
                            Level = level,
                            Info = text,
                            TimeStamp = DateTime.UtcNow,
                            ModuleName = moduleName,
                            Class = (methBase.DeclaringType != null) ? methBase.DeclaringType.Name : "",
                            Method = methBase.Name,
                            Namespace = (methBase.DeclaringType != null) ? methBase.DeclaringType.Namespace : "",
                            SiteIdentity = siteIdentity,
                            UserId = userId,
                            UserName = userName,
                            IPAddress = ipAddress,
                            RequestedUrl = requestedUrl,
                            ReferrerUrl = referrer,
                        };
                        DataProvider.Add(record);
                        break;
                }
                if (Callbacks != null) {
                    foreach (Action<string> callback in Callbacks) {
                        callback(text);
                    }
                }
            } catch (Exception) { }

            WriteInProgess = false;
        }

        public void RegisterCallback(Action<string> callback) {
            if (Callbacks == null) Callbacks = new List<Action<string>>();
            Callbacks.Add(callback);
        }
        public LogRecord GetItem(int key) {
            Flush();
            if (IOMode == WebConfigHelper.IOModeEnum.File) throw new InternalError("Not supported for File I/O");
            return DataProvider.Get(key);
        }
        public bool RemoveItem(int key) {
            Flush();
            if (IOMode == WebConfigHelper.IOModeEnum.File) throw new InternalError("Not supported for File I/O");
            return DataProvider.Remove(key);
        }

        public List<LogRecord> GetItems(List<DataProviderFilterInfo> filters) {
            Flush();
            if (IOMode == WebConfigHelper.IOModeEnum.File) throw new InternalError("Not supported for File I/O");
            int total;
            return DataProvider.GetRecords(0, 0, null, filters, out total);
        }
        public List<LogRecord> GetItems(int skip, int take, List<DataProviderSortInfo> sort, List<DataProviderFilterInfo> filters, out int total) {
            Flush();
            if (IOMode == WebConfigHelper.IOModeEnum.File) throw new InternalError("Not supported for File I/O");
            return DataProvider.GetRecords(skip, take, sort, filters, out total);
        }
        public int RemoveItems(List<DataProviderFilterInfo> filters) {
            Flush();
            if (IOMode == WebConfigHelper.IOModeEnum.File) throw new InternalError("Not supported for File I/O");
            return DataProvider.RemoveRecords(filters);
        }

        private static MethodBase GetCallInfo(int level, out string moduleName) {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(level + 2);
            MethodBase methodBase = stackFrame.GetMethod();
            moduleName = "(core)";
            for (int lvl = level + 1 ; lvl < stackTrace.FrameCount ; ++lvl) {
                stackFrame = stackTrace.GetFrame(lvl);
                MethodBase mb = stackFrame.GetMethod();
                if (mb.DeclaringType != null) {
                    string name = mb.DeclaringType.FullName;
                    string[] s = name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length >= 4) {
                        if (string.Compare(s[1], "Modules", true) == 0) {
                            moduleName = methodBase.Module.Name;
                            if (moduleName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                                moduleName = moduleName.Substring(0, moduleName.Length - 4);
                            return methodBase;
                        }
                    }
                }
            }
            return methodBase;
        }

        private static string GetCallStack(int level) {
            StringBuilder sb = new StringBuilder();
            StackTrace stackTrace = new StackTrace();
            for (int lvl = level+2 ; lvl < stackTrace.FrameCount ; ++lvl) {
                StackFrame stackFrame = stackTrace.GetFrame(lvl);
                MethodBase methBase = stackFrame.GetMethod();
                if (methBase.DeclaringType != null) {
                    sb.AppendFormat(" - {0} {1}", methBase.DeclaringType.Namespace, methBase.DeclaringType.Name);
                } else {
                    sb.Append(" - ?");
                }
            }
            return sb.ToString();
        }

        public bool CanBrowse {
            get {
                return CanImportOrExport;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool CanImportOrExport {
            get {
                IDataProvider<int, LogRecord> providef = DataProvider;// to evaluate IOMode
                if (IOMode == WebConfigHelper.IOModeEnum.Determine)
                    throw new InternalError("unexpected IOMode");
                return base.IOMode == WebConfigHelper.IOModeEnum.Sql;
            }
        }
        public string GetLogFileName() {
            IDataProvider<int, LogRecord> providef = DataProvider;// to evaluate IOMode
            if (IOMode != WebConfigHelper.IOModeEnum.File)
                throw new InternalError("Not supported for current I/O mode");
            return LogFile;
        }

        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL
        // IINSTALLABLEMODEL

        public bool IsInstalled() {
            return DataProvider.IsInstalled();
        }
        public bool InstallModel(List<string> errorList) {
            bool success = DataProvider.InstallModel(errorList);
            if (success)
                YetaWF.Core.Log.Logging.SetupLogging();
            return success;
        }
        public bool UninstallModel(List<string> errorList) {
            YetaWF.Core.Log.Logging.TerminateLogging();
            return DataProvider.UninstallModel(errorList);
        }
        public void AddSiteData() {
            DataProvider.AddSiteData();
        }
        public void RemoveSiteData() {
            DataProvider.RemoveSiteData();
        }
        public bool ExportChunk(int chunk, SerializableList<SerializableFile> fileList, out object obj) {
            // we're not exporting any data
            //if (CanImportOrExport)
            //    return DataProvider.ExportChunk(chunk, fileList, out obj);
            //else {
            obj = null;
            return false;
            //}
        }
        public void ImportChunk(int chunk, SerializableList<SerializableFile> fileList, object obj) {
            // we're not importing any data
            //if (CanImportOrExport)
            //    DataProvider.ImportChunk(chunk, fileList, obj);
        }
    }
}