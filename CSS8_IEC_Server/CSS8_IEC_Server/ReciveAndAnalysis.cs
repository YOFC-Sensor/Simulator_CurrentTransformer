using HttpSend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Xml;

namespace CSS8_IEC_Server
{
    public enum FrameType
    {
        FixedFrame = 0,
        NotFixedFrame = 1,
        ErrorFrame = -1
    }

    public enum ASDUType
    {
        MasterCallASDU = 0,
        RemoteASDU = 1,
        ErrorASDU = -1
    }

    public class ReciveAndAnalysis
    {
        private FrameHandle frameHandle = new FrameHandle();

        /// <summary>
        /// 2字节转小数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public double Byte2ToDouble(byte[] data, int accuracy)
        {
            double div = Math.Pow(10, accuracy);
            double result = ((double)data[1] * 256 + (double)data[0]) / div;
            return result;
        }

        /// <summary>
        /// 获取帧类型
        /// </summary>
        /// <param name="frameHead"></param>
        /// <returns></returns>
        public FrameType GetFrameType(byte frameHead)
        {
            FrameType frameType = FrameType.ErrorFrame;
            switch (frameHead)
            {
                case 0x10:
                    frameType = FrameType.FixedFrame;
                    break;
                case 0x68:
                    frameType = FrameType.NotFixedFrame;
                    break;
            }
            return frameType;
        }

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="asduTypeByte"></param>
        /// <returns></returns>
        public ASDUType GetASDUType(byte asduTypeByte)
        {
            ASDUType asduType = ASDUType.ErrorASDU;
            switch (asduTypeByte)
            {
                case 0x64:
                    asduType = ASDUType.MasterCallASDU;
                    break;
                case 0x09:
                    asduType = ASDUType.RemoteASDU;
                    break;
            }
            return asduType;
        }

        /// <summary>
        /// 获取接收到的帧
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="connectState"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public byte[] ReciveFrame(Socket socket, ServerForm form)
        {
            //接受客户端发送的信息
            byte[] recvBuffer = new byte[1024];
            int recvDataLen = 0;
            try
            {
                recvDataLen = socket.Receive(recvBuffer);
            }
            catch (Exception)
            {
                return null;
            }
            //判断是否主动断开
            if (!socket.Connected)
            {
                return null;
            }
            //判断客户端是否断开
            if (socket.Poll(1000, SelectMode.SelectRead))
            {
                if (socket.Available <= 0)
                {
                    return null;
                }
            }
            //取出数据
            byte[] realData = recvBuffer.Skip(0).Take(recvDataLen).ToArray();
            return realData;
        }

        /// <summary>
        /// 判断是否可以发送信息
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public List<DataInfo> GetDataInfoList(byte[] frames)
        {
            List<DataInfo> dataInfoList = new List<DataInfo>();
            List<byte[]> frameList = frameHandle.DivideFrames(frames);
            foreach (byte[] frame in frameList)
            {
                //获取帧中的信息
                FrameInfo frameInfo = frameHandle.DivideFrame(frame);
                //获取帧中的数据信息
                ASDUInfo asduInfo = frameHandle.DivideASDU(frameInfo.asdu.ToArray());
                //获取数据信息
                DataInfo dataInfo = new DataInfo();
                dataInfo.frameType = GetFrameType(frameInfo.header);
                dataInfo.asduType = GetASDUType(asduInfo.type);
                dataInfo.data = asduInfo.data;
                dataInfo.macNumber = frameInfo.addr;
                dataInfoList.Add(dataInfo);
            }
            return dataInfoList;
        }

        /// <summary>
        /// 获取JSON类型的数据
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<Dictionary<string, double>> GetJsonDataList(int number, List<byte> data)
        {
            List<Dictionary<string, double>> jsonDataList = new List<Dictionary<string, double>>();
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
                jsonData.Add("sensor_" + number.ToString() + "_" + (i / 2).ToString() + "_A", Byte2ToDouble(data.Skip(t).Take(2).ToArray(), accuracy));
                t = i + data.Count / 2;
                jsonData.Add("sensor_" + number.ToString() + "_" + (i / 2).ToString() + "_V", Byte2ToDouble(data.Skip(t).Take(2).ToArray(), accuracy));
                totalJsonData.Add(jsonData);
            }
            return jsonDataList;
        }

        /// <summary>
        /// 读取XML文件中的传感器URL
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public List<List<string>> GetTotalSensorUrls(string xmlPath)
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

        /// <summary>
        /// 将Json类型的数据发送给http服务器
        /// </summary>
        /// <param name="dataInfos"></param>
        /// <param name="urls"></param>
        public void SendToHttpServer(List<DataInfo> dataInfos, List<string> urls)
        {
            foreach (DataInfo dataInfo in dataInfos)
            {
                DataInfo correctDataInfo = new DataInfo();
                correctDataInfo.frameType = FrameType.NotFixedFrame;
                correctDataInfo.asduType = ASDUType.RemoteASDU;
                if (dataInfo.Equals(correctDataInfo))
                {
                    int number = dataInfo.macNumber[0] * 256 + dataInfo.macNumber[1];
                    List<Dictionary<string, double>> jsonDataList = GetJsonDataList(number, dataInfo.data);
                    for (int i = 0; i < urls.Count; i++)
                    {
                        Dictionary<string, double> jsonData = jsonDataList[i];
                        HTTPTransmit.Send(urls[i], jsonData);
                    }
                }
            }
        }
    }

    public class DataInfo
    {
        public FrameType frameType = FrameType.ErrorFrame;
        public ASDUType asduType = ASDUType.ErrorASDU;
        public List<byte> data = new List<byte>();
        public byte[] macNumber = new byte[2];
        public int fcb = 0;
        public int fcv = 0;

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            if (frameType == ((DataInfo)obj).frameType && asduType == ((DataInfo)obj).asduType)
            {
                isEqual = true;
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(frameType, asduType, data, macNumber, fcb);
        }
    }
}
