﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/CurrencyConverter#License */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using YetaWF.Core.Addons;
using YetaWF.Core.DataProvider;
using YetaWF.Core.DataProvider.Attributes;
using YetaWF.Core.IO;
using YetaWF.Core.Packages;
using YetaWF.Core.Serializers;
using YetaWF.Core.Support;
using YetaWF.Modules.CurrencyConverter.Controllers;

namespace YetaWF.Modules.CurrencyConverter.DataProvider {

    public class ExchangeRateEntry {
        public string CurrencyName { get; set; }
        public string Code { get; set; }
        public decimal Rate { get; set; }
    }

    public class ExchangeRateData {

#if DEBUG
        public static readonly TimeSpan ExpiresAfter = new TimeSpan(8, 0, 0); // 8 hours (1 month = ~180 requests max)
#else
        public static readonly TimeSpan ExpiresAfter = new TimeSpan(0, 5, 0); // 5 minutes (1 month = ~18000 requests max)
#endif
        [Data_PrimaryKey]
        public int Key { get; set; }
        public DateTime SaveTime { get; set; }

        public SerializableList<ExchangeRateEntry> Rates { get; set; }

        public ExchangeRateData() {
            Rates = new SerializableList<ExchangeRateEntry>();
        }
    }

    public class ExchangeRateDataProvider : DataProviderImpl, IInstallableModel {

        // IMPLEMENTATION
        // IMPLEMENTATION
        // IMPLEMENTATION

        public const int KEY = 1000;
        public const string JSFile = "ExchangeRates.js";

        private static object LockObject = new object();

        public ExchangeRateDataProvider() : base(YetaWFManager.Manager.CurrentSite.Identity) { SetDataProvider(DataProvider); }

        private IDataProvider<int, ExchangeRateData> DataProvider {
            get {
                if (_dataProvider == null) {
                    _dataProvider = new YetaWF.DataProvider.FileDataProvider<int, ExchangeRateData>(
                        Path.Combine(YetaWFManager.DataFolder, AreaRegistration.CurrentPackage.AreaName),
                        Cacheable: true);
                }
                return _dataProvider;
            }
        }
        private IDataProvider<int, ExchangeRateData> _dataProvider { get; set; }

        // LOAD/SAVE
        // LOAD/SAVE
        // LOAD/SAVE

        public ExchangeRateData GetItem() {
            lock (LockObject) {
                ExchangeRateData data = DataProvider.Get(KEY);
                if (data != null && data.SaveTime.Add(ExchangeRateData.ExpiresAfter) < DateTime.UtcNow)
                    data = null;
                if (data != null && !File.Exists(GetJSFileName()))
                    data = null;
                if (data == null)
                    data = GetExchangeRates();
                return data;
            }
        }
        private ExchangeRateData GetExchangeRates() {

            ConfigData config = ConfigDataProvider.GetConfig();
            if (string.IsNullOrWhiteSpace(config.AppID))
                throw new InternalError("The App ID has not been specified in the Currency Converter Settings (see Admin > Configuration > Currency Converter Settings) - openexchangerates.org requires an app id to be able to retrieve currency exchange rates");

            ExchangeRateData data = new ExchangeRateData();
            data.Key = KEY;
            data.SaveTime = DateTime.UtcNow;

            string url = string.Format("{0}://openexchangerates.org/api/latest.json?app_id={1}", config.UseHttps ? "https" : "http", config.AppID);
            string json = GetJSONResponse(url);
            CheckForErrors(json);

            url = string.Format("{0}://openexchangerates.org/api/currencies.json?app_id={1}", config.UseHttps ? "https" : "http", config.AppID);
            string jsonCurrencies = GetJSONResponse(url);
            CheckForErrors(jsonCurrencies);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            // get all currencies
            Dictionary<string, object> currencies = serializer.Deserialize<Dictionary<string, object>>(jsonCurrencies);
            // add all rates
            var jsonObject = serializer.Deserialize<dynamic>(json);
            var rates = jsonObject["rates"];
            foreach (var rate in rates) {
                string code = rate.Key;
                object currency;
                if (!currencies.TryGetValue(code, out currency))// replace 3 digit codes by actual name
                    currency = code;
                decimal val = (decimal) rate.Value;
                data.Rates.Add(new ExchangeRateEntry { Code = code, CurrencyName = (string) currency, Rate = val });
            }
            // Save new rates
            UpdateStatusEnum status = DataProvider.Update(KEY, KEY, data);
            if (status != UpdateStatusEnum.OK) {
                if (status != UpdateStatusEnum.RecordDeleted)
                    throw new InternalError("Unexpected status {0}", status);
                if (!DataProvider.Add(data))
                    throw new InternalError("Unexpected error adding data");
            }
            // Create a javascript file with rates so we can include it in a page
            SaveRatesJS(data);
            return data;
        }

        private void SaveRatesJS(ExchangeRateData data) {
            string file = GetJSFileName();
            ScriptBuilder sb = new ScriptBuilder();
            sb.Append("// Generated file (see ExchangeRateDataProvider) - Do not modify\n");
            sb.Append("YetaWF_CurrencyConverter_Rates = \n");
            sb.Append(YetaWFManager.Jser.Serialize(data.Rates));
            sb.Append(";\n");
            File.WriteAllText(file, sb.ToString());
        }

        private static string GetJSFileName() {
            VersionManager.AddOnProduct addon = VersionManager.FindModuleVersion(AreaRegistration.CurrentPackage.Domain, AreaRegistration.CurrentPackage.Product);
            string url = addon.GetAddOnUrl();
            string path = YetaWFManager.UrlToPhysical(url);
            return Path.Combine(path, JSFile);
        }

        private void CheckForErrors(string json) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var jsonObject = serializer.Deserialize<dynamic>(json);
            if (jsonObject.ContainsKey("error"))
                throw new InternalError("An error occurred retrieving exchange rates from openexchangerates.org - {0}: {1}", jsonObject["message"], jsonObject["description"]);
        }
        private string GetJSONResponse(string url) {
            var http = (HttpWebRequest) WebRequest.Create(new Uri(url));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";
            System.Net.WebResponse resp;
            try {
                resp = http.GetResponse();
            } catch (Exception exc) {
                throw new InternalError("An error occurred retrieving exchange rates from openexchangerates.org - {0}", exc.Message);
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
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
        public bool ExportChunk(int chunk, SerializableList<SerializableFile> fileList, out object obj) { obj = null; return false; }
        public void ImportChunk(int chunk, SerializableList<SerializableFile> fileList, object obj) { }
    }
}