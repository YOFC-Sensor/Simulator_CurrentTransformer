using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSS8_IEC_Server
{
    public class MacInfo
    {
        public byte[] number = new byte[2];
        public Socket socket = null;
        public List<byte> recvData = new List<byte>();
        public int recvCount = 0;
        public int reSendCount = 0;
        public int FCB = 0;
        public int FCV = 0;
        public string message = "";
        public bool isCycleSend = false;
        public bool isUserDisconnect = false;
        public bool isSocketError = false;
        public bool isPrepDelete = false;
    }
}
