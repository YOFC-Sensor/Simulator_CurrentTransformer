using System.Collections.Generic;

namespace CSS8_IEC_Client
{
    public class ASDU_Info
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
