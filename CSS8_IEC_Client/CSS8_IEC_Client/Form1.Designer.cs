
namespace CSS8_IEC_Client
{
    partial class Client_Form
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
            this.Mac_ListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.Mac_Name_Column = new System.Windows.Forms.ColumnHeader();
            this.Mac_Number_Column = new System.Windows.Forms.ColumnHeader();
            this.Mac_State_Column = new System.Windows.Forms.ColumnHeader();
            this.Recv_TextBox = new System.Windows.Forms.TextBox();
            this.Recv_Label = new System.Windows.Forms.Label();
            this.Clear_Recv_Str_Button = new System.Windows.Forms.Button();
            this.Connect_Button = new System.Windows.Forms.Button();
            this.Disconnect_Button = new System.Windows.Forms.Button();
            this.Refresh_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Mac_ListView
            // 
            this.Mac_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.Mac_Name_Column,
            this.Mac_Number_Column,
            this.Mac_State_Column});
            this.Mac_ListView.FullRowSelect = true;
            this.Mac_ListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.Mac_ListView.HideSelection = false;
            this.Mac_ListView.Location = new System.Drawing.Point(12, 42);
            this.Mac_ListView.Name = "Mac_ListView";
            this.Mac_ListView.Size = new System.Drawing.Size(407, 378);
            this.Mac_ListView.TabIndex = 2;
            this.Mac_ListView.UseCompatibleStateImageBehavior = false;
            this.Mac_ListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 0;
            // 
            // Mac_Name_Column
            // 
            this.Mac_Name_Column.Text = "设备名称";
            this.Mac_Name_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_Name_Column.Width = 140;
            // 
            // Mac_Number_Column
            // 
            this.Mac_Number_Column.Text = "设备站号";
            this.Mac_Number_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_Number_Column.Width = 140;
            // 
            // Mac_State_Column
            // 
            this.Mac_State_Column.Tag = "";
            this.Mac_State_Column.Text = "设备状态";
            this.Mac_State_Column.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mac_State_Column.Width = 120;
            // 
            // Recv_TextBox
            // 
            this.Recv_TextBox.Location = new System.Drawing.Point(443, 42);
            this.Recv_TextBox.Multiline = true;
            this.Recv_TextBox.Name = "Recv_TextBox";
            this.Recv_TextBox.ReadOnly = true;
            this.Recv_TextBox.Size = new System.Drawing.Size(453, 377);
            this.Recv_TextBox.TabIndex = 4;
            // 
            // Recv_Label
            // 
            this.Recv_Label.AutoSize = true;
            this.Recv_Label.Location = new System.Drawing.Point(443, 16);
            this.Recv_Label.Name = "Recv_Label";
            this.Recv_Label.Size = new System.Drawing.Size(92, 17);
            this.Recv_Label.TabIndex = 5;
            this.Recv_Label.Text = "接收到的消息：";
            // 
            // Clear_Recv_Str_Button
            // 
            this.Clear_Recv_Str_Button.Location = new System.Drawing.Point(784, 12);
            this.Clear_Recv_Str_Button.Name = "Clear_Recv_Str_Button";
            this.Clear_Recv_Str_Button.Size = new System.Drawing.Size(112, 24);
            this.Clear_Recv_Str_Button.TabIndex = 6;
            this.Clear_Recv_Str_Button.Text = "清除";
            this.Clear_Recv_Str_Button.UseVisualStyleBackColor = true;
            // 
            // Connect_Button
            // 
            this.Connect_Button.Location = new System.Drawing.Point(12, 12);
            this.Connect_Button.Name = "Connect_Button";
            this.Connect_Button.Size = new System.Drawing.Size(88, 24);
            this.Connect_Button.TabIndex = 8;
            this.Connect_Button.Text = "一键连接";
            this.Connect_Button.UseVisualStyleBackColor = true;
            // 
            // Disconnect_Button
            // 
            this.Disconnect_Button.Location = new System.Drawing.Point(169, 12);
            this.Disconnect_Button.Name = "Disconnect_Button";
            this.Disconnect_Button.Size = new System.Drawing.Size(88, 24);
            this.Disconnect_Button.TabIndex = 9;
            this.Disconnect_Button.Text = "一键断开";
            this.Disconnect_Button.UseVisualStyleBackColor = true;
            // 
            // Refresh_Button
            // 
            this.Refresh_Button.Location = new System.Drawing.Point(331, 12);
            this.Refresh_Button.Name = "Refresh_Button";
            this.Refresh_Button.Size = new System.Drawing.Size(88, 24);
            this.Refresh_Button.TabIndex = 10;
            this.Refresh_Button.Text = "重置设备";
            this.Refresh_Button.UseVisualStyleBackColor = true;
            // 
            // Client_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 450);
            this.Controls.Add(this.Refresh_Button);
            this.Controls.Add(this.Disconnect_Button);
            this.Controls.Add(this.Connect_Button);
            this.Controls.Add(this.Clear_Recv_Str_Button);
            this.Controls.Add(this.Recv_Label);
            this.Controls.Add(this.Recv_TextBox);
            this.Controls.Add(this.Mac_ListView);
            this.Name = "Client_Form";
            this.Text = "客户端";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView Mac_ListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader Mac_Name_Column;
        private System.Windows.Forms.ColumnHeader Mac_State_Column;
        private System.Windows.Forms.ColumnHeader Mac_Number_Column;
        private System.Windows.Forms.TextBox Recv_TextBox;
        private System.Windows.Forms.Label Recv_Label;
        private System.Windows.Forms.Button Clear_Recv_Str_Button;
        private System.Windows.Forms.Button Connect_Button;
        private System.Windows.Forms.Button Disconnect_Button;
        private System.Windows.Forms.Button Refresh_Button;
    }
}

