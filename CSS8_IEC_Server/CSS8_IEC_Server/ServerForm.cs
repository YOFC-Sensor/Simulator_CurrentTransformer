using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace CSS8_IEC_Server
{
    public delegate void EditMac(MacInfo macInfo);//用于修改设备的委托
    public partial class ServerForm : Form
    {
        public static FrameHandle frameHandle = new FrameHandle();
        public static ReciveAndAnalysis reciveAndAnalysis = new ReciveAndAnalysis();
        public static ComposeAndSend composeAndSend = new ComposeAndSend();
        public static Socket serverSocket = null;//用于监听的套接字
        public static List<MacInfo> macInfoList = new List<MacInfo>();//设备列表
        public static int currentSelectIndex = -1;//当前选择的设备下标
        public static string xmlPath = @".\SensorUrls.xml";//XML文件路径
        public static Dictionary<string, List<string>> totalSensorUrls = new Dictionary<string, List<string>>();//每个传感器数据对应的URL
        public static Mutex mtu = new Mutex();//线程锁
        
        public ServerForm()
        {
            InitializeComponent();
            //选择设备列表事件
            Mac_ListView.SelectedIndexChanged += Mac_ListView_SelectedIndexChanged;
            //开启服务器按钮点击事件
            Start_Server_Button.Click += Start_Server_Button_Click;
            //开始数据获取按钮点击事件
            Get_Data_Button.Click += Get_Data_Button_Click;
            //断开设备按钮点击事件
            DisConnect_Button.Click += DisConnect_Button_Click;
            //修改按钮点击事件
            Edit_Mac_Button.Click += Edit_Mac_Button_Click; 
        }

        /*
         * 控件事件
         */
        public void Start_Server_Button_Click(object sender, EventArgs e)
        {
            if (Server_State.Text == "服务器未开启")
            {
                StartServer();
            }
            else
            {
                CloseServer();
            }
        }

        public void Get_Data_Button_Click(object sender, EventArgs e)
        {
            MacInfo macInfo = macInfoList[currentSelectIndex];
            if (!macInfo.isCycleSend)
            {
                macInfo.isCycleSend = true;
                Thread t = new Thread(() => CycleSendAndRecv(macInfo, this));
                t.IsBackground = true;
                t.Start();
                Mac_ListView.SelectedItems[0].SubItems[3].Text = "已启用";
                ((Button)sender).Text = "关闭通道";
            }
            else
            {
                macInfo.isCycleSend = false;
                Mac_ListView.SelectedItems[0].SubItems[3].Text = "未启用";
                ((Button)sender).Text = "打开通道";
            }
        }

        public void Mac_ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ListView)sender).SelectedItems.Count != 0)
            {
                currentSelectIndex = ((ListView)sender).SelectedItems[0].Index;
                Recv_TextBox.Text = macInfoList[currentSelectIndex].message;
                DisConnect_Button.Enabled = true;
                Edit_Mac_Button.Enabled = true;
                if (macInfoList[currentSelectIndex].isCycleSend)
                {
                    Get_Data_Button.Text = "关闭通道";
                }
                else
                {
                    Get_Data_Button.Text = "打开通道";
                }
                Get_Data_Button.Enabled = true;
            }
            else
            {
                DisConnect_Button.Enabled = false;
                Edit_Mac_Button.Enabled = false;
                Get_Data_Button.Text = "打开通道";
                Get_Data_Button.Enabled = false;
            }
        }

        public void DisConnect_Button_Click(object sender, EventArgs e)
        {
            //关闭设备的套接字
            MacInfo macInfo = macInfoList[currentSelectIndex];
            macInfo.isUserDisconnect = true;
            macInfo.socket.Disconnect(true);
        }
        
        public void Edit_Mac_Button_Click(object sender, EventArgs e)
        {
            EditMACForm.currentSelectMacInfo = macInfoList[currentSelectIndex];
            EditMac editMac = new EditMac(EditMacInfo);
            EditMACForm editMACForm = new EditMACForm(editMac);
            editMACForm.ShowDialog();
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void StartServer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = null;
            try
            {
                ipAddress = IPAddress.Parse(Server_IP_TextBox.Text);
            }
            catch(Exception e)
            {
                MessageBox.Show("IP地址格式不正确：" + e.Message);
                return;
            }
            int port = 0;
            try
            {
                port = int.Parse(Server_Port_Text.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show("端口号格式不正确：" + e.Message);
                return;
            }
            IPEndPoint serverPoint = new IPEndPoint(ipAddress, port);
            serverSocket.Bind(serverPoint);
            serverSocket.Listen(10);
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
            Server_IP_TextBox.ReadOnly = true;
            Server_Port_Text.ReadOnly = true;
            Server_State.Text = "服务器已开启";
            Start_Server_Button.Text = "关闭服务";
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void CloseServer()
        {
            serverSocket.Close();
            for (int i = macInfoList.Count - 1; i >= 0; i--)
            {
                //关闭套接字
                MacInfo macInfo = macInfoList[i];
                macInfo.isUserDisconnect = true;
                macInfo.socket.Disconnect(true);
            }
            Server_IP_TextBox.ReadOnly = false;
            Server_Port_Text.ReadOnly = false;
            Server_State.Text = "服务器未开启";
            Start_Server_Button.Text = "开启服务";
        }

        /// <summary>
        /// 接收连接请求回调函数
        /// </summary>
        /// <param name="result"></param>
        public void AcceptCallBack(IAsyncResult result)
        {
            Socket serverSocket = (Socket)result.AsyncState;
            Socket socket = null;
            //获取与客户端通信的套接字
            try
            {
                socket = serverSocket.EndAccept(result);
            }
            catch (Exception)
            {
                return;
            }
            //添加第一个设备到内存列表
            MacInfo macInfo = new MacInfo();
            int currentCount = macInfoList.Count;
            macInfo.number[0] = 0x00;
            macInfo.number[1] = 0x00;
            macInfo.socket = socket;
            macInfoList.Add(macInfo);
            //将客户端信息显示到客户端
            AddMacToListView(socket);
            //接受客户端的数据
            Thread t = new Thread(() => CycleReciveData(macInfo));
            t.IsBackground = true;
            t.Start();
            //判断客户端是否在线
            Thread t1 = new Thread(() => JudgeAlive(macInfo, this));
            t1.IsBackground = true;
            t1.Start();
            //递归循环
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        /// <summary>
        /// 将设备添加到ListView中
        /// </summary>
        /// <param name="socket"></param>
        public void AddMacToListView(Socket socket)
        {
            //将用户填写的设备信息添到列表
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Text = "";
            listViewItem.SubItems.Add(((IPEndPoint)socket.RemoteEndPoint).ToString());
            listViewItem.SubItems.Add("未配置");
            listViewItem.SubItems.Add("未启用");
            //异步修改ListView并且等待修改完成
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                Mac_ListView.Items.Add(listViewItem);
                if (Mac_ListView.Items.Count == 1)
                {
                    Mac_ListView.Items[0].Selected = true;
                }
            })));
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="macInfo"></param>
        public void RemoveMac(MacInfo macInfo)
        {
            int index = macInfoList.IndexOf(macInfo);
            macInfoList.Remove(macInfo);
            //删除ListView中的设备
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                Mac_ListView.Items.Remove(Mac_ListView.Items[index]);
            })));
            //删除后自动选择
            if (macInfoList.Count > 0)
            {
                if (currentSelectIndex == index)
                {
                    if (index > 0)
                    {
                        Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[currentSelectIndex - 1].Selected = true;
                        })));
                    }
                    else
                    {
                        Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[0].Selected = true;
                        })));
                    }
                }
            }
            else
            {
                currentSelectIndex = -1;
            }
        }

        /// <summary>
        /// 修改设备
        /// </summary>
        /// <param name="macInfo"></param>
        public void EditMacInfo(MacInfo macInfo)
        {
            int numberInt = macInfo.number[0] * 256 + macInfo.number[1];
            Mac_ListView.SelectedItems[0].SubItems[2].Text = numberInt.ToString();
        }

        /// <summary>
        /// 判断设备是否准备删除
        /// </summary>
        /// <param name="macInfo"></param>
        /// <param name="form"></param>
        public static void JudgeAlive(MacInfo macInfo, ServerForm form)
        {
            while (true)
            {
                if (macInfo.isPrepDelete)
                {
                    macInfo.socket.Close();
                    mtu.WaitOne();
                    form.RemoveMac(macInfo);
                    mtu.ReleaseMutex();
                    return;
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// 循环收发数据
        /// </summary>
        /// <param name="macInfo"></param>
        /// <param name="form"></param>
        public static void CycleSendAndRecv(MacInfo macInfo, ServerForm form)
        {
            //获取当前设备的下标
            int index = macInfoList.IndexOf(macInfo);
            //判断是否配置了站号
            string numberState = "";
            form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                numberState = form.Mac_ListView.Items[index].SubItems[2].Text;
            })));
            if (numberState == "未配置")
            {
                MessageBox.Show("请先配置设备的站号！");
                macInfo.isCycleSend = false;
                form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                    form.Mac_ListView.SelectedItems[0].SubItems[3].Text = "未启用";
                    form.Get_Data_Button.Text = "打开通道";
                })));
                
                return;
            }
            //获取全部的Url
            totalSensorUrls = reciveAndAnalysis.GetTotalSensorUrls(xmlPath);
            string numberStr = (macInfo.number[0] * 256 + macInfo.number[1]).ToString();
            if (!totalSensorUrls.ContainsKey(numberStr))
            {
                MessageBox.Show("配置文件中无对应的站号！");
                macInfo.isCycleSend = false;
                macInfo.isCycleSend = false;
                form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                    form.Mac_ListView.SelectedItems[0].SubItems[3].Text = "未启用";
                    form.Get_Data_Button.Text = "打开通道";
                })));
                return;
            }
            List<string> urls = totalSensorUrls[numberStr];
            while (macInfo.isCycleSend)
            {
                //发送总召唤帧
                macInfo.FCV = 1;
                DataInfo dataInfo = new DataInfo();
                dataInfo.macNumber = macInfo.number;
                dataInfo.fcb = macInfo.FCB;
                dataInfo.fcv = macInfo.FCV;
                composeAndSend.Send(macInfo.socket, composeAndSend.CombinedFrame(dataInfo));
                //获取完整的消息
                int tempCount = 0;
                do
                {
                    tempCount = macInfo.recvData.Count;
                    Thread.Sleep(500);
                } while (tempCount != macInfo.recvData.Count);
                //若没接受到消息则重发
                if (macInfo.recvData.Count == 0)
                {
                    macInfo.reSendCount++;
                    if (macInfo.reSendCount == 3)
                    {
                        form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                            form.Mac_ListView.Items[index].SubItems[3].Text = "已超时";
                        })));
                        break;
                    }
                    continue;
                }
                macInfo.reSendCount = 0;              
                //拼接16进制字符串
                foreach (byte data in macInfo.recvData)
                {
                    string tempStr = data.ToString("X");
                    if (tempStr.Length < 2)
                    {
                        tempStr = "0" + tempStr;
                    }
                    macInfo.message += tempStr;
                }
                macInfo.message += "\r\n";
                //显示消息
                if (index == currentSelectIndex)
                {
                    form.Recv_TextBox.EndInvoke(form.Recv_TextBox.BeginInvoke(new Action(() => {
                        form.Recv_TextBox.Text = macInfo.message;
                    })));
                }
                //获取所有帧中的重要数据
                List<DataInfo> dataInfos = reciveAndAnalysis.GetDataInfoList(macInfo.recvData.ToArray());
                //清空缓冲区
                macInfo.recvData.Clear();
                //获取遥测帧中的传感数据并发送给http服务器
                reciveAndAnalysis.SendToHttpServer(dataInfos, urls);
                //更新FCB
                if (macInfo.FCV == 1)
                {
                    if (macInfo.FCB == 0)
                    {
                        macInfo.FCB = 1;
                    }
                    else
                    {
                        macInfo.FCB = 0;
                    }
                }
                //若接受6次消息则清空内存
                macInfo.recvCount++;
                if (macInfo.recvCount == 6)
                {
                    macInfo.message = "";
                    macInfo.recvCount = 0;
                }
            }
            if (macInfo.isUserDisconnect || macInfo.isSocketError)
            {
                macInfo.isPrepDelete = true;
            }
        }

        /// <summary>
        /// 循环接收数据
        /// </summary>
        /// <param name="macInfo"></param>
        public static void CycleReciveData(MacInfo macInfo)
        {
            while (!macInfo.isUserDisconnect)
            {
                byte[] recvData = reciveAndAnalysis.ReciveFrame(macInfo.socket);
                if (recvData == null)
                {
                    macInfo.isSocketError = true;
                    break;
                }
                macInfo.recvData.AddRange(recvData);
            }
            if (!macInfo.isCycleSend)
            {
                macInfo.isPrepDelete = true;
            }
            else
            {
                macInfo.isCycleSend = false;
            }
        }
    }
}
