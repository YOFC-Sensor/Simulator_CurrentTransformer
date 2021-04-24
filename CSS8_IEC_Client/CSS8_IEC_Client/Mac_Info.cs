using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Client
{
    public class Mac_Info
    {
        public string name = "";
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public byte[] number = new byte[2];
        public string recvData = "";
        public List<byte> data = new List<byte>();
    }
}
