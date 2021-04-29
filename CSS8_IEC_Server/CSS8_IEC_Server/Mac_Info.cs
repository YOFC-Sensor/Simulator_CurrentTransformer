using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSS8_IEC_Server
{
    public class Mac_Info
    {
        public int number = 65535;
        public Socket socket = null;
        public byte[] recvBuffer = new byte[1024];
        public int recvDataLen = 0;
        public int recvCount = 0;
        public int reSendCount = 0;
        public int FCB = 0;
        public string message = "";
        public bool isChangeFCB = false;
        public bool isCycleSend = false;
        public bool isUserDisconnect = false;
        public bool isSocketError = false;
        public bool isPrepDelete = false;
    }
}
