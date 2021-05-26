using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Server
{
    public class ComposeAndSend
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
                byte[] masterCallFrame = frameHandle.CombineMasterCallFrame(dataInfo.macNumber, dataInfo.fcb, dataInfo.fcv);
                frameList.Add(masterCallFrame);
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
