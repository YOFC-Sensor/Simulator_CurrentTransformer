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
        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }

        public static byte[] CombineFrame(Frame_Info frameInfo)
        {
            List<byte> frame = new List<byte>();
            frame.AddRange(new byte[] { frameInfo.header, frameInfo.length, frameInfo.length, frameInfo.header });
            frame.Add(frameInfo.ctrl);
            frame.AddRange(frameInfo.addr);
            frame.AddRange(frameInfo.asdu);
            frame.Add(frameInfo.cs);
            frame.Add(frameInfo.ender);
            return frame.ToArray();
        }

        public static byte[] CombineASDU(ASDU_Info asduInfo)
        {
            List<byte> asdu = new List<byte>();
            asdu.AddRange(new byte[] { asduInfo.type, asduInfo.vsq, asduInfo.cot });
            asdu.AddRange(asduInfo.addr);
            asdu.AddRange(new byte[] { asduInfo.fun, asduInfo.inf });
            asdu.AddRange(asduInfo.data);
            return asdu.ToArray();
        }

        public static byte GetCS(byte ctrl, byte[] addr, byte[] asdu)
        {
            long sum = 0;
            sum += ctrl;
            sum += addr[0] + addr[1];
            foreach (byte b in asdu)
            {
                sum += b;
            }
            byte cs = (byte)(sum & 0xFF);
            return cs;
        }

        public static List<byte[]> DivideComplexFrame(byte[] complexFrame)
        {
            List<byte[]> frameList = new List<byte[]>();
            int i = 0;
            while (i < complexFrame.Length)
            {
                byte length = complexFrame[i + 1];
                byte[] frame = complexFrame.Skip(i).Take(i + length + 6).ToArray();
                frameList.Add(frame);
                i = i + length + 6;
            }
            return frameList;
        }

        public static Frame_Info DivideFrame(byte[] frame)
        {
            Frame_Info frame_info = new Frame_Info();
            frame_info.header = frame[0];
            switch (frame_info.header)
            {
                case 0x10:
                    frame_info.ctrl = frame[1];
                    frame_info.addr = new byte[] { frame[2], frame[3] };
                    frame_info.cs = frame[4];
                    break;
                case 0x68:
                    frame_info.length = frame[1];
                    frame_info.ctrl = frame[4];
                    frame_info.addr = new byte[] { frame[5], frame[6] };
                    frame_info.asdu = frame.Skip(7).Take(frame_info.length - 2 - 1).ToList();
                    frame_info.cs = frame[frame.Length - 2];
                    break;
                default:
                    return frame_info;
            }
            return frame_info;
        }

        public static ASDU_Info DivideASDU(byte[] asdu)
        {
            ASDU_Info asdu_info = new ASDU_Info();
            asdu_info.type = asdu[0];
            asdu_info.vsq = asdu[1];
            asdu_info.cot = asdu[2];
            asdu_info.addr = new byte[] { asdu[3], asdu[4] };
            asdu_info.fun = asdu[5];
            asdu_info.inf = asdu[6];
            int dataLen = asdu_info.vsq & 0x7F;
            asdu_info.data = asdu.Skip(7).Take(dataLen).ToList();
            return asdu_info;
        }

        public static double ByteToDouble(byte[] data, int accuracy)
        {
            double div = Math.Pow(10, accuracy);
            double result = ((double)data[1] * 256 + (double)data[0]) / div;
            return result;
        }

        public static byte[] CombineMasterCallFrame(Mac_Info macInfo)
        {
            ASDU_Info asduInfo = new ASDU_Info();
            Frame_Info frameInfo = new Frame_Info();
            //分解站号
            byte heighNumber = (byte)(macInfo.number >> 8);
            byte lowNumber = (byte)(macInfo.number & 0x00FF);
            //组装ASDU
            asduInfo.type = 0x64;
            asduInfo.vsq = 0x01;
            asduInfo.cot = 0x06;
            asduInfo.addr = new byte[] { heighNumber, lowNumber };
            asduInfo.fun = 0x00;
            asduInfo.inf = 0x00;
            asduInfo.data.Add(0x14);
            byte[] asdu = Utils.CombineASDU(asduInfo);
            //组装总控帧
            frameInfo.header = 0x68;
            frameInfo.length = 0x0b;
            frameInfo.ctrl = (byte)(0x53 + macInfo.FCB * (int)Math.Pow(2, macInfo.FCB * 5));
            frameInfo.addr = new byte[] { heighNumber, lowNumber };
            frameInfo.asdu = asdu.ToList();
            frameInfo.cs = Utils.GetCS(frameInfo.ctrl, frameInfo.addr, frameInfo.asdu.ToArray());
            byte[] frame = Utils.CombineFrame(frameInfo);
            //判断是否改变FCB
            byte ctrl = frameInfo.ctrl;
            int FCV = (ctrl >> 4) & 0x01;
            if (FCV == 0)
            {
                macInfo.isChangeFCB = false;
            }
            else
            {
                macInfo.isChangeFCB = true;
            }
            return frame;
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
