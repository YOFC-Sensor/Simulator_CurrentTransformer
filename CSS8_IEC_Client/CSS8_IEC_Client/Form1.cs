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
    public enum ConnectState
    {
        HasConnect = 0,
        DisConnect = 1
    }

    public partial class Client_Form : Form
    {
        public static ReciveAndAnalysis reciveAndAnalysis = new ReciveAndAnalysis();
        public static ComposeAndSend composeAndSend = new ComposeAndSend();
        public static List<MacInfo> macInfoList = new List<MacInfo>();//用户已经添加的设备列表
        public static int currentSekectIndex = -1;//当前所选设备的下标
        public static string xmlPath = @".\macInfoList.xml";//XML文件路径
        public Client_Form()
        {
            InitializeComponent();
            //添加设备
            AddMac();
            //绑定清除接收按钮点击事件
            Clear_Recv_Str_Button.Click += Clear_Recv_Str_Button_Click;
            //设备列表选择事件
            Mac_ListView.SelectedIndexChanged += Mac_ListView_SelectedIndexChanged;
            //一键连接按钮点击事件
            Connect_Button.Click += Connect_Button_Click;
            //一键断开按钮点击事件
            Disconnect_Button.Click += Disconnect_Button_Click;
            //刷新按钮点击事件
            Refresh_Button.Click += Refresh_Button_Click;
        }

        /*
         * 控件绑定函数
         */
        public void Clear_Recv_Str_Button_Click(object sender, EventArgs e)
        {
            if (currentSekectIndex != -1)
            {
                macInfoList[currentSekectIndex].recvData = "";
                Recv_TextBox.EndInvoke(Recv_TextBox.BeginInvoke(new Action(() => {
                    Recv_TextBox.Text = "";
                })));
            }
            else
            {
                MessageBox.Show("请先选择设备！");
            }
        }

        public void Mac_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            //判断是否选择了设备
            if (((ListView)sender).SelectedItems.Count != 0)
            {
                //若选择了设备则显示对应设备接收到的信息
                currentSekectIndex = ((ListView)sender).SelectedItems[0].Index;
                Recv_TextBox.Text = macInfoList[currentSekectIndex].recvData;
            }
        }

        public void Connect_Button_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() => MacConnectServer());
            t.IsBackground = true;
            t.Start();
        }

        public void Disconnect_Button_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < macInfoList.Count; i++)
            {
                if (macInfoList[i].socket.Connected)
                {
                    macInfoList[i].isCanRecive = false;
                    macInfoList[i].socket.Disconnect(true);
                    Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[i].SubItems[3].Text = "未连接";
                    })));
                }
            }
        }

        public void Refresh_Button_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < macInfoList.Count; i++)
            {
                if (macInfoList[i].socket.Connected)
                {
                    MessageBox.Show("请先断开所有设备！");
                    return;
                }
            }
            Mac_ListView.Items.Clear();
            AddMac();
        }

        /// <summary>
        /// 将设备信息添加到ListView中
        /// </summary>
        public void AddMac()
        {
            macInfoList = Utils.XmlTomacInfoList(xmlPath);
            //添加设备
            for (int i = 0; i < macInfoList.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Text = "";
                item.SubItems.Add(macInfoList[i].name);
                int number = macInfoList[i].number[0] * 256 + macInfoList[i].number[1];
                item.SubItems.Add(number.ToString());
                item.SubItems.Add("未连接");
                Mac_ListView.Items.Add(item);
            }
            if (macInfoList.Count > 0)
            {
                Mac_ListView.Items[0].Selected = true;
            }
        }

        /// <summary>
        /// 设备连接服务器
        /// </summary>
        public void MacConnectServer()
        {
            foreach (MacInfo macInfo in macInfoList)
            {
                int index = macInfoList.IndexOf(macInfo);
                if (!macInfo.socket.Connected)
                {
                    macInfo.socket.Close();
                    macInfo.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[index].SubItems[3].Text = "连接中";
                    })));
                    //判断是否连接成功
                    try
                    {
                        macInfo.socket.Connect(macInfo.serverPoint);
                    }
                    catch (Exception)
                    {
                        Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                        })));
                        continue;
                    }
                    Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                        Mac_ListView.Items[index].SubItems[3].Text = "已连接";
                    })));
                    //若已连接则开始收发
                    macInfo.isCanRecive = true;
                    Thread t = new Thread(() => RecvAndSend(macInfo, this));
                    t.IsBackground = true;
                    t.Start();
                }
            }
        }

        /// <summary>
        /// 设备接受和发送程序
        /// </summary>
        /// <param name="macInfo"></param>
        /// <param name="form"></param>
        public static void RecvAndSend(MacInfo macInfo, Client_Form form)
        {
            int index = macInfoList.IndexOf(macInfo);
            while (macInfo.isCanRecive)
            {
                //接受服务器发送的信息
                byte[] recvFrame = reciveAndAnalysis.ReciveFrame(macInfo.socket, form);
                if (recvFrame == null)
                {
                    form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                        form.Mac_ListView.Items[index].SubItems[3].Text = "未连接";
                    })));
                    break;
                }
                //若收到的消息不为空
                if (recvFrame.Length > 0)
                {
                    //展示信息
                    string recvStr = "";
                    foreach (byte b in recvFrame)
                    {
                        recvStr += b.ToString("X");
                    }
                    recvStr += "\r\n";
                    form.Recv_TextBox.EndInvoke(form.Recv_TextBox.BeginInvoke(new Action(() => {
                        form.Recv_TextBox.Text += recvStr;
                    })));
                    //获取数据信息
                    DataInfo dataInfo = new DataInfo();
                    dataInfo = reciveAndAnalysis.GetDataInfo(recvFrame);
                    //判断是否可以发送消息
                    if (composeAndSend.isCanSend(dataInfo, macInfo.number))
                    {
                        //封装完整帧
                        List<byte[]> frameList = composeAndSend.CombinedFrame(dataInfo);
                        //发送帧
                        composeAndSend.Send(macInfo.socket, frameList);
                    }
                }
            }
        }
    }
}
