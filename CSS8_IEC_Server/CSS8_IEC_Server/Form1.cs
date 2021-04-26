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
        public static Socket serverSocket = null;
        //设备参数列表
        public static List<Mac_Info> macInfos = new List<Mac_Info>();
        //多线程通信用参数
        public static int macCount = 0;
        public static int _index = -1;
        public static Mac_Info _macInfo  = null;
        public static bool isUserDisConnect = false;
        public static string xmlPath = "";
        public Form1()
        {
            xmlPath = System.IO.Directory.GetCurrentDirectory() + @"\SensorUrls.xml";
            InitializeComponent();
            //开启设备信息实时更新
            Thread t = new Thread(() => EditMacInfo());
            t.IsBackground = true;
            t.Start();
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
            if (Mac_ListView.SelectedItems[0].SubItems[3].Text == "未启用")
            {
                macInfos[index].isCycleSend = true;
                Thread t = new Thread(() => CycleSendAndRecv(macInfos[index], this));
                t.IsBackground = true;
                t.Start();
                Mac_ListView.SelectedItems[0].SubItems[3].Text = "已启用";
                ((Button)sender).Text = "关闭通道";
            }
            else
            {
                macInfos[index].isCycleSend = false;
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
            if (Mac_ListView.SelectedItems[0].SubItems[3].Text == "已启用")
            {
                MessageBox.Show("请先停止启用！");
                return;
            }
            isUserDisConnect = true;
            //关闭设备的套接字
            macInfos[index].socket.Disconnect(true);
            Utils.Delay(100);
            macInfos[index].socket.Close();
            RemoveMac(macInfos[index]);
        }
        
        public void Edit_Mac_Button_Click(object sender, EventArgs e)
        {
            int index = Mac_ListView.SelectedItems[0].Index;
            Form2._macInfo = macInfos[index];
            Form2 form2 = new Form2();
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
            }
            int port = 0;
            try
            {
                port = int.Parse(Server_Port_Text.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show("端口号格式不正确：" + e.Message);
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
            foreach (ListViewItem item in Mac_ListView.Items)
            {
                if (item.SubItems[3].Text == "已启用")
                {
                    MessageBox.Show("请先停止所有设备！");
                    return;
                }
            }
            serverSocket.Close();
            isUserDisConnect = true;
            for (int i = macInfos.Count - 1; i >= 0; i--)
            {
                int temp = macCount;
                Mac_Info macInfo = macInfos[i];
                macInfo.socket.Disconnect(true);
                macInfo.socket.Close();
                Utils.Delay(100);
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
            macCount++;
            //将客户端信息显示到客户端
            AddMacToListView(socket);
            //接受客户端的数据
            Thread t = new Thread(() => Form1.ReciveData(macInfo, this));
            t.IsBackground = true;
            t.Start();
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
            //删除内存中的设备
            macInfos.Remove(macInfo);
            //删除ListView中的设备s
            Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                ListViewItem item = Mac_ListView.Items[index];
                Mac_ListView.Items.Remove(item);
            })));
            //删除后自动选择
            if (index == _index)
            {
                if (macCount > 1)
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
            macCount--;
        }

        public void EditMacInfo()
        {
            while (_macInfo != null)
            {
                int index = 0;
                Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                    index = Mac_ListView.SelectedItems[0].Index;
                })));
                macInfos[index] = _macInfo;
                Mac_ListView.EndInvoke(Mac_ListView.BeginInvoke(new Action(() => {
                    Mac_ListView.SelectedItems[0].SubItems[2].Text = _macInfo.number.ToString();
                })));
                _macInfo = null;
            }
        }

        /*
         * 用于多线程的函数(静态函数，线程安全)
         */
        public static void CycleSendAndRecv(Mac_Info macInfo, Form1 form)
        {
            int index = macInfos.IndexOf(macInfo);
            //获取全部的Url
            List<string[]> totalSensorUrls = new List<string[]>();
            totalSensorUrls = Utils.GetTotalSensorUrls(xmlPath);
            while (macInfo.isCycleSend)
            {
                Utils.Delay(1000);
                //发送总召唤帧
                macInfo.socket.Send(Utils.CombineMasterCallFrame(macInfo));
                //若超过5秒钟仍没接收到消息则重发，重发3次之后显示超时
                Utils.Delay(100);
                if (macInfo.recvDataLen == 0)
                {
                    Utils.Delay(5000);
                    if (macInfo.recvDataLen == 0)
                    {
                        if (macInfo.reSendCount == 3)
                        {
                            MessageBox.Show("设备" + _index.ToString() + "超时！");
                            form.Mac_ListView.EndInvoke(form.Mac_ListView.BeginInvoke(new Action(() => {
                                form.Mac_ListView.Items[index].SubItems[3].Text = "已超时";
                            })));
                            return;
                        }
                        macInfo.socket.Send(Utils.CombineMasterCallFrame(macInfo));
                        macInfo.reSendCount += 1;
                        CycleSendAndRecv(macInfo, form);
                        return;
                    }
                }
                //获取完整的消息
                int tempDataLen = 0;
                do
                {
                    tempDataLen = macInfo.recvDataLen;
                    Utils.Delay(100);
                } while (tempDataLen != macInfo.recvDataLen);
                byte[] realData = macInfo.recvBuffer.Skip(0).Take(macInfo.recvDataLen).ToArray();
                //清除接收缓冲区
                Array.Clear(macInfo.recvBuffer, 0, macInfo.recvBuffer.Length);
                macInfo.recvDataLen = 0;
                //获取遥测帧中的传感数据并发送给http服务器
                Utils.AnalysisFrameToHttpServer(realData, totalSensorUrls[index]);
                //更新FCB
                if (macInfos[index].isChangeFCB)
                {
                    if (macInfos[index].FCB == 0)
                    {
                        macInfos[index].FCB = 1;
                    }
                    else
                    {
                        macInfos[index].FCB = 0;
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
                macInfos[index].message += recvStr;
                form.Recv_TextBox.EndInvoke(form.Recv_TextBox.BeginInvoke(new Action(() => {
                    form.Recv_TextBox.Text = macInfos[_index].message;
                })));
                macInfo.recvCount += 1;
                //若接受10次消息则清空内存
                if (macInfo.recvCount > 10)
                {
                    macInfo.message = "";
                    macInfo.recvCount = 0;
                }
            }
        }

        public static void ReciveData(Mac_Info macInfo, Form1 form)
        {
            while (true)
            {
                //判断客户端是否断开
                bool isFailed = macInfo.socket.Poll(1000, SelectMode.SelectRead);
                if (isFailed)
                {
                    if (macInfo.socket.Available <= 0)
                    {
                        //关闭设备的套接字
                        macInfo.socket.Disconnect(true);
                        macInfo.socket.Close();
                        form.RemoveMac(macInfo);
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
                    if (!isUserDisConnect)
                    {
                        //关闭设备的套接字
                        macInfo.socket.Disconnect(true);
                        macInfo.socket.Close();
                        form.RemoveMac(macInfo);
                    }
                    break;
                }
                byte[] realData = tempBuffer.Skip(0).Take(tempDataLen).ToArray();
                realData.CopyTo(macInfo.recvBuffer, macInfo.recvDataLen);
                macInfo.recvDataLen += tempDataLen;
            }
        }
    }
}
