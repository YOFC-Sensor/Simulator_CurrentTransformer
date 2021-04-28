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
        public static List<Mac_Info> macInfos = new List<Mac_Info>();//用户已经添加的设备列表
        public static int _index = -1;//当前所选设备的下标
        public static string xmlPath = @".\MacInfos.xml";
        public Client_Form()
        {
            InitializeComponent();
            //添加设备
            AddMac();
            //绑定清除接收按钮点击事件
            Clear_Recv_Str_Button.Click += Clear_Recv_Str_Button_Click;
            //设备列表选择事件
            Mac_ListView.SelectedIndexChanged += Mac_ListView_SelectedIndexChanged;
            //一减连接按钮点击事件
            Connect_Button.Click += Connect_Button_Click;
        }

        /*
         * 控件绑定函数
         */
        public void Clear_Recv_Str_Button_Click(object sender, EventArgs e)
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

        public void Mac_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            //判断是否选择了设备
            if (Mac_ListView.SelectedItems.Count != 0)
            {
                //若选择了设备则显示对应设备接收到的信息
                _index = ((ListView)sender).SelectedItems[0].Index;
                Recv_TextBox.Text = macInfos[_index].recvData;
            }
        }

        public void Connect_Button_Click(object sender, EventArgs e)
        {
            foreach (Mac_Info macInfo in macInfos)
            {
                Thread t = new Thread(() => MacConnectServer(macInfo, this));
                t.IsBackground = true;
                t.Start();
            }
        }

        /*
         * 非静态函数，线程不安全，但占用资源较少
         */
        public void AddMac()
        {
            macInfos = Utils.XmlToMacInfos(xmlPath);
            //添加设备
            for (int i = 0; i < macInfos.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Text = "";
                item.SubItems.Add(macInfos[i].name);
                int number = macInfos[i].number[0] * 256 + macInfos[i].number[1];
                item.SubItems.Add(number.ToString());
                item.SubItems.Add("未连接");
                Mac_ListView.Items.Add(item);
            }
        }

        public void RemoveMac(Mac_Info macInfo)
        {
            int index = macInfos.IndexOf(macInfo);
            macInfo.isDelete = true;
            if (macInfo.isCanRecive)
            {
                macInfo.isCanRecive = false;
            }
            else
            {
                macInfo.socket.Close();
                macInfos.Remove(macInfo);
            }
            Mac_ListView.Items.Remove(Mac_ListView.Items[index]);
        }

        /*
         * 静态函数，线程安全，但占用较多资源
         */
        public static void MacConnectServer(Mac_Info macInfo, Client_Form form)
        {
            int index = macInfos.IndexOf(macInfo);
            if (!macInfo.socket.Connected)
            {
                //判断是否连接成功
                form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                    form.Mac_ListView.Items[index].SubItems[3].Text = "连接中";
                })));
                try
                {
                    macInfo.socket.Connect(macInfo.serverPoint);
                }
                catch (Exception)
                {
                    form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                        form.Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                    })));
                    return;
                }
                form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                    form.Mac_ListView.Items[index].SubItems[3].Text = "已连接";
                })));
                //若已连接则开始收发
                macInfo.isCanRecive = true;
                Thread t = new Thread(() => RecvAndSend(macInfo, form));
                t.IsBackground = true;
                t.Start();
            }
        }

        public static void RecvAndSend(Mac_Info macInfo, Client_Form form)
        {
            int index = macInfos.IndexOf(macInfo);
            while (macInfo.isCanRecive)
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
                    form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                        form.Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                    })));
                    return;
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
                        form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                            form.Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                        })));
                        return;
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
                        IAsyncResult result = form.Recv_TextBox.BeginInvoke(new Action(() => {
                            form.Recv_TextBox.Text = macInfo.recvData;
                        }));
                        form.Recv_TextBox.EndInvoke(result);
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
            if (macInfo.isDelete)
            {
                form.RemoveMac(macInfo);
            }
        }
    }
}
