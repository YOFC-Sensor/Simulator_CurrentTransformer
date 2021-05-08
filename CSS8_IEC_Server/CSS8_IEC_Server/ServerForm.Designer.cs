
namespace CSS8_IEC_Server
{
    partial class ServerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Recv_Data_Label = new System.Windows.Forms.Label();
            this.Recv_TextBox = new System.Windows.Forms.TextBox();
            this.Mac_ListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.Mac_Address_Column = new System.Windows.Forms.ColumnHeader();
            this.Mac_Number_Column = new System.Windows.Forms.ColumnHeader();
            this.Mac_Satae_Column = new System.Windows.Forms.ColumnHeader();
            this.DisConnect_Button = new System.Windows.Forms.Button();
            this.Edit_Mac_Button = new System.Windows.Forms.Button();
            this.Start_Server_Button = new System.Windows.Forms.Button();
            this.Get_Data_Button = new System.Windows.Forms.Button();
            this.Server_IP_Label = new System.Windows.Forms.Label();
            this.Server_Port_Label = new System.Windows.Forms.Label();
            this.Server_State = new System.Windows.Forms.Label();
            this.MAC_State = new System.Windows.Forms.Label();
            this.Server_IP_TextBox = new System.Windows.Forms.TextBox();
            this.Server_Port_Text = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Recv_Data_Label
            // 
            this.Recv_Data_Label.AutoSize = true;
            this.Recv_Data_Label.Location = new System.Drawing.Point(332, 28);
            this.Recv_Data_Label.Name = "Recv_Data_Label";
            this.Recv_Data_Label.Size = new System.Drawing.Size(80, 17);
            this.Recv_Data_Label.TabIndex = 8;
            this.Recv_Data_Label.Text = "接收的数据：";
            // 
            // Recv_TextBox
            // 
            this.Recv_TextBox.Location = new System.Drawing.Point(332, 51);
            this.Recv_TextBox.Multiline = true;
            this.Recv_TextBox.Name = "Recv_TextBox";
            this.Recv_TextBox.ReadOnly = true;
            this.Recv_TextBox.Size = new System.Drawing.Size(439, 349);
            this.Recv_TextBox.TabIndex = 7;
            // 
            // Mac_ListView
            // 
            this.Mac_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.Mac_Address_Column,
            this.Mac_Number_Column,
            this.Mac_Satae_Column});
            this.Mac_ListView.FullRowSelect = true;
            this.Mac_ListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.Mac_ListView.HideSelection = false;
            this.Mac_ListView.Location = new System.Drawing.Point(29, 51);
            this.Mac_ListView.Name = "Mac_ListView";
            this.Mac_ListView.Size = new System.Drawing.Size(297, 349);
            this.Mac_ListView.TabIndex = 6;
            this.Mac_ListView.UseCompatibleStateImageBehavior = false;
            this.Mac_ListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 0;
            // 
            // Mac_Address_Column
            // 
            this.Mac_Address_Column.Text = "设备地址";
            this.Mac_Address_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_Address_Column.Width = 100;
            // 
            // Mac_Number_Column
            // 
            this.Mac_Number_Column.Text = "设备站号";
            this.Mac_Number_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_Number_Column.Width = 100;
            // 
            // Mac_Satae_Column
            // 
            this.Mac_Satae_Column.Text = "设备状态";
            this.Mac_Satae_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_Satae_Column.Width = 80;
            // 
            // DisConnect_Button
            // 
            this.DisConnect_Button.Enabled = false;
            this.DisConnect_Button.Location = new System.Drawing.Point(29, 21);
            this.DisConnect_Button.Name = "DisConnect_Button";
            this.DisConnect_Button.Size = new System.Drawing.Size(86, 24);
            this.DisConnect_Button.TabIndex = 5;
            this.DisConnect_Button.Text = "断开设备";
            this.DisConnect_Button.UseVisualStyleBackColor = true;
            // 
            // Edit_Mac_Button
            // 
            this.Edit_Mac_Button.Enabled = false;
            this.Edit_Mac_Button.Location = new System.Drawing.Point(241, 21);
            this.Edit_Mac_Button.Name = "Edit_Mac_Button";
            this.Edit_Mac_Button.Size = new System.Drawing.Size(85, 24);
            this.Edit_Mac_Button.TabIndex = 12;
            this.Edit_Mac_Button.Text = "修改站号";
            this.Edit_Mac_Button.UseVisualStyleBackColor = true;
            // 
            // Start_Server_Button
            // 
            this.Start_Server_Button.Location = new System.Drawing.Point(686, 415);
            this.Start_Server_Button.Name = "Start_Server_Button";
            this.Start_Server_Button.Size = new System.Drawing.Size(86, 23);
            this.Start_Server_Button.TabIndex = 16;
            this.Start_Server_Button.Text = "开启服务";
            this.Start_Server_Button.UseVisualStyleBackColor = true;
            // 
            // Get_Data_Button
            // 
            this.Get_Data_Button.Enabled = false;
            this.Get_Data_Button.Location = new System.Drawing.Point(690, 21);
            this.Get_Data_Button.Name = "Get_Data_Button";
            this.Get_Data_Button.Size = new System.Drawing.Size(81, 23);
            this.Get_Data_Button.TabIndex = 17;
            this.Get_Data_Button.Text = "获取数据";
            this.Get_Data_Button.UseVisualStyleBackColor = true;
            // 
            // Server_IP_Label
            // 
            this.Server_IP_Label.AutoSize = true;
            this.Server_IP_Label.Location = new System.Drawing.Point(333, 418);
            this.Server_IP_Label.Name = "Server_IP_Label";
            this.Server_IP_Label.Size = new System.Drawing.Size(31, 17);
            this.Server_IP_Label.TabIndex = 20;
            this.Server_IP_Label.Text = "IP：";
            // 
            // Server_Port_Label
            // 
            this.Server_Port_Label.AutoSize = true;
            this.Server_Port_Label.Location = new System.Drawing.Point(501, 418);
            this.Server_Port_Label.Name = "Server_Port_Label";
            this.Server_Port_Label.Size = new System.Drawing.Size(56, 17);
            this.Server_Port_Label.TabIndex = 22;
            this.Server_Port_Label.Text = "端口号：";
            // 
            // Server_State
            // 
            this.Server_State.AutoSize = true;
            this.Server_State.Location = new System.Drawing.Point(137, 24);
            this.Server_State.Name = "Server_State";
            this.Server_State.Size = new System.Drawing.Size(80, 17);
            this.Server_State.TabIndex = 24;
            this.Server_State.Text = "服务器未开启";
            // 
            // MAC_State
            // 
            this.MAC_State.AutoSize = true;
            this.MAC_State.Location = new System.Drawing.Point(500, 25);
            this.MAC_State.Name = "MAC_State";
            this.MAC_State.Size = new System.Drawing.Size(0, 17);
            this.MAC_State.TabIndex = 25;
            // 
            // Server_IP_TextBox
            // 
            this.Server_IP_TextBox.Location = new System.Drawing.Point(370, 415);
            this.Server_IP_TextBox.Name = "Server_IP_TextBox";
            this.Server_IP_TextBox.Size = new System.Drawing.Size(100, 23);
            this.Server_IP_TextBox.TabIndex = 26;
            // 
            // Server_Port_Text
            // 
            this.Server_Port_Text.Location = new System.Drawing.Point(563, 415);
            this.Server_Port_Text.Name = "Server_Port_Text";
            this.Server_Port_Text.Size = new System.Drawing.Size(100, 23);
            this.Server_Port_Text.TabIndex = 27;
            // 
            // Server_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Server_Port_Text);
            this.Controls.Add(this.Server_IP_TextBox);
            this.Controls.Add(this.MAC_State);
            this.Controls.Add(this.Server_State);
            this.Controls.Add(this.Server_Port_Label);
            this.Controls.Add(this.Server_IP_Label);
            this.Controls.Add(this.Get_Data_Button);
            this.Controls.Add(this.Start_Server_Button);
            this.Controls.Add(this.Edit_Mac_Button);
            this.Controls.Add(this.Recv_Data_Label);
            this.Controls.Add(this.Recv_TextBox);
            this.Controls.Add(this.Mac_ListView);
            this.Controls.Add(this.DisConnect_Button);
            this.Name = "Server_Form";
            this.Text = "服务器";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Recv_Data_Label;
        private System.Windows.Forms.TextBox Recv_TextBox;
        private System.Windows.Forms.ListView Mac_ListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader Mac_Address_Column;
        private System.Windows.Forms.Button DisConnect_Button;
        private System.Windows.Forms.ColumnHeader Mac_Number_Column;
        private System.Windows.Forms.ColumnHeader Mac_Satae_Column;
        private System.Windows.Forms.Button Edit_Mac_Button;
        private System.Windows.Forms.Button Start_Server_Button;
        private System.Windows.Forms.Button Get_Data_Button;
        private System.Windows.Forms.Label Server_IP_Label;
        private System.Windows.Forms.Label Server_Port_Label;
        private System.Windows.Forms.Label Server_State;
        private System.Windows.Forms.Label MAC_State;
        private System.Windows.Forms.TextBox Server_IP_TextBox;
        private System.Windows.Forms.TextBox Server_Port_Text;
    }
}

