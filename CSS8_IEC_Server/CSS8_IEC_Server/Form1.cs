using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace CSS8_IEC_Server
{
    public partial class Form1 : Form
    {
        //委托
        public delegate void Forma1Delegate(Mac_Info macInfo);
        //用于监听的套接字
        public static Socket serverSocket = null;
        //设备参数列表
        public static List<Mac_Info> macInfos = new List<Mac_Info>();
        //多线程通信用参数
        public static int _index = -1;
        public static string xmlPath = @".\SensorUrls.xml";
        public static List<List<string>> totalSensorUrls = new List<List<string>>();
        public static Mutex mtu = new Mutex();//线程锁
        public Form1()
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
            int index = Mac_ListView.SelectedItems[0].Index;
            Mac_Info macInfo = macInfos[index];
            if (!macInfo.isCycleSend)
            {
                macInfo.isCycleSend = true;
                Thread t = new Thread(() => CycleSendAndRecv(macInfos[index], this));
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
                _index = ((ListView)sender).SelectedItems[0].Index;
                Recv_TextBox.Text = macInfos[_index].message;
                DisConnect_Button.Enabled = true;
                Edit_Mac_Button.Enabled = true;
                if (((ListView)sender).SelectedItems[0].SubItems[3].Text == "已启用")
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
            int index = Mac_ListView.SelectedItems[0].Index;
            //关闭设备的套接字
            Mac_Info macInfo = macInfos[index];
            macInfo.isUserDisconnect = true;
            macInfo.socket.Disconnect(true);
        }
        
        public void Edit_Mac_Button_Click(object sender, EventArgs e)
        {
            int index = Mac_ListView.SelectedItems[0].Index;
            Form2._macInfo = macInfos[index];
            Forma1Delegate forma1Delegate = new Forma1Delegate(EditMacInfo);
            Form2 form2 = new Form2(forma1Delegate);
            form2.ShowDialog();
        }

        /*
         * 非静态函数，线程不安全，占用较少资源
         */
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

        public void CloseServer()
        {
            serverSocket.Close();
            for (int i = macInfos.Count - 1; i >= 0; i--)
            {
                //关闭套接字
                Mac_Info macInfo = macInfos[i];
                macInfo.isUserDisconnect = true;
                macInfo.socket.Disconnect(true);
            }
            Server_IP_TextBox.ReadOnly = false;
            Server_Port_Text.ReadOnly = false;
            Server_State.Text = "服务器未开启";
            Start_Server_Button.Text = "开启服务";
        }

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
            Mac_Info macInfo = new Mac_Info();
            macInfo.socket = socket;
            macInfos.Add(macInfo);
            //将客户端信息显示到客户端
            AddMacToListView(socket);
            //接受客户端的数据
            Thread t = new Thread(() => ReciveData(macInfo, this));
            t.IsBackground = true;
            t.Start();
            //判断客户端是否在线
            Thread t1 = new Thread(() => JudgeAlive(macInfo, this));
            t1.IsBackground = true;
            t1.Start();
            //递归循环
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        public void AddMacToListView(Socket socket)
        {
            //将用户填写的设备信息添到列表
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Text = "";
            listViewItem.SubItems.Add(((IPEndPoint)socket.RemoteEndPoint).ToString());
            listViewItem.SubItems.Add((0xFFFF).ToString());
            listViewItem.SubItems.Add("未启用");
            //异步修改ListView并且等待修改完成
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                Mac_ListView.Items.Add(listViewItem);
            })));
            //自动选择第一个设备
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                if (Mac_ListView.Items.Count == 1)
                {
                    Mac_ListView.Items[0].Selected = true;
                }
            })));
        }

        public void RemoveMac(Mac_Info macInfo)
        {
            int index = macInfos.IndexOf(macInfo);
            macInfos.Remove(macInfo);
            //删除ListView中的设备
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                ListViewItem item = Mac_ListView.Items[index];
                Mac_ListView.Items.Remove(item);
            })));
            //删除后自动选择
            if (index == _index)
            {
                if (macInfos.Count > 1)
                {
                    if (index > 0)
                    {
                        Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[_index - 1].Selected = true;
                        })));
                        
                    }
                    else
                    {
                        Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                            Mac_ListView.Items[_index + 1].Selected = true;
                        })));
                    }
                }
                else
                {
                    _index = -1;
                }
            }
        }

        public void EditMacInfo(Mac_Info macInfo)
        {
            Mac_ListView.SelectedItems[0].SubItems[2].Text = macInfo.number.ToString();
        }

        /*
         * 用于多线程的函数(静态函数，线程安全，但占用资源较多)
         */
        public static void JudgeAlive(Mac_Info macInfo, Form1 form)
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
                Thread.Sleep(1000);
            }
        }

        public void CycleSendAndRecv(Mac_Info macInfo, Form1 form)
        {
            //获取全部的Url
            totalSensorUrls = Utils.GetTotalSensorUrls(xmlPath);
            int index = macInfos.IndexOf(macInfo);
            List<string> urls = totalSensorUrls[index];
            while (macInfo.isCycleSend)
            {
                //发送总召唤帧
                macInfo.socket.Send(Utils.CombineMasterCallFrame(macInfo));
                //若超过3秒钟仍没接收到消息则重发，重发3次之后显示超时
                while (macInfo.reSendCount < 3)
                {
                    Thread.Sleep(1000);
                    if (macInfo.recvDataLen == 0)
                    {
                        macInfo.socket.Send(Utils.CombineMasterCallFrame(macInfo));
                        macInfo.reSendCount += 1;
                    }
                    else
                    {
                       // break;
                        macInfo.reSendCount = 0;
                        break;
                    }
                    if (macInfo.reSendCount == 3)
                    {
                        MessageBox.Show("设备" + _index.ToString() + "超时！");
                        form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                            form.Mac_ListView.Items[index].SubItems[3].Text = "已超时";
                        })));
                        return;
                    }
                }
                //获取完整的消息
                int tempDataLen = 0;
                do
                {
                    tempDataLen = macInfo.recvDataLen;
                    Thread.Sleep(100);
                } while (tempDataLen != macInfo.recvDataLen);
                byte[] realData = macInfo.recvBuffer.Skip(0).Take(macInfo.recvDataLen).ToArray();
                //清除接收缓冲区
                Array.Clear(macInfo.recvBuffer, 0, macInfo.recvBuffer.Length);
                macInfo.recvDataLen = 0;
                //获取遥测帧中的传感数据并发送给http服务器
                Utils.AnalysisFrameToHttpServer(realData, urls);
                //更新FCB
                if (macInfo.isChangeFCB)
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
                //展示收到的消息
                string recvStr = "";
                foreach (byte data in realData)
                {
                    string tempStr = data.ToString("X");
                    if (tempStr.Length < 2)
                    {
                        tempStr = "0" + tempStr;
                    }
                    recvStr += tempStr;
                }
                //将接收到的消息放入内存
                recvStr += "\r\n";
                macInfo.message += recvStr;
                macInfo.recvCount += 1;
                //若接受10次消息则清空内存
                if (macInfo.recvCount == 6)
                {
                    macInfo.message = "";
                    macInfo.recvCount = 0;
                }
                //显示消息
                if (index == _index)
                {
                    form.Recv_TextBox.EndInvoke(form.Recv_TextBox.BeginInvoke(new Action(() => {
                        form.Recv_TextBox.Text = macInfo.message;
                    })));
                }
            }
            if (macInfo.isUserDisconnect || macInfo.isSocketError)
            {
                macInfo.isPrepDelete = true;
            }
        }

        public static void ReciveData(Mac_Info macInfo, Form1 form)
        {
            while (macInfo.socket.Connected)
            {
                //判断客户端是否断开
                if (macInfo.socket.Poll(1000, SelectMode.SelectRead))
                {
                    if (macInfo.socket.Available <= 0)
                    {
                        //关闭设备的套接字
                        macInfo.isCycleSend = false;
                        macInfo.isSocketError = true;
                        break;
                    }
                }
                //接受客户端发送的信息
                byte[] tempBuffer = new byte[1024];
                int tempDataLen = 0;
                try
                {
                    tempDataLen = macInfo.socket.Receive(tempBuffer);
                }
                catch (Exception)
                {
                    //关闭设备的套接字
                    macInfo.isCycleSend = false;
                    macInfo.isSocketError = true;
                    break;
                }
                byte[] realData = tempBuffer.Skip(0).Take(tempDataLen).ToArray();
                realData.CopyTo(macInfo.recvBuffer, macInfo.recvDataLen);
                macInfo.recvDataLen += tempDataLen;
            }
            if (macInfo.isCycleSend)
            {
                macInfo.isCycleSend = false;
            }
            else
            {
                macInfo.isPrepDelete = true;
                int count = macInfos.Count;
            }
        }
    }
}
