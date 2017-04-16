﻿/* Copyright © 2017 Softel vdm, Inc. - https://yetawf.com/Documentation/YetaWF/Scheduler#License */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YetaWF.Core.DataProvider;
using YetaWF.Core.Models;
using YetaWF.Core.Packages;
using YetaWF.Core.Scheduler;
using YetaWF.Core.Support;
using YetaWF.Modules.Scheduler.DataProvider;

namespace YetaWF.Modules.Scheduler.Support {
    public partial class Scheduler {

        // INSTALL/UNINSTALL
        // INSTALL/UNINSTALL
        // INSTALL/UNINSTALL

        /// <summary>
        /// Install all events for the given package. This is typically used to install scheduler items while installing packages.
        /// </summary>
        public void InstallItems(Package package) {
            Type[] typesInAsm;
            try {
                typesInAsm = package.PackageAssembly.GetTypes();
            } catch (ReflectionTypeLoadException ex) {
                typesInAsm = ex.Types;
            }
            List<Type> types = typesInAsm.Where(type => IsSchedulerEventType(type)).ToList<Type>();
            foreach (var type in types)
                InstallItems(type);
        }
        private bool IsSchedulerEventType(Type type) {
            if (!TypeIsPublicClass(type))
                return false;
            return typeof(IScheduling).IsAssignableFrom(type);
        }
        private bool TypeIsPublicClass(Type type) {
            return (type != null && type.IsPublic && type.IsClass && !type.IsAbstract);
        }

        /// <summary>
        /// Install all events for the given object type. This is typically used to install scheduler items while installing packages.
        /// </summary>
        /// <param name="container"></param>
        private void InstallItems(Type type) {
            string eventType = type.FullName + ", " + type.Assembly.GetName().Name;
            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {
                IScheduling schedEvt;
                try {
                    schedEvt = (IScheduling) Activator.CreateInstance(type);
                } catch (Exception exc) {
                    throw new InternalError("The specified object does not support the required IScheduling interface.", exc);
                }
                try {
                    SchedulerItemBase[] items = schedEvt.GetItems();
                    foreach (var item in items) {
                        SchedulerItemData evnt = new SchedulerItemData();
                        ObjectSupport.CopyData(item, evnt);
                        evnt.Event.Name = item.EventName;
                        evnt.Event.ImplementingAssembly = type.Assembly.GetName().Name;
                        evnt.Event.ImplementingType = type.FullName;
                        dataProvider.AddItem(evnt);// we ignore whether the add fails - it's OK if it already exists
                    }
                } catch (Exception exc) {
                    throw new InternalError("InstallEvents for the specified type {0} failed.", eventType, exc);
                }
            }
        }

        /// <summary>
        /// Uninstall all events for the given package. This is typically used to uninstall scheduler items while uninstalling packages.
        /// </summary>
        public void UninstallItems(Package package) {
            Type[] typesInAsm;
            try {
                typesInAsm = package.PackageAssembly.GetTypes();
            } catch (ReflectionTypeLoadException ex) {
                typesInAsm = ex.Types;
            }
            List<Type> types = typesInAsm.Where(type => IsSchedulerEventType(type)).ToList<Type>();
            foreach (var type in types)
                UninstallItems(type);
        }
        /// <summary>
        /// Uninstall all events for the given object type. This is typically used to uninstall scheduler items while removing packages.
        /// </summary>
        /// <param name="container"></param>
        private void UninstallItems(Type type) {
            string asmName = type.Assembly.GetName().Name;
            string eventType = type.FullName + ", " + asmName;
            using (SchedulerDataProvider dataProvider = new SchedulerDataProvider()) {
                IScheduling schedEvt = null;
                try {
                    schedEvt = (IScheduling) Activator.CreateInstance(type);
                } catch (Exception exc) {
                    throw new InternalError("The specified object does not support the required IScheduling interface.", exc);
                }
                if (schedEvt != null) {
                    try {
                        // Event.ImplementingAssembly == asmName
                        List<DataProviderFilterInfo> filters = new List<DataProviderFilterInfo> {
                            new DataProviderFilterInfo {
                                Field = "Event.ImplementingAssembly", Operator = "==", Value = asmName
                            }
                        };
                        dataProvider.RemoveItems(filters);// we ignore whether the remove fails
                    } catch (Exception exc) {
                        throw new InternalError("UninstallItems for the specified type {0} failed.", eventType, exc);
                    }
                }
            }
        }
    }
}