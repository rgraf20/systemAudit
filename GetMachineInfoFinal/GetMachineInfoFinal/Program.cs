using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//New import.
using System.Management;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GetMachineInfoFinal
{
    /// <summary>
    /// Agafa la informació de la màquina i la envia al servidor.
    /// </summary>
    
    class Program
    {
        //Canviar si és necessari.
        private const String SERVER_URL = "http://192.168.1.132/system_audit/php/controllers/add_system.php";
        static void Main(string[] args)
        {
            //Creem el Writer general.
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            writer.WriteStartElement("components");

            //Creem el XML del system:
            WMI wmiSystem = new WMI(AppConstants.SYSTEM_CLASS, AppConstants.PROP_SYSTEM);
            wmiSystem.DictProp.Add("mac", WMI.FindMAC());
            wmiSystem.DictProp.Add("status", "OFFLINE");
            wmiSystem.SetXML(writer);

            //Creem el XML de la placa mare:
            WMI wmiMotherBoard = new WMI(AppConstants.MOTHERBOARD_CLASS, AppConstants.PROP_MOTHERBOARD);
            wmiMotherBoard.SetXML(writer);

            //Creem el XML de la BIOS:
            WMI wmiBios = new WMI(AppConstants.BIOS_CLASS, AppConstants.PROP_BIOS);
            wmiBios.SetXML(writer);

            //Creem el XML de la memòria.
            WMI wmiMemory = new WMI(AppConstants.MEMORY_CLASS, AppConstants.PROP_MEMORY);
            wmiMemory.SetXML(writer);

            //Creem l'XML del vídeo:
            WMI wmiVideo = new WMI(AppConstants.VIDEO_CLASS, AppConstants.PROP_VIDEO);
            wmiVideo.SetXML(writer);

            //Creem l'XML del processador:
            WMI wmiProcessor = new WMI(AppConstants.PROCESSOR_CLASS, AppConstants.PROP_PROCESSOR);
            wmiProcessor.SetXML(writer);

            //Xarxa (Fem ús dels mètodes estàtics de la classe WMI per a xarxa i de Win32_NetworkAdapter, Win32_NetworkAdapterConfiguration):
            WMI wmiNetwork = new WMI(AppConstants.NETWORK_CLASS, AppConstants.PROP_NETWORK);
            IPAddress ip = WMI.GetIP();
            IPAddress mask = WMI.GetSubnetMask(ip);
            IPAddress broadcast = WMI.GetBroadcastAddress(ip, mask);

            wmiNetwork.DictProp.Add("ip", ip == null ? "N/A" : ip.ToString());
            wmiNetwork.DictProp.Add("mask", mask == null ? "N/A" : mask.ToString());
            wmiNetwork.DictProp.Add("broadcasting", broadcast == null ? "N/A" : broadcast.ToString());

            WMI wmiNetworkProp = new WMI(AppConstants.NETWORK_CLASS_CONFIGURATION, AppConstants.PROP_NETWORK_CONFIGURATION);
            wmiNetwork.DictProp.Add("DefaultIPGateway", wmiNetworkProp.DictProp["DefaultIPGateway"]);
            wmiNetwork.DictProp.Add("dns", WMI.GetDnsAddresses());

            wmiNetwork.SetXML(writer);

            //Creem el XML del disk:
            //Agafem les propietats del drive installation.
            WMI wmiSingleDrive = new WMI(AppConstants.DISK_CLASS, "", AppConstants.PROP_DISK);

            //Agafem les propietats de tot el disk.
            WMI wmiDisk = new WMI(AppConstants.DISK_CLASS, AppConstants.PROP_DISK);
            wmiDisk.DictProp.Remove("DeviceID"); //Eliminem la propietat gran ens quedem amb la del installed drive.
            wmiDisk.DictProp.Remove("Size"); //Eliminem la propietat gran ens quedem amb la del installed drive.
            wmiDisk.DictProp.Add("DeviceID", wmiSingleDrive.DictProp["DeviceID"]);
            wmiDisk.DictProp.Add("FileSystem", wmiSingleDrive.DictProp["FileSystem"]);
            wmiDisk.DictProp.Add("Size", wmiSingleDrive.DictProp["Size"]);
            wmiDisk.DictProp.Add("FreeSpace", wmiSingleDrive.DictProp["FreeSpace"]);
            wmiDisk.SetXML(writer);

            //Sistema operatiu (agafem dades de Win32_ComputerSystem i de Win32_OperatingSystem):
            WMI wmiComputer = new WMI(AppConstants.COMPUTER_SYSTEM_CLASS, AppConstants.COMPUTER_SYSTEM_CLASS_CONFIGURATION);

            WMI wmiOS = new WMI(AppConstants.OPERATING_SYSTEM_CLASS, AppConstants.OPERATING_SYSTEM_CLASS_CONFIGURATION);
            wmiOS.DictProp.Add("hostname", wmiComputer.DictProp["DNSHostName"]);
            wmiOS.DictProp.Add("workgroup", wmiComputer.DictProp["Workgroup"]);
            wmiOS.DictProp.Add("RegisteredPassword", "patata");
            wmiOS.SetXML(writer);

            //SW:
            writer.WriteStartElement("sw");
            WMI.GetAndSetAllPrograms(writer);
            writer.WriteEndElement(); 
            
            //Tanquem el Writer general.
            writer.WriteEndElement(); 
            writer.Close();

            Console.WriteLine(sw.ToString());
            Console.WriteLine(XMLParser.PostXML(SERVER_URL, sw.ToString()));
        }
    }
}
