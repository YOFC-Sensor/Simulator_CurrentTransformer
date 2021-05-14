using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 封装确认帧
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte[] CombineAckFrame(byte[] addr)
        {
            ASDUInfo ackASDUInfo = new ASDUInfo();
            ackASDUInfo.type = 0x64;
            ackASDUInfo.vsq = 0x01;
            ackASDUInfo.cot = 0x07;
            ackASDUInfo.addr = addr;
            ackASDUInfo.fun = 0x00;
            ackASDUInfo.inf = 0x00;
            ackASDUInfo.data.Add(0x14);
            byte[] ackASDU = frameHandle.CombineASDU(ackASDUInfo);
            FrameInfo ackFrameInfo = new FrameInfo();
            ackFrameInfo.header = 0x68;
            ackFrameInfo.length = (byte)(ackASDU.Length + addr.Length + 1);
            ackFrameInfo.ctrl = 0x80;
            ackFrameInfo.addr = addr;
            ackFrameInfo.asdu = ackASDU.ToList();
            ackFrameInfo.cs = frameHandle.GetCS(ackFrameInfo.ctrl, addr, ackASDU);
            byte[] ackFrame = frameHandle.CombineFrame(ackFrameInfo);
            return ackFrame;
        }

        /// <summary>
        /// 封装遥测帧
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] CombineTelFrame(byte[] addr, byte[] data)
        {
            ASDUInfo telASDUInfo = new ASDUInfo();
            telASDUInfo.type = 0x09;
            telASDUInfo.vsq = (byte)(0x80 | (data.Length + 2));
            telASDUInfo.cot = 0x14;
            telASDUInfo.addr = addr;
            telASDUInfo.fun = 0x01;
            telASDUInfo.inf = 0x07;
            telASDUInfo.data.AddRange(new byte[] { 0x00, 0x00 });
            telASDUInfo.data.AddRange(data);
            byte[] telASDU = frameHandle.CombineASDU(telASDUInfo);
            FrameInfo telFrameInfo = new FrameInfo();
            telFrameInfo.header = 0x68;
            telFrameInfo.length = (byte)(telASDU.Length + addr.Length + 1);
            telFrameInfo.ctrl = 0x88;
            telFrameInfo.addr = telASDUInfo.addr;
            telFrameInfo.asdu = telASDU.ToList();
            telFrameInfo.cs = frameHandle.GetCS(telFrameInfo.ctrl, addr, telASDU);
            byte[] telFrame = frameHandle.CombineFrame(telFrameInfo);
            return telFrame;
        }

        /// <summary>
        /// 封装结束帧
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte[] CombineEndFrame(byte[] addr)
        {
            ASDUInfo endASDUInfo = new ASDUInfo();
            endASDUInfo.type = 0x64;
            endASDUInfo.vsq = 0x01;
            endASDUInfo.cot = 0x0a;
            endASDUInfo.addr = addr;
            endASDUInfo.fun = 0x00;
            endASDUInfo.inf = 0x00;
            endASDUInfo.data.Add(0x14);
            byte[] endASDU = frameHandle.CombineASDU(endASDUInfo);
            FrameInfo endFrameInfo = new FrameInfo();
            endFrameInfo.header = 0x68;
            endFrameInfo.length = (byte)(endASDU.Length + addr.Length + 1);
            endFrameInfo.ctrl = 0x88;
            endFrameInfo.addr = addr;
            endFrameInfo.asdu = endASDU.ToList();
            endFrameInfo.cs = frameHandle.GetCS(endFrameInfo.ctrl, addr, endASDU);
            byte[] endFrame = frameHandle.CombineFrame(endFrameInfo);
            return endFrame;
        }

        /// <summary>
        /// 封装完整的帧
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public List<byte[]> CombinedFrame(DataInfo dataInfo)
        {
            List<byte[]> frameList = new List<byte[]>();
            byte[] ackFrame = CombineAckFrame(dataInfo.macNumber);
            frameList.Add(ackFrame);
            byte[] telFrame = CombineTelFrame(dataInfo.macNumber, dataInfo.data.ToArray());
            frameList.Add(telFrame);
            byte[] endFrame = CombineEndFrame(dataInfo.macNumber);
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
