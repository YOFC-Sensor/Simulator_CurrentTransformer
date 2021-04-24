using System.Collections.Generic;

namespace CSS8_IEC_Client
{
    public class Frame_Info
    {
        public byte header = 0x00;
        public byte length = 0x00;
        public byte ctrl = 0x00;
        public byte[] addr = new byte[] { 0x00, 0x00 };
        public List<byte> asdu = new List<byte>();
        public byte cs = 0x00;
        public byte ender = 0x16;
    }
}
