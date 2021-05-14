using System;
using System.Collections.Generic;
using System.Linq;

namespace CSS8_IEC_Server
{
    public class FrameHandle
    {
        /// <summary>
        /// 分割粘在一起的帧
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public List<byte[]> DivideFrames(byte[] frames)
        {
            List<byte> ListFrames = frames.ToList();
            ListFrames.Remove(ListFrames[0]);
            frames = ListFrames.ToArray();
            List<byte[]> frameList = new List<byte[]>();
            while (frames.Length > 0)
            {
                int length = frames[1];
                int totalLength = length + 6;
                byte[] frame = frames.Skip(0).Take(totalLength).ToArray();
                frameList.Add(frame);
                List<byte> listFrames = frames.ToList();
                listFrames.RemoveRange(0, totalLength);
                frames = listFrames.ToArray();
            }
            return frameList;
        }

        /// <summary>
        ///分割帧
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public FrameInfo DivideFrame(byte[] frame)
        {
            FrameInfo FrameInfo = new FrameInfo();
            FrameInfo.header = frame[0];
            switch (FrameInfo.header)
            {
                case 0x10:
                    FrameInfo.ctrl = frame[1];
                    FrameInfo.addr = new byte[] { frame[2], frame[3] };
                    FrameInfo.cs = frame[4];
                    break;
                case 0x68:
                    FrameInfo.length = frame[1];
                    FrameInfo.ctrl = frame[4];
                    FrameInfo.addr = new byte[] { frame[5], frame[6] };
                    FrameInfo.asdu = frame.Skip(7).Take(FrameInfo.length - 2 - 1).ToList();
                    FrameInfo.cs = frame[frame.Length - 2];
                    break;
                default:
                    return FrameInfo;
            }
            return FrameInfo;
        }

        /// <summary>
        /// 分割数据
        /// </summary>
        /// <param name="asdu"></param>
        /// <returns></returns>
        public ASDUInfo DivideASDU(byte[] asdu)
        {
            ASDUInfo ASDUInfo = new ASDUInfo();
            ASDUInfo.type = asdu[0];
            ASDUInfo.vsq = asdu[1];
            ASDUInfo.cot = asdu[2];
            ASDUInfo.addr = new byte[] { asdu[3], asdu[4] };
            ASDUInfo.fun = asdu[5];
            ASDUInfo.inf = asdu[6];
            int dataLen = ASDUInfo.vsq & 0x7F;
            ASDUInfo.data = asdu.Skip(7).Take(dataLen).ToList();
            return ASDUInfo;
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
        /// 封装总召唤帧
        /// </summary>
        /// <param name="macInfo"></param>
        /// <returns></returns>
        public byte[] CombineMasterCallFrame(byte[] addr, int fcb, int fcv)
        {
            ASDUInfo asduInfo = new ASDUInfo();
            FrameInfo frameInfo = new FrameInfo();
            //组装ASDU
            asduInfo.type = 0x64;
            asduInfo.vsq = 0x01;
            asduInfo.cot = 0x06;
            asduInfo.addr = addr;
            asduInfo.fun = 0x00;
            asduInfo.inf = 0x00;
            asduInfo.data.Add(0x14);
            byte[] asdu = CombineASDU(asduInfo);
            //组装总控帧
            frameInfo.header = 0x68;
            frameInfo.length = 0x0b;
            frameInfo.ctrl = (byte)(0x43 + fcb * (int)Math.Pow(2, 5) + fcv * (int)Math.Pow(2, 4));
            frameInfo.addr = addr;
            frameInfo.asdu = asdu.ToList();
            frameInfo.cs = GetCS(frameInfo.ctrl, frameInfo.addr, frameInfo.asdu.ToArray());
            byte[] frame = CombineFrame(frameInfo);
            return frame;
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
