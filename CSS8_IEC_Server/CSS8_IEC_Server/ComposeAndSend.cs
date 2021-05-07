using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Server
{
    class ComposeAndSend
    {
        private FrameHandle frameHandle = new FrameHandle();

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
            foreach (byte[] frame in frameList)
            {
                socket.Send(frame);
            }
        }
    }
}
