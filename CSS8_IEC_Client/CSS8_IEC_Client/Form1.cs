using CSS8_IEC_Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSS8_IEC_Client
{
    public partial class Client_Form : Form
    {
        public static IPEndPoint serverPoint = null;
        public static List<Mac_Info> macInfos = new List<Mac_Info>();//用户已经添加的设备列表
        public static int _index = -1;//当前所选设备的下标
        public static bool isConnectCOmplete = false;
        public Client_Form()
        {
            string xmlPath =  @".\MacInfos.xml";
            serverPoint = Utils.XmlToServerPoint(xmlPath);
            macInfos = Utils.XmlToMacInfos(xmlPath);
            InitializeComponent();
            //绑定清除接收按钮点击事件
            Clear_Recv_Str_Button.Click += ClearRecvStr;
            //设备列表选择事件
            Mac_ListView.SelectedIndexChanged += PrintMessage;
            //处理XML设备
            AddMacsToListView(macInfos);
            Thread t = new Thread(() => MacsConnectServer(macInfos));
            t.IsBackground = true;
            t.Start();
        }

        public void AddMacsToListView(List<Mac_Info> macInfos)
        {
            foreach (Mac_Info macInfo in macInfos)
            {
                ListViewItem item = new ListViewItem();
                item.Text = "";
                item.SubItems.Add(macInfo.name);
                item.SubItems.Add("0x" + macInfo.number[0].ToString("X") + macInfo.number[1].ToString("X"));
                item.SubItems.Add("未连接");
                Mac_ListView.Items.Add(item);
            }
            if (macInfos.Count != 0)
            {
                Mac_ListView.Items[0].Selected = true;
            }
        }

        public void MacsConnectServer(List<Mac_Info> macInfos)
        {
            for (int i = 0; i < macInfos.Count; i++)
            {
                //判断是否连接成功
                try
                {
                    macInfos[i].socket.Connect(serverPoint);
                    Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[i].SubItems[3].Text = "已连接";
                    })));
                    //若已连接则开始收发
                    isConnectCOmplete = false;
                    Thread t = new Thread(() => RecvAndSend(macInfos[i], i));
                    t.IsBackground = true;
                    t.Start();
                    while (!isConnectCOmplete) { }
                }
                catch (Exception)
                {
                    Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[i].SubItems[3].Text = "未连接";
                    })));
                }
            }
        }

        public void RecvAndSend(Mac_Info macInfo, int index)
        {
            isConnectCOmplete = true;
            while (true)
            {
                //接受服务器发送的信息
                byte[] recvBuffer = new byte[1024];
                int recvDataLen = 0;
                try
                {
                    recvDataLen = macInfo.socket.Receive(recvBuffer);
                }
                catch (Exception)
                {
                    macInfo.socket.Close();
                    IAsyncResult result = Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                    }));
                    Mac_ListView.EndInvoke(result);
                    break;
                }
                byte[] realData = recvBuffer.Skip(0).Take(recvDataLen).ToArray();
                //清楚缓冲区
                Array.Clear(recvBuffer, 0, recvBuffer.Length);
                recvDataLen = 0;
                //判断服务端是否断开
                bool isFailed = macInfo.socket.Poll(1000, SelectMode.SelectRead);
                if (isFailed)
                {
                    if (macInfo.socket.Available <= 0)
                    {
                        macInfo.socket.Close();
                        IAsyncResult result = Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                        }));
                        Mac_ListView.EndInvoke(result);
                        break;
                    }
                }
                //若收到的消息不为空
                if (realData.Length > 0)
                {
                    //显示接收到的信息
                    string realStr = "";
                    foreach (byte data in realData)
                    {
                        string str = data.ToString("X").ToUpper();
                        if (str.Length == 1)
                        {
                            str = "0" + str;
                        }
                        realStr += str;
                    }
                    realStr += "\r\n";
                    macInfo.recvData += realStr;
                    if (index == _index)
                    {
                        IAsyncResult result = Recv_TextBox.BeginInvoke(new Action(() => {
                            Recv_TextBox.Text = macInfo.recvData;
                        }));
                        Recv_TextBox.EndInvoke(result);
                    }
                    //获取帧中的信息
                    Frame_Info frameInfo = Utils.DivideFrame(realData);
                    int frameType = 0;
                    //判断帧的类型
                    switch (frameInfo.header)
                    {
                        case 0x10:
                            frameType = 0;
                            break;
                        case 0x68:
                            frameType = 1;
                            break;
                        default:
                            continue;
                    }
                    //判断是否发送消息
                    bool canSend = false;
                    if (frameType == 1)
                    {
                        //判断可变帧的命令类型
                        int dataType = 0;
                        ASDU_Info aSDUInfo = Utils.DivideASDU(frameInfo.asdu.ToArray());
                        switch (aSDUInfo.type)
                        {
                            case 0x64:
                                dataType = 0;
                                break;
                            case 0x09:
                                dataType = 1;
                                break;
                            default:
                                continue;
                        }
                        //若是总召命令则判断地址是否一致
                        if (dataType == 0)
                        {
                            if (frameInfo.addr[0] == macInfo.number[0] && frameInfo.addr[1] == macInfo.number[1])
                            {
                                canSend = true;
                            }
                        }
                    }
                    //发送数据
                    if (canSend)
                    {
                        macInfo.socket.Send(new byte[] { 0x00 });
                        //组装确认帧
                        byte[] ackFrame = Utils.CombineAckFrame(macInfo.number);
                        //发送确认帧
                        macInfo.socket.Send(ackFrame);
                        Task.Delay(1000);
                        //组装遥测帧
                        byte[] telFrame = Utils.CombineTelFrame(macInfo.number, macInfo.data.ToArray());
                        //发送遥测帧
                        macInfo.socket.Send(telFrame);
                        Task.Delay(1000);
                        //组装结束帧
                        byte[] endFrame = Utils.CombineEndFrame(macInfo.number);
                        //发送结束帧
                        macInfo.socket.Send(endFrame);
                        Task.Delay(1000);
                    }
                }
            }
        }

        public void PrintMessage(object sender, EventArgs e)
        {
            //判断是否选择了设备
            if (Mac_ListView.SelectedItems.Count != 0)
            {
                //若选择了设备则显示对应设备接收到的信息
                _index = ((ListView)sender).SelectedItems[0].Index;
                Recv_TextBox.Text = macInfos[_index].recvData;
            }
        }

        public void ClearRecvStr(object sender, EventArgs e)
        {
            for (int i = 0; i < macInfos.Count; i++)
            {
                macInfos[_index].recvData = "";
            }
            IAsyncResult result = Recv_TextBox.BeginInvoke(new Action(() => {
                Recv_TextBox.Text = "";
            }));
            Recv_TextBox.EndInvoke(result);
        }
    }
}
