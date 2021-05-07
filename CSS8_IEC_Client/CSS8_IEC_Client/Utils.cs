using CSS8_IEC_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace CSS8_IEC_Server
{
    public class Utils
    {
        /// <summary>
        /// 整数转两个字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] IntToByte2(int data)
        {
            byte heigh = (byte)(data / 256);
            byte low = (byte)(data % 256);
            byte[] result = new byte[] { heigh, low };
            return result;
        }

        /// <summary>
        /// 小数转两个字节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static byte[] DoubleToByte2(double data, int div)
        {
            byte[] result = new byte[2];
            byte heigh = (byte)((int)(data * div) % 256);
            byte low = (byte)((int)(data * div) / 256);
            result[0] = heigh;
            result[1] = low;
            return result;
        }

        /// <summary>
        /// 读取XML配置文件建立虚拟设备
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static List<MacInfo> XmlTomacInfoList(string xmlPath)
        {
            List<MacInfo> macInfos = new List<MacInfo>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode xmlRoot = xmlDoc.SelectSingleNode("Root");
            XmlNodeList xmlClients = xmlRoot.SelectNodes("Client");
            foreach (XmlNode xmlClient in xmlClients)
            {
                XmlElement client = (XmlElement)xmlClient;
                IPAddress ipAddress = IPAddress.Parse(client.GetAttribute("serverIPAddr"));
                int port = int.Parse(client.GetAttribute("serverPort"));
                IPEndPoint serverPoint = new IPEndPoint(ipAddress, port);
                XmlNodeList xmlMacs = xmlClient.SelectNodes("Mac");
                foreach (XmlNode xmlMac in xmlMacs)
                {
                    MacInfo macInfo = new MacInfo();
                    macInfo.serverPoint = serverPoint;
                    XmlElement mac = (XmlElement)xmlMac;
                    macInfo.name = mac.GetAttribute("name");
                    macInfo.number = IntToByte2(int.Parse(mac.GetAttribute("number")));
                    string strI = mac.GetAttribute("i_data");
                    string strV = mac.GetAttribute("v_data");
                    if (strI.Contains(',') && strV.Contains(','))
                    {
                        string[] iStrs = strI.Split(',');
                        string[] vStrs = strV.Split(',');
                        int dataLen = iStrs.Length + vStrs.Length;
                        int div = 100;
                        if (dataLen > 33)
                        {
                            div = 10;
                        }
                        foreach (string iStr in iStrs)
                        {
                            macInfo.data.AddRange(DoubleToByte2(double.Parse(iStr), div));
                        }
                        foreach (string vStr in vStrs)
                        {
                            macInfo.data.AddRange(DoubleToByte2(double.Parse(vStr), div));
                        }
                    }
                    macInfos.Add(macInfo);
                }
            }
            return macInfos;
        }
    }
}
