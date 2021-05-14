using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSS8_IEC_Client
{
    public class FrameHandle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public FrameInfo DivideFrame(byte[] frame)
        {
            FrameInfo frameInfo = new FrameInfo();
            frameInfo.header = frame[0];
            switch (frameInfo.header)
            {
                case 0x10:
                    frameInfo.ctrl = frame[1];
                    frameInfo.addr = new byte[] { frame[2], frame[3] };
                    frameInfo.cs = frame[4];
                    break;
                case 0x68:
                    frameInfo.length = frame[1];
                    frameInfo.ctrl = frame[4];
                    frameInfo.addr = new byte[] { frame[5], frame[6] };
                    frameInfo.asdu = frame.Skip(7).Take(frameInfo.length - 2 - 1).ToList();
                    frameInfo.cs = frame[frame.Length - 2];
                    break;
                default:
                    return frameInfo;
            }
            return frameInfo;
        }

        /// <summary>
        /// 分割数据
        /// </summary>
        /// <param name="asdu"></param>
        /// <returns></returns>
        public ASDUInfo DivideASDU(byte[] asdu)
        {
            ASDUInfo asduInfo = new ASDUInfo();
            asduInfo.type = asdu[0];
            asduInfo.vsq = asdu[1];
            asduInfo.cot = asdu[2];
            asduInfo.addr = new byte[] { asdu[3], asdu[4] };
            asduInfo.fun = asdu[5];
            asduInfo.inf = asdu[6];
            int dataLen = asduInfo.vsq & 0x7F;
            asduInfo.data = asdu.Skip(7).Take(dataLen).ToList();
            return asduInfo;
        }

        /// <summary>
        /// 获取校验码
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="addr"></param>
        /// <param name="asdu"></param>
        /// <returns></returns>
        public byte GetCS(byte ctrl, byte[] addr, byte[] asdu)
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

        /// <summary>
        /// 封装帧
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <returns></returns>
        public byte[] CombineFrame(FrameInfo frameInfo)
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

        /// <summary>
        /// 封装数据
        /// </summary>
        /// <param name="asduInfo"></param>
        /// <returns></returns>
        public byte[] CombineASDU(ASDUInfo asduInfo)
        {
            List<byte> asdu = new List<byte>();
            asdu.AddRange(new byte[] { asduInfo.type, asduInfo.vsq, asduInfo.cot });
            asdu.AddRange(asduInfo.addr);
            asdu.AddRange(new byte[] { asduInfo.fun, asduInfo.inf });
            asdu.AddRange(asduInfo.data);
            return asdu.ToArray();
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
            byte[] ackASDU = CombineASDU(ackASDUInfo);
            FrameInfo ackFrameInfo = new FrameInfo();
            ackFrameInfo.header = 0x68;
            ackFrameInfo.length = (byte)(ackASDU.Length + addr.Length + 1);
            ackFrameInfo.ctrl = 0x80;
            ackFrameInfo.addr = addr;
            ackFrameInfo.asdu = ackASDU.ToList();
            ackFrameInfo.cs = GetCS(ackFrameInfo.ctrl, addr, ackASDU);
            byte[] ackFrame = CombineFrame(ackFrameInfo);
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
            byte[] telASDU = CombineASDU(telASDUInfo);
            FrameInfo telFrameInfo = new FrameInfo();
            telFrameInfo.header = 0x68;
            telFrameInfo.length = (byte)(telASDU.Length + addr.Length + 1);
            telFrameInfo.ctrl = 0x88;
            telFrameInfo.addr = telASDUInfo.addr;
            telFrameInfo.asdu = telASDU.ToList();
            telFrameInfo.cs = GetCS(telFrameInfo.ctrl, addr, telASDU);
            byte[] telFrame = CombineFrame(telFrameInfo);
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
            byte[] endASDU = CombineASDU(endASDUInfo);
            FrameInfo endFrameInfo = new FrameInfo();
            endFrameInfo.header = 0x68;
            endFrameInfo.length = (byte)(endASDU.Length + addr.Length + 1);
            endFrameInfo.ctrl = 0x88;
            endFrameInfo.addr = addr;
            endFrameInfo.asdu = endASDU.ToList();
            endFrameInfo.cs = GetCS(endFrameInfo.ctrl, addr, endASDU);
            byte[] endFrame = CombineFrame(endFrameInfo);
            return endFrame;
        }
    }

    public class FrameInfo
    {
        public byte header = 0x00;
        public byte length = 0x00;
        public byte ctrl = 0x00;
        public byte[] addr = new byte[] { 0x00, 0x00 };
        public List<byte> asdu = new List<byte>();
        public byte cs = 0x00;
        public byte ender = 0x16;
    }

    public class ASDUInfo
    {
        public byte type = 0x00;
        public byte vsq = 0x00;
        public byte cot = 0x00;
        public byte[] addr = new byte[] { 0x00, 0x00 };
        public byte fun = 0x000;
        public byte inf = 0x00;
        public List<byte> data = new List<byte>();
    }
}
