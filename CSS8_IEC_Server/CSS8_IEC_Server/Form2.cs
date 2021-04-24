﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CSS8_IEC_Server
{
    public partial class Form2 : Form
    {
        public static Mac_Info _macInfo = null;
        public Form2()
        {
            InitializeComponent();
            Number_TextBox.Text = _macInfo.number.ToString();
            //修改完成按钮点击函数
            Edit_MAC_OK_Button.Click += Edit_MAC_OK_Button_Click;
        }

        public void Edit_MAC_OK_Button_Click(object sender, EventArgs e)
        {
            Mac_Info macInfo = new Mac_Info();
            macInfo = _macInfo;
            try
            {
                macInfo.number = int.Parse(Number_TextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("设备站号格式不正确：" + ex.Message);
                return;
            }
            Form1._macInfo = macInfo;
            _macInfo = null;
            Close();
        }
    }
}
