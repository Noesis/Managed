using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Noesis;

namespace NoesisApp
{
    internal class LibUdev
    {

        #region Imports
        [DllImport("libudev.so.1", EntryPoint = "udev_new")]
        internal static extern IntPtr New();

        [DllImport("libudev.so.1", EntryPoint = "udev_unref")]
        internal static extern IntPtr Unref(IntPtr udev);

        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_new")]
        internal static extern IntPtr EnumerateNew(IntPtr udev);

        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_ref")]
        internal static extern IntPtr EnumerateRef(IntPtr udev);

        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_unref")]
        internal static extern IntPtr EnumerateUnref(IntPtr udev);

        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_add_match_subsystem")]
        internal static extern int EnumerateAddMatchSubsystem(IntPtr udev, string subsystem);
        
        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_scan_devices")]
        internal static extern int EnumerateScanDevices(IntPtr udev_enumerate);

        [DllImport("libudev.so.1", EntryPoint = "udev_enumerate_get_list_entry")]
        internal static extern IntPtr EnumerateGetListEntry(IntPtr udev_enumerate);

        [DllImport("libudev.so.1", EntryPoint = "udev_list_entry_get_next")]
        internal static extern IntPtr ListEntryGetNext(IntPtr list_entry);

        [DllImport("libudev.so.1", EntryPoint = "udev_list_entry_get_name")]
        internal static extern IntPtr ListEntryGetName(IntPtr list_entry);

        [DllImport("libudev.so.1", EntryPoint = "udev_device_new_from_syspath")]
        internal static extern IntPtr DeviceNewFromSyspath(IntPtr udev, IntPtr syspath);

        [DllImport("libudev.so.1", EntryPoint = "udev_device_unref")]
        internal static extern IntPtr DeviceUnref(IntPtr udev_device);

        [DllImport("libudev.so.1", EntryPoint = "udev_device_get_devnode")]
        internal static extern IntPtr DeviceGetDevnode(IntPtr udev_device);

        [DllImport("libudev.so.1", EntryPoint = "udev_device_get_action")]
        internal static extern IntPtr DeviceGetAction(IntPtr udev_device);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_new_from_netlink")]
        internal static extern IntPtr MonitorNewFromNetlink(IntPtr udev, string name);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_unref")]
        internal static extern IntPtr MonitorUnref(IntPtr udev_monitor);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_get_fd")]
        internal static extern int MonitorGetFd(IntPtr udev_monitor);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_filter_add_match_subsystem_devtype")]
        internal static extern IntPtr MonitorFilterAddMatchSubsystemDevtype(IntPtr udev_monitor, string subsystem, string devtype);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_enable_receiving")]
        internal static extern int MonitorEnableReceiving(IntPtr udev_monitor);

        [DllImport("libudev.so.1", EntryPoint = "udev_monitor_receive_device")]
        internal static extern IntPtr MonitorReceiveDevice(IntPtr udev_monitor);
        #endregion
    }
}