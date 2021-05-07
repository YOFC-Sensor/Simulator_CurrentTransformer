using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using HttpSend;

namespace CSS8_IEC_Server
{
    class Utils
    {
        public static byte[] IntToByte2(int data)
        {
            byte heigh = (byte)(data / 256);
            byte low = (byte)(data % 256);
            byte[] result = new byte[] { heigh, low };
            return result;
        }

        public static double Byte2ToDouble(byte[] data, int accuracy)
        {
            double div = Math.Pow(10, accuracy);
            double result = ((double)data[1] * 256 + (double)data[0]) / div;
            return result;
        }

        public static void AnalysisFrameToHttpServer(byte[] frames,List<string> urls)
        {
            //删除帧的第一个元素
            List<byte> tempFrames = frames.ToList();
            tempFrames.Remove(tempFrames[0]);
            frames = tempFrames.ToArray();
            //解析帧
            int len = frames.Length;
            List<byte[]> frameList = new List<byte[]>();
            frameList = DivideComplexFrame(frames);
            List<byte> data = new List<byte>();
            int number = 0;
            foreach (byte[] frame in frameList)
            {
                Frame_Info frameInfo = new Frame_Info();
                frameInfo = DivideFrame(frame);
                if (frameInfo.ctrl == 0x88)
                {
                    byte[] asdu = frameInfo.asdu.ToArray();
                    ASDU_Info asduInfo = new ASDU_Info();
                    asduInfo = DivideASDU(asdu);
                    if (asduInfo.type == 0x09)
                    {
                        number = asduInfo.addr[0] * (int)Math.Pow(16, 2) + asduInfo.addr[1];
                        data = asduInfo.data;
                    }
                }
            }
            int accuracy = 0;
            if (data.Count > 33)
            {
                accuracy = 2;
            }
            else
            {
                accuracy = 1;
            }
            data.RemoveRange(0, 2);
            //将帧中的数据组装成字典
            List<Dictionary<string, double>> totalJsonData = new List<Dictionary<string, double>>();
            for (int i = 0; i < (data.Count / 2); i = i + 2)
            {
                int t = 0;
                Dictionary<string, double> jsonData = new Dictionary<string, double>();
                t = i;
                jsonData.Add("sensor_" + number.ToString() + "_" + (i / 2).ToString() + "_A", ByteToDouble(data.Skip(t).Take(2).ToArray(), accuracy));
                t = i + data.Count / 2;
                jsonData.Add("sensor_" + number.ToString() + "_" + (i / 2).ToString() + "_V", ByteToDouble(data.Skip(t).Take(2).ToArray(), accuracy));
                totalJsonData.Add(jsonData);
            }
            //将每个传感器的数据发送给http服务器
            for (int i = 0; i < urls.Count; i++)
            {
                Dictionary<string, double> jsonData = totalJsonData[i];
                HTTPTransmit.Send(urls[i], jsonData);
            }
        }

        public static List<List<string>> GetTotalSensorUrls(string xmlPath)
        {
            List<List<string>> totalUrls = new List<List<string>>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode xmlServer = xmlDoc.SelectSingleNode("Server");
            XmlNodeList xmlMacs = xmlServer.SelectNodes("Mac");
            foreach (XmlNode xmlMac in xmlMacs)
            {
                List<string> sensorUrls = new List<string>();
                XmlNodeList xmlSensors = xmlMac.SelectNodes("Sersor");
                foreach (XmlNode xmlSensor in xmlSensors)
                {
                    XmlElement xe = (XmlElement)xmlSensor;
                    string url = xe.GetAttribute("url");
                    sensorUrls.Add(url);
                }
                totalUrls.Add(sensorUrls);
            }
            return totalUrls;
        }
    }
}
