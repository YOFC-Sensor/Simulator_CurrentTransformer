using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSS8_IEC_Client
{
    public class MacInfo
    {
        public string name = "";
        public IPEndPoint serverPoint = null;
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public byte[] number = new byte[2];
        public byte[] recvData = null;
        public string message = "";
        public List<byte> data = new List<byte>();
        public bool isCanRecive = false;
        public bool isDelete = false;
    }
}
