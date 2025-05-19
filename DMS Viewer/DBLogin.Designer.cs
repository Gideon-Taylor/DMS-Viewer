﻿namespace DMS_Viewer
{
    partial class DBLogin
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
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDBUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDBPass = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDBConnect = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSchema = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtDBName
            // 
            this.txtDBName.Location = new System.Drawing.Point(92, 32);
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Size = new System.Drawing.Size(135, 20);
            this.txtDBName.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "DB Name:";
            // 
            // txtDBUser
            // 
            this.txtDBUser.Location = new System.Drawing.Point(92, 84);
            this.txtDBUser.Name = "txtDBUser";
            this.txtDBUser.Size = new System.Drawing.Size(135, 20);
            this.txtDBUser.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "User Name:";
            // 
            // txtDBPass
            // 
            this.txtDBPass.Location = new System.Drawing.Point(92, 110);
            this.txtDBPass.Name = "txtDBPass";
            this.txtDBPass.PasswordChar = '*';
            this.txtDBPass.Size = new System.Drawing.Size(135, 20);
            this.txtDBPass.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password:";
            // 
            // btnDBConnect
            // 
            this.btnDBConnect.Location = new System.Drawing.Point(83, 147);
            this.btnDBConnect.Name = "btnDBConnect";
            this.btnDBConnect.Size = new System.Drawing.Size(90, 23);
            this.btnDBConnect.TabIndex = 21;
            this.btnDBConnect.Text = "Connect";
            this.btnDBConnect.UseVisualStyleBackColor = true;
            this.btnDBConnect.Click += new System.EventHandler(this.btnDBConnect_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(193, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Use Bootstrap or Read Only credentials";
            // 
            // txtSchema
            // 
            this.txtSchema.Location = new System.Drawing.Point(92, 58);
            this.txtSchema.Name = "txtSchema";
            this.txtSchema.Size = new System.Drawing.Size(135, 20);
            this.txtSchema.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Bootstrap User:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // DBLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 184);
            this.Controls.Add(this.txtSchema);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnDBConnect);
            this.Controls.Add(this.txtDBPass);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDBUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtDBName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DBLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDBName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDBUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDBPass;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDBConnect;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSchema;
        private System.Windows.Forms.Label label5;
    }
}