
namespace CSS8_IEC_Server
{
    partial class EditMACForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Number_TextBox = new System.Windows.Forms.TextBox();
            this.Number_Label = new System.Windows.Forms.Label();
            this.Edit_MAC_OK_Button = new System.Windows.Forms.Button();
            this.Edit_MAC_Cancle_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Number_TextBox
            // 
            this.Number_TextBox.Location = new System.Drawing.Point(155, 37);
            this.Number_TextBox.Name = "Number_TextBox";
            this.Number_TextBox.Size = new System.Drawing.Size(121, 23);
            this.Number_TextBox.TabIndex = 0;
            // 
            // Number_Label
            // 
            this.Number_Label.AutoSize = true;
            this.Number_Label.Location = new System.Drawing.Point(81, 40);
            this.Number_Label.Name = "Number_Label";
            this.Number_Label.Size = new System.Drawing.Size(68, 17);
            this.Number_Label.TabIndex = 1;
            this.Number_Label.Text = "设备站号：";
            // 
            // Edit_MAC_OK_Button
            // 
            this.Edit_MAC_OK_Button.Location = new System.Drawing.Point(34, 104);
            this.Edit_MAC_OK_Button.Name = "Edit_MAC_OK_Button";
            this.Edit_MAC_OK_Button.Size = new System.Drawing.Size(75, 23);
            this.Edit_MAC_OK_Button.TabIndex = 2;
            this.Edit_MAC_OK_Button.Text = "修改";
            this.Edit_MAC_OK_Button.UseVisualStyleBackColor = true;
            // 
            // Edit_MAC_Cancle_Button
            // 
            this.Edit_MAC_Cancle_Button.Location = new System.Drawing.Point(281, 104);
            this.Edit_MAC_Cancle_Button.Name = "Edit_MAC_Cancle_Button";
            this.Edit_MAC_Cancle_Button.Size = new System.Drawing.Size(75, 23);
            this.Edit_MAC_Cancle_Button.TabIndex = 3;
            this.Edit_MAC_Cancle_Button.Text = "取消";
            this.Edit_MAC_Cancle_Button.UseVisualStyleBackColor = true;
            // 
            // Edit_MAC_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 135);
            this.Controls.Add(this.Edit_MAC_Cancle_Button);
            this.Controls.Add(this.Edit_MAC_OK_Button);
            this.Controls.Add(this.Number_Label);
            this.Controls.Add(this.Number_TextBox);
            this.Name = "Edit_MAC_Form";
            this.Text = "修改设备";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Number_TextBox;
        private System.Windows.Forms.Label Number_Label;
        private System.Windows.Forms.Button Edit_MAC_OK_Button;
        private System.Windows.Forms.Button Edit_MAC_Cancle_Button;
    }
}