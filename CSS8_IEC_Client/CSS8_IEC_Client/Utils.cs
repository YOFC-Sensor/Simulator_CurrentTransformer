using CSS8_IEC_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;

namespace CSS8_IEC_Server
{
    public class Utils
    {
        public static byte Char2ToByte(char[] data)
        {
            int result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                int s = 0;
                if (data[i] >= '0' && data[i] <= '9')
                {
                    s = (data[i] - '0');
                }
                else
                {
                    if (data[i] == 'a' || data[i] == 'A')
                    {
                        s = 10;
                    }
                    if (data[i] == 'b' || data[i] == 'B')
                    {
                        s = 11;
                    }
                    if (data[i] == 'c' || data[i] == 'C')
                    {
                        s = 12;
                    }
                    if (data[i] == 'd' || data[i] == 'D')
                    {
                        s = 13;
                    }
                    if (data[i] == 'e' || data[i] == 'E')
                    {
                        s = 14;
                    }
                    if (data[i] == 'f' || data[i] == 'F')
                    {
                        s = 15;
                    }
                }
                result += s * (int)Math.Pow(16, data.Length - i - 1);
            }
            return (byte)result;
        }

        public static byte[] ByteStrToBytes(string data)
        {
            List<byte> result = new List<byte>();
            for (int i = 0; i < data.Length; i = i + 2)
            {
                string byteStr = data.Substring(i, 2);
                byte byteInt = Char2ToByte(byteStr.ToArray());
                result.Add(byteInt);
            }
            return result.ToArray();
        }

        public static IPEndPoint XmlToServerPoint(string xmlPath)
        {
            List<Mac_Info> macInfos = new List<Mac_Info>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode xmlClient = xmlDoc.SelectSingleNode("Client");
            XmlElement xe = (XmlElement)xmlClient;
            IPAddress ipAddress = IPAddress.Parse(xe.GetAttribute("serverIPAddr"));
            int port = int.Parse(xe.GetAttribute("serverPort"));
            IPEndPoint point = new IPEndPoint(ipAddress, port);
            return point;
        }

        public static List<Mac_Info> XmlToMacInfos(string xmlPath)
        {
            List<Mac_Info> macInfos = new List<Mac_Info>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode xmlClient = xmlDoc.SelectSingleNode("Client");
            XmlNodeList xmlMacs = xmlClient.SelectNodes("Mac");
            foreach (XmlNode xmlMac in xmlMacs)
            {
                Mac_Info macInfo = new Mac_Info();
                XmlElement xe = (XmlElement)xmlMac;
                macInfo.name = xe.GetAttribute("name");
                macInfo.number = ByteStrToBytes(xe.GetAttribute("number"));
                macInfo.data = ByteStrToBytes(xe.GetAttribute("data")).ToList();
                macInfos.Add(macInfo);
            }
            return macInfos;
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

        public static byte[] CombineAckFrame(byte[] addr)
        {
            ASDU_Info ackASDUInfo = new ASDU_Info();
            ackASDUInfo.type = 0x64;
            ackASDUInfo.vsq = 0x01;
            ackASDUInfo.cot = 0x07;
            ackASDUInfo.addr = addr;
            ackASDUInfo.fun = 0x00;
            ackASDUInfo.inf = 0x00;
            ackASDUInfo.data.Add(0x14);
            byte[] ackASDU = Utils.CombineASDU(ackASDUInfo);
            Frame_Info ackFrameInfo = new Frame_Info();
            ackFrameInfo.header = 0x68;
            ackFrameInfo.length = (byte)(ackASDU.Length + addr.Length + 1);
            ackFrameInfo.ctrl = 0x80;
            ackFrameInfo.addr = addr;
            ackFrameInfo.asdu = ackASDU.ToList();
            ackFrameInfo.cs = Utils.GetCS(ackFrameInfo.ctrl, addr, ackASDU);
            byte[] ackFrame = Utils.CombineFrame(ackFrameInfo);
            return ackFrame;
        }

        public static byte[] CombineTelFrame(byte[] addr, byte[]  data)
        {
            ASDU_Info telASDUInfo = new ASDU_Info();
            telASDUInfo.type = 0x09;
            telASDUInfo.vsq = (byte)(0x80 | (data.Length + 2));
            telASDUInfo.cot = 0x14;
            telASDUInfo.addr = addr;
            telASDUInfo.fun = 0x01;
            telASDUInfo.inf = 0x07;
            telASDUInfo.data.AddRange(new byte[] { 0x00, 0x00 });
            telASDUInfo.data.AddRange(data);
            byte[] telASDU = Utils.CombineASDU(telASDUInfo);
            Frame_Info telFrameInfo = new Frame_Info();
            telFrameInfo.header = 0x68;
            telFrameInfo.length = (byte)(telASDU.Length + addr.Length + 1);
            telFrameInfo.ctrl = 0x88;
            telFrameInfo.addr = telASDUInfo.addr;
            telFrameInfo.asdu = telASDU.ToList();
            telFrameInfo.cs = Utils.GetCS(telFrameInfo.ctrl, addr, telASDU);
            byte[] telFrame = Utils.CombineFrame(telFrameInfo);
            return telFrame;
        }

        public static byte[] CombineEndFrame(byte[] addr)
        {
            ASDU_Info endASDUInfo = new ASDU_Info();
            endASDUInfo.type = 0x64;
            endASDUInfo.vsq = 0x01;
            endASDUInfo.cot = 0x0a;
            endASDUInfo.addr = addr;
            endASDUInfo.fun = 0x00;
            endASDUInfo.inf = 0x00;
            endASDUInfo.data.Add(0x14);
            byte[] endASDU = Utils.CombineASDU(endASDUInfo);
            Frame_Info endFrameInfo = new Frame_Info();
            endFrameInfo.header = 0x68;
            endFrameInfo.length = (byte)(endASDU.Length + addr.Length + 1);
            endFrameInfo.ctrl = 0x88;
            endFrameInfo.addr = addr;
            endFrameInfo.asdu = endASDU.ToList();
            endFrameInfo.cs = Utils.GetCS(endFrameInfo.ctrl, addr, endASDU);
            byte[] endFrame = Utils.CombineFrame(endFrameInfo);
            return endFrame;
        }
    }
}
