using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Client
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
        public byte[] ReciveFrame(Socket socket, Client_Form form)
        {
            //接受服务器发送的信息
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
            //判断服务器是否断开
            if (socket.Poll(1000, SelectMode.SelectRead))
            {
                if (socket.Available <= 0)
                {
                    socket.Disconnect(true);
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
        public DataInfo GetDataInfo(byte[] frame)
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
            return dataInfo;
        }
    }

    public class DataInfo
    {
        public FrameType frameType = FrameType.ErrorFrame;
        public ASDUType asduType = ASDUType.ErrorASDU;
        public List<byte> data = new List<byte>();
        public byte[] macNumber = new byte[2];

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            if (frameType == ((DataInfo)obj).frameType && asduType == ((DataInfo)obj).asduType && macNumber[0] == ((DataInfo)obj).macNumber[0] && macNumber[1] == ((DataInfo)obj).macNumber[1])
            {
                isEqual = true;
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(frameType, asduType, data, macNumber);
        }
    }
}
