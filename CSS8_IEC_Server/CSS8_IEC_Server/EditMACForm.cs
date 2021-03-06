using System;
using System.Windows.Forms;

namespace CSS8_IEC_Server
{
    public partial class EditMACForm : Form
    {
        private EditMac editMac;
        public static MacInfo currentSelectMacInfo = null;

        public EditMACForm(EditMac form1Delegate)
        {
            editMac = form1Delegate;
            InitializeComponent();
            Number_TextBox.Text = Byte2ToInt(currentSelectMacInfo.number).ToString();
            //修改完成按钮点击函数
            Edit_MAC_OK_Button.Click += Edit_MAC_OK_Button_Click;
            //取消按钮点击事件
            Edit_MAC_Cancle_Button.Click += Edit_MAC_Cancle_Button_Click;
        }

        /*
         * 按钮点击事件
         */
        public void Edit_MAC_OK_Button_Click(object sender, EventArgs e)
        {
            try
            {
                int intNumber = int.Parse(Number_TextBox.Text);
                currentSelectMacInfo.number = IntToByte2(intNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show("设备站号格式不正确：" + ex.Message);
                return;
            }
            editMac(currentSelectMacInfo);
            Close();
        }

        public void Edit_MAC_Cancle_Button_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 2字节转整数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Byte2ToInt(byte[] data)
        {
            int result = data[0] * 256 + data[1];
            return result;
        }

        /// <summary>
        /// 整数转2字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] IntToByte2(int data)
        {
            byte heigh = (byte)(data / 256);
            byte low = (byte)(data % 256);
            byte[] result = new byte[] { heigh, low };
            return result;
        }
    }
}
