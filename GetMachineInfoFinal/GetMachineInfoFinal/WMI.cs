using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//New import.
using System.Management;
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GetMachineInfoFinal
{
    /// <summary>
    /// Classe que centralitza les consultes Windows Machine Information.
    /// </summary>
    
    class WMI
    {
        #region Atributs
        private String className = "";
        //Nomès si es single.
        private String select = "";
        private String[] arrayToGet;
        private PropertyDataCollection arrayProp;
        private Dictionary<String, Object> dictProp = new Dictionary<String, Object>();
        #endregion

        #region Propietats
        public Dictionary<String, Object> DictProp
        {
            get
            {
                return this.dictProp;
            }
        }
        #endregion

        #region Constructors
        //General query.
        public WMI(String wmiClass, String[] arrayToGet)
        {
            this.className = wmiClass;
            this.arrayToGet = arrayToGet;
            this.arrayProp = CustomGetValues();
            if (this.arrayProp != null)
                this.dictProp = CustomTransformProperties(this.arrayProp);
        }

        //Single Query.
        public WMI(String wmiClass, String select, String[] arrayToGet)
        {
            this.className = wmiClass;
            this.select = select;
            if (this.className == AppConstants.DISK_CLASS) //Si és disk.
                this.select = "Win32_LogicalDisk.DeviceID=\"" + GetInstalledDrive() + "\"";
            this.arrayToGet = arrayToGet;
            this.arrayProp = CustomGetValues(true);
            if (this.arrayProp != null)
                this.dictProp = CustomTransformProperties(this.arrayProp);
        }
        #endregion

        #region Mètodes
        //Retornarà les propietats del primer objecte trobat.
        private PropertyDataCollection CustomGetValues()
        {
            try
            {
                //Creem la instància seleccionant la classe que li passem.
                ManagementClass mgmt = new ManagementClass(this.className);

                //Agafem la colecció d'objectes trobats.
                ManagementObjectCollection objCol = mgmt.GetInstances();

                //Fem un loop i parem al primer.
                foreach (ManagementObject obj in objCol)
                {
                    if (this.className == AppConstants.NETWORK_CLASS_CONFIGURATION)
                    {
                        if ((bool)obj["IPEnabled"]) //Si està enabled la ip.
                            return obj.Properties;
                    }
                    else if (this.className == AppConstants.NETWORK_CLASS) //Si està la xarxa enabled.
                    {
                        if (obj.Properties["NetEnabled"].Value != null)
                            if (obj.Properties["NetEnabled"].Value.ToString().Equals("True"))
                                return obj.Properties;
                    }
                    else
                        return obj.Properties;
                }
            }
            catch (Exception) { ; }

            return null;
        }

        //Retornarà les propietats del primer objecte trobat d'un objecte en concret a travès de una select.
        private PropertyDataCollection CustomGetValues(bool single)
        {
            ManagementObject disk = new ManagementObject(this.select);
            disk.Get();
            return disk.Properties;
        }

        //Transforma l'array de propietats a un diccionari, degut a que aquest array no es pot modificar ni afegir valors, per tant amb un diccionari ens farem la vida més fàcil.
        private static Dictionary<String, Object> CustomTransformProperties(PropertyDataCollection prop)
        {
            Dictionary<String, Object> toReturn = new Dictionary<String, Object>();
            foreach (PropertyData obj in prop)
                toReturn.Add(obj.Name, obj.Value);
            return toReturn;
        }

        //S'encarrega d'afegir al XML l'objecte transformat.
        public void SetXML(XmlTextWriter writer)
        {
            if (this.dictProp != null)
                XMLParser.SetXMLFromProperties(this.className, this.arrayToGet, this.dictProp, writer);
        }

        //S'encarrega d'obtenir el drive en el qual Windows està instal·lat (Ex: C:\).
        public String GetInstalledDrive()
        {
            String auxClassName = this.className;
            this.className = AppConstants.OPERATING_SYSTEM_CLASS;
            this.arrayProp = CustomGetValues();
            this.className = auxClassName;
            return arrayProp["SystemDrive"].Value.ToString();
        }

        #region Network

        //Retornarà les propietats del primer objecte trobat.
        public static String FindMAC()
        {
            ManagementClass mgmt = new ManagementClass(AppConstants.NETWORK_CLASS_CONFIGURATION);
            ManagementObjectCollection objCol = mgmt.GetInstances();
            foreach (ManagementObject obj in objCol)
            {
                    if ((bool)obj["IPEnabled"]) //Si està enabled la ip.
                        return obj.Properties["MacAddress"].Value.ToString();
            }
            return "";
        }

        //S'encarrega d'agafar la current ip.
        public static IPAddress GetIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            //Per cada adreça.
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
            }
            return null;
        }

        //Agafem la màscara, sigui de subnettingo no.
        public static IPAddress GetSubnetMask(IPAddress address)
        {
            if (address == null) return null;
            //Per cada adaptador.
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                //Per cada unicast address.
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    //Si correspon a la xarxa interna.
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                            return unicastIPAddressInformation.IPv4Mask;
                    }
                }
            }
            return null;
        }

        //Retorna la adreça de broadcast a través de la ip i de la màscara.
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            if(address == null || subnetMask == null) return null;

            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                return null;

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                //Fem un or combinat amb un XOR(^) per fer el broadcasting address.
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        //Retorna els DNS del primer adaptador amb status Up.
        public static String GetDnsAddresses()
        {
            String toWrite = "N/A";
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters) //Per cada adapter.
            {
                //Si l'status és Up i no és de Loopback.
                if (adapter.OperationalStatus.ToString() == "Up" && !adapter.Description.Contains("back"))
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                    if (dnsServers.Count > 0)
                    {
                        toWrite = "";
                        foreach (IPAddress dns in dnsServers) //Per cada DNS els converteixo a Strings.
                        {
                            if (!dns.ToString().Contains(":"))
                                toWrite += dns + ", ";
                        }
                        toWrite = toWrite.Substring(0, toWrite.Length - 2);
                        return toWrite;
                    }
                }
            }
            return toWrite;
        }
        #endregion

        #region SW
        //S'encarrega d'obtenir i assignar al XML les propietats de cada programa.
        public static void GetAndSetAllPrograms(XmlTextWriter writer)
        {
            //Agafem tots els productes.
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
            foreach (ManagementObject mo in mos.Get()) //I per cada objecte fem la crida al parser.
                XMLParser.SetXMLFromProperties(AppConstants.PROGRAM, AppConstants.SW_CONFIGURATION, CustomTransformProperties(mo.Properties), writer);
        }
        #endregion 

        //ToString per comprovar les propietats del objecte.
        public override string ToString()
        {
            String toReturn = "Propietats de " + this.className + ":\n\n";
            if(this.arrayProp != null)
            {
                foreach (PropertyData prop in this.arrayProp)
                {
                    toReturn += prop.Name + ": " + prop.Value + "\n";
                }
            }
            else
            {
                toReturn = "La consulta no té propietats!";
            }
            return toReturn;
        }
        #endregion
    }
}
