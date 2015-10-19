using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//New import:
using System.Management;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GetMachineInfoFinal
{
    /// <summary>
    /// Classe que s'encarrega de montar l'XML i d'enviar-lo.
    /// </summary>
    
    class XMLParser
    {
        //S'encarrega d'afegir al writer passat, el diccionari de propietats i els valors del component corresponent.
        public static void SetXMLFromProperties(String className, String[] arrayToGet, Dictionary<String, Object> dictProp, XmlTextWriter writer)
        {
            writer.WriteStartElement(AppConstants.DICT_TAGS[className]); //<component>
            for (int i = 0; i < arrayToGet.Length; i++)
            {
                writer.WriteStartElement(arrayToGet[i]);
                try
                {
                    if (arrayToGet[i] == "DefaultIPGateway") //Si es la porta d'enllaç, agafarem el primer element del array.
                    {
                        String[] array = (String[])dictProp[arrayToGet[i]];
                        writer.WriteString(array[0]);
                    }
                    else
                    {
                        writer.WriteString(
                            //if
                            (dictProp[arrayToGet[i]] == null) ||
                            (dictProp[arrayToGet[i]].Equals("")) ||
                            (dictProp[arrayToGet[i]].Equals("0")) || //
                            (dictProp[arrayToGet[i]].Equals("N/A")) ? "N/A" :
                            //else
                            (dictProp[arrayToGet[i]].ToString().Trim())
                            );
                    }
                    writer.WriteEndElement();
                }
                catch (Exception)
                {
                    writer.WriteString("N/A");
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement(); //</component>
        }

        //S'encarrega de fer el post del xml al servidor.
        public static String PostXML(String destinationUrl, String xml)
        {
            try
            {
                //Creem la petició amb el destination url.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);

                //Codifiquem les dades.
                byte[] bytes;
                bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";

                //'Escrivim' les dades.
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                //Retornem la resposta.
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    response.Close();
                    return responseStr;
                }
                else
                    Console.WriteLine("Error al enviar dades!");
                response.Close();
            }
            catch (Exception) { Console.WriteLine("Error al enviar dades!"); }
            return "";
        }
    }
}
