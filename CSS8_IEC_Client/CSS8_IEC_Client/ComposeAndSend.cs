using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Client
{
    public class ComposeAndSend
    {
        private FrameHandle frameHandle = new FrameHandle();

        /// <summary>
        /// 判断是否具备发送遥测帧的条件
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <param name="macNumber"></param>
        /// <returns></returns>
        public bool isCanSend(DataInfo dataInfo, byte[] macNumber)
        {
            DataInfo correctDataInfo = new DataInfo();
            correctDataInfo.frameType = FrameType.NotFixedFrame;
            correctDataInfo.asduType = ASDUType.MasterCallASDU;
            correctDataInfo.macNumber = macNumber;
            return dataInfo.Equals(correctDataInfo);
        }

        /// <summary>
        /// 封装完整的帧
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public List<byte[]> CombinedFrame(DataInfo dataInfo)
        {
            List<byte[]> frameList = new List<byte[]>();
            byte[] ackFrame = frameHandle.CombineAckFrame(dataInfo.macNumber);
            frameList.Add(ackFrame);
            byte[] telFrame = frameHandle.CombineTelFrame(dataInfo.macNumber, dataInfo.data.ToArray());
            frameList.Add(telFrame);
            byte[] endFrame = frameHandle.CombineEndFrame(dataInfo.macNumber);
            frameList.Add(endFrame);
            return frameList;
        }
        
        /// <summary>
        /// 发送帧
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="frameList"></param>
        public void Send(Socket socket, List<byte[]> frameList)
        {
            socket.Send(new byte[] { 0x00 });
            foreach (byte[] frame in frameList)
            {
                socket.Send(frame);
            }
        }
    }
}
