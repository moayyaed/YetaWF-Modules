﻿/* Copyright © 2016 Softel vdm, Inc. - http://yetawf.com/Documentation/YetaWF/Scheduler#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using YetaWF.Core.DataProvider;
using YetaWF.Core.IO;
using YetaWF.Core.Localize;
using YetaWF.Core.Log;
using YetaWF.Core.Packages;
using YetaWF.Core.Scheduler;
using YetaWF.Core.Site;
using YetaWF.Core.Support;
using YetaWF.Modules.Scheduler.DataProvider;

namespace YetaWF.Modules.Scheduler.Support {

    public partial class Scheduler : IInitializeApplicationStartup {

        internal static Scheduler Instance = null;

        public void InitializeApplicationStartup() {
            Instance = this;
            SchedulerSupport.Install = InstallItems;
            SchedulerSupport.Uninstall = UninstallItems;
            SchedulerSupport.RunItem = RunItem;

            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {
                SchedulerSupport.Enabled = dataProvider.GetRunning();
            }
            if (SchedulerSupport.Enabled)
                Initialize();// start task
        }

        /// <summary>
        /// Initializes scheduling system-wide, called during application startup.
        /// </summary>
        private void Initialize() {
            List<Type> items = SchedulerEvents; // evaluate to cache available scheduler events
            schedulingThread = new Thread(new ThreadStart(Execute));
            schedulingThread.Start();
        }

        /// SCHEDULER ITEMS
        /// SCHEDULER ITEMS
        /// SCHEDULER ITEMS

        /// <summary>
        /// Returns a list of types that implement scheduler events.
        /// </summary>
        public List<Type> SchedulerEvents {
            get {
                if (_schedulerEvents == null) {
                    IEnumerable<Type> evtTypes = FilterTypes();
                    _schedulerEvents = evtTypes.ToList<Type>();
                }
                return _schedulerEvents;
            }
        }
        private static List<Type> _schedulerEvents;

        private IEnumerable<Type> FilterTypes() {
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            foreach (Package package in Package.GetAvailablePackages()) {
                Assembly assembly = package.PackageAssembly;
                Type[] typesInAsm;
                try {
                    typesInAsm = assembly.GetTypes();
                } catch (ReflectionTypeLoadException ex) {
                    typesInAsm = ex.Types;
                }
                typesSoFar = typesSoFar.Concat(typesInAsm);
            }
            return typesSoFar.Where(type => IsSchedulerEventType(type));
        }

        // SCHEDULER
        // SCHEDULER
        // SCHEDULER

        /// <summary>
        /// Run a scheduler item.
        /// </summary>
        /// <param name="name"></param>
        public void RunItem(string name) {

            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {

                StringLocks.DoAction("YetaWF_Scheduler##Scheduler_" + name, () => {

                    SchedulerItemData evnt = dataProvider.GetItem(name);
                    if (evnt == null)
                        throw new Error(this.__ResStr("errItemNotFound", "Scheduler item '{0}' does not exist."), name);
                    if (evnt.RunOnce)
                        evnt.Enabled = true;
                    if (!evnt.Enabled)
                        throw new Error(this.__ResStr("errItemDisabled", "Scheduler item '{0}' is currently disabled and cannot be scheduled."), evnt.Name);
                    evnt.Next = DateTime.UtcNow.AddSeconds(-1);
                    evnt.Errors = null;
                    UpdateStatusEnum status = dataProvider.UpdateItem(evnt);
                    if (status != UpdateStatusEnum.OK)
                        throw new Error(this.__ResStr("errItemUpdFail", "Scheduler item '{0}' couldn't be updated."), evnt.Name);

                    Dispatch();// run the scheduler now
                });
            }
        }

        /// <summary>
        /// Run the scheduler (wake it from waiting)
        /// </summary>
        public void Dispatch() {
            if (schedulingThread != null)
                schedulingThread.Interrupt();
        }

        private Thread schedulingThread;
#if DEBUG
        private TimeSpan defaultTimeSpan = new TimeSpan(0, 0, 60); // 60 seconds (quicker in debug, for better debuggability)
        private TimeSpan defaultStartupTimeSpan = new TimeSpan(0, 0, 30); // 30 seconds
#else
        private TimeSpan defaultTimeSpan = new TimeSpan(0, 2, 0); // 2 minutes
        private TimeSpan defaultStartupTimeSpan = new TimeSpan(0, 0, 30); // 30 seconds
#endif

        private void Execute() {

            // get a manager for the scheduler
            YetaWFManager.MakeInitialThreadInstance(null);

            Logging.AddLog("Scheduler task started");

            // Because initialization is called during application startup, we'll wait before we
            // check for any schedule items that may be due (just so app start isn't all too slow).
            try {
                Thread.Sleep(defaultStartupTimeSpan);
            } catch (ThreadInterruptedException) {
                // thread was interrupted because there is work to be done
            }

            // run all scheduled items that are supposed to be run at application startup
            try {
                RunStartupItems();
            } catch (Exception exc) {
                Logging.AddErrorLog("An error occurred running startup items", exc);
            }

            for ( ; ; ) {
                TimeSpan delayTime = new TimeSpan(1, 0, 0);// 1 minute
                if (SchedulerSupport.Enabled) {
                    try {
                        delayTime = RunItems();
                    } catch (Exception exc) {
                        delayTime = defaultTimeSpan;
                        Logging.AddErrorLog("An error occurred in the scheduling loop.", exc);
                    }
                    if (delayTime < new TimeSpan(0, 0, 10))// at least 10 seconds
                        delayTime = new TimeSpan(0, 0, 10);
                }
                try {
                    Thread.Sleep(delayTime);
                } catch (ThreadInterruptedException) {
                    // thread was interrupted because there is work to be done
                } catch (ThreadAbortException) { }
            }
        }

        private void RunStartupItems() {

            Logging.AddLog("Scheduler event - checking startup scheduler items");

            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {
                if (!dataProvider.IsInstalled()) return;

                // reset any items that are still marked as running : Next >= DateTime.MaxValue
                List<DataProviderFilterInfo> filters = new List<DataProviderFilterInfo> {
                    new DataProviderFilterInfo {
                        Field = "Next", Operator = ">=", Value = DateTime.MaxValue,
                    },
                };
                List<SchedulerItemData> list = dataProvider.GetItems(filters);
                foreach (var item in list) {
                    item.SetNextRuntime();
                    UpdateStatusEnum status = dataProvider.UpdateItem(item);
                    if (status != UpdateStatusEnum.OK)
                        throw new Error(this.__ResStr("errUpdate", "Failed to update scheduler item {0} during startup to reset its next run time ({1})"), item.Name, status);
                }

                if (SchedulerSupport.Enabled) {
                    // enable all startup items : "EnableOnStartup == true and Enabled == false"
                    filters = new List<DataProviderFilterInfo> {
                        new DataProviderFilterInfo {
                            Logic = "&&",
                            Filters = new List<DataProviderFilterInfo> {
                                new DataProviderFilterInfo {
                                    Field = "EnableOnStartup", Operator = "==", Value = true,
                                },
                                new DataProviderFilterInfo {
                                    Field = "Enabled", Operator = "==", Value = false,
                                },
                             },
                        }
                    };
                    list = dataProvider.GetItems(filters);
                    foreach (var item in list) {
                        item.Enabled = true;
                        item.SetNextRuntime();
                        UpdateStatusEnum status = dataProvider.UpdateItem(item);
                        if (status != UpdateStatusEnum.OK)
                            throw new Error(this.__ResStr("errUpdateEnable", "Failed to update scheduler item {0} during startup to enable it ({1})"), item.Name, status);
                    }

                    // run all enabled startup items : Startup == true and Enabled == true
                    filters = new List<DataProviderFilterInfo> {
                        new DataProviderFilterInfo {
                            Logic = "&&",
                            Filters = new List<DataProviderFilterInfo> {
                                new DataProviderFilterInfo {
                                    Field = "Startup", Operator = "==", Value = true,
                                },
                                new DataProviderFilterInfo {
                                    Field = "Enabled", Operator = "==", Value = true,
                                },
                             },
                        }
                    };
                    DateTime next = DateTime.UtcNow.Add(defaultTimeSpan);
                    list = dataProvider.GetItems(filters);
                    foreach (var item in list) {
                        RunItem(dataProvider, item);
                        // check if we have to start back up before the default timespan elapses
                        if (item.Next != null && (((DateTime)item.Next) > DateTime.UtcNow && next > item.Next))
                            next = (DateTime) item.Next;
                    }
                }
            }
        }

        private TimeSpan RunItems() {

            Logging.AddLog("Scheduler event - checking scheduler items");

            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {

                DateTime next = DateTime.UtcNow.Add(defaultTimeSpan);
                if (dataProvider.IsInstalled()) {
                    // Enabled == true and Next != null and Next < DateTime.UtcNow
                    List<DataProviderFilterInfo> filters = new List<DataProviderFilterInfo> {
                        new DataProviderFilterInfo {
                            Logic = "&&",
                            Filters = new List<DataProviderFilterInfo> {
                                new DataProviderFilterInfo {
                                    Field = "Enabled", Operator = "==", Value = true,
                                },
                                new DataProviderFilterInfo {
                                    Field = "Next", Operator = "!=", Value = null,
                                },
                                new DataProviderFilterInfo {
                                    Field = "Next", Operator = "<", Value = DateTime.UtcNow,
                                },
                             },
                        }
                    };
                    List<SchedulerItemData> list = dataProvider.GetItems(filters);
                    foreach (var item in list) {
                        RunItem(dataProvider, item);
                        // check if we have to start back up before the default timespan elapses
                        if (item.Next != null && ((DateTime)item.Next) > DateTime.UtcNow && next > item.Next)
                            next = (DateTime) item.Next;
                    }
                }
                return next.Subtract(DateTime.UtcNow);
            }
        }

        private void RunItem(SchedulerDataProvider dataProvider, SchedulerItemData item) {

            item.IsRunning = true;
            item.RunTime = new TimeSpan();
            item.Last = DateTime.UtcNow;

            try {
                dataProvider.UpdateItem(item);
            } catch (Exception exc) {
                Logging.AddErrorLog("Updating scheduler item {0} failed.", item.Name, exc);
            }

            StringBuilder errors = new StringBuilder();

            try {
                item.Errors = null;

                DateTime now = DateTime.UtcNow;
                errors.AppendLine(Logging.AddLog("Scheduler event - running scheduler item '{0}'.", item.Name));

                Type tp = null;
                try {
                    Assembly asm = Assemblies.Load(item.Event.ImplementingAssembly);
                    tp = asm.GetType(item.Event.ImplementingType);
                } catch (Exception exc) {
                    throw new InternalError("Scheduler item '{0}' could not be loaded (Type={1}, Assembly={2}) - {3}", item.Name, item.Event.ImplementingType, item.Event.ImplementingAssembly, exc.Message);
                }

                IScheduling schedEvt = null;
                try {
                    schedEvt = (IScheduling) Activator.CreateInstance(tp);
                } catch (Exception exc) {
                    throw new InternalError("Scheduler item '{0}' could not be instantiated (Type={1}, Assembly={2}) - {3}", item.Name, item.Event.ImplementingType, item.Event.ImplementingAssembly, exc.Message);
                }

                try {
                    if (item.SiteSpecific) {
                        SiteDefinition.SitesInfo info = SiteDefinition.GetSites(0, 0, null, null);
                        foreach (SiteDefinition site in info.Sites) {
                            YetaWFManager.MakeThreadInstance(site);// set up a manager for the site
                            SchedulerItemBase itemBase = new SchedulerItemBase { Name = item.Name, Description = item.Description, EventName = item.Event.Name, Enabled = true, Frequency = item.Frequency, Startup = item.Startup, SiteSpecific = true };
                            schedEvt.RunItem(itemBase);
                            foreach (var s in itemBase.Log)
                                errors.AppendLine(Logging.AddErrorLog("{0}: {1}", site.Identity, s));
                        }
                        YetaWFManager.MakeThreadInstance(null);// restore scheduler's manager
                    } else {
                        SchedulerItemBase itemBase = new SchedulerItemBase { Name = item.Name, Description = item.Description, EventName = item.Event.Name, Enabled = true, Frequency = item.Frequency, Startup = item.Startup, SiteSpecific = false };
                        schedEvt.RunItem(itemBase);
                        foreach (var s in itemBase.Log)
                            errors.AppendLine(Logging.AddErrorLog(s));
                    }
                } catch (Exception exc) {
                    throw new InternalError("An error occurred in scheduler item '{0}' - {1}", item.Name, exc.Message);
                }

                TimeSpan diff = DateTime.UtcNow - now;
                item.RunTime = diff;
                errors.AppendLine(Logging.AddErrorLog("Schedule item '{0}' took {1} milliseconds.", item.Name, diff));

            } catch (Exception exc) {
                errors.AppendLine(Logging.AddErrorLog("Scheduler item {0} failed.", item.Name, exc));
            }

            if (item.RunOnce)
                item.Enabled = false;

            item.IsRunning = false;

            item.Errors = errors.ToString();

            try {
                dataProvider.UpdateItem(item);
            } catch (Exception exc) {
                Logging.AddErrorLog("Updating scheduler item {0} failed.", item.Name, exc);
            }
        }
    }
}