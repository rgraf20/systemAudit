using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetMachineInfoFinal
{
    /// <summary>
    /// Constants de la aplicació.
    /// </summary>
    
    class AppConstants
    {
        //SYSTEM:
        public const String SYSTEM_CLASS = "system";
        public static String[] PROP_SYSTEM = new String[] { "mac", "status" };

        //MOTHERBOARD:
        public const String MOTHERBOARD_CLASS = "Win32_BaseBoard";
        public static String[] PROP_MOTHERBOARD = new String[] { "SerialNumber", "Manufacturer", "Model" };

        //BIOS:
        public const String BIOS_CLASS = "Win32_BIOS";
        public static String[] PROP_BIOS = new String[] {"SerialNumber", "Manufacturer", "Name"};

        //MEMORY:
        public const String MEMORY_CLASS = "Win32_PhysicalMemory";
        public static String[] PROP_MEMORY = new String[] { "SerialNumber", "Description", "Capacity"};

        //VIDEO:
        public const String VIDEO_CLASS = "Win32_VideoController";
        public static String[] PROP_VIDEO = new String[] { "DeviceID", "AdapterCompatibility", "Description", "AdapterRAM" };

        //PROCESSOR:
        public const String PROCESSOR_CLASS = "Win32_Processor";
        public static String[] PROP_PROCESSOR = new String[] { "ProcessorId", "Manufacturer", "Name", "Description", "MaxClockSpeed", "NumberOfCores", "NumberOfLogicalProcessors" };

        //NETWORK:
        public const String NETWORK_CLASS = "Win32_NetworkAdapter";
        public static String[] PROP_NETWORK = new String[] { "DeviceID", "Manufacturer", "Description", "ip", "mask", "DefaultIPGateway", "broadcasting", "dns" };

        public const String NETWORK_CLASS_CONFIGURATION = "Win32_NetworkAdapterConfiguration";
        public static String[] PROP_NETWORK_CONFIGURATION = new String[] { };

        //DISK:
        public const String DISK_CLASS = "Win32_DiskDrive";
        public static String[] PROP_DISK = new String[] { "SerialNumber", "DeviceID", "Manufacturer", "Model", "FileSystem", "Size", "FreeSpace" };

        //COMPUTER SYSTEM:
        public const String COMPUTER_SYSTEM_CLASS = "Win32_ComputerSystem";
        public static String[] COMPUTER_SYSTEM_CLASS_CONFIGURATION = new String[] { };

        //OPERATING SYSTEM:
        public const String OPERATING_SYSTEM_CLASS = "Win32_OperatingSystem";
        public static String[] OPERATING_SYSTEM_CLASS_CONFIGURATION = new String[] { "SerialNumber", "Caption", "Version", "hostname", "workgroup", "RegisteredUser", "RegisteredPassword","CountryCode" };

        //SW:
        public const String PROGRAM = "program";
        public static String[] SW_CONFIGURATION = new String[] { "IdentifyingNumber", "Name", "Version", "Vendor", "InstallLocation", "URLInfoAbout" };


        //XML TAGS DICTIONARY:
        public static Dictionary<String, String> DICT_TAGS = new Dictionary<String, String>() 
        {
            {
                SYSTEM_CLASS, SYSTEM_CLASS
            },
            {
                MOTHERBOARD_CLASS, "motherboard"
            }, 
            {
                BIOS_CLASS, "bios" 
            }, 
            {
                MEMORY_CLASS, "memory"
            }, 
            {
                VIDEO_CLASS, "video"
            },
            {
                PROCESSOR_CLASS, "processor"
            }, 
            {
                NETWORK_CLASS, "network"
            }, 
            {
                DISK_CLASS, "disk"
            },
            {
                OPERATING_SYSTEM_CLASS, "windows"
            },
            {
                PROGRAM, PROGRAM
            }
        };
    }
}
