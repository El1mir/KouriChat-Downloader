namespace KouriChat_Downloader
{
    partial class Form1
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
            groupBox1 = new GroupBox();
            comboBox1 = new ComboBox();
            groupBox2 = new GroupBox();
            comboBox2 = new ComboBox();
            button1 = new Button();
            progressBar1 = new ProgressBar();
            groupBox3 = new GroupBox();
            button2 = new Button();
            textBox1 = new TextBox();
            groupBox4 = new GroupBox();
            textBox2 = new TextBox();
            groupBox5 = new GroupBox();
            button3 = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Location = new Point(12, 147);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(308, 58);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "选择下载分支";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(6, 22);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(296, 25);
            comboBox1.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(comboBox2);
            groupBox2.Location = new Point(12, 211);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(308, 63);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "选择python版本";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(6, 22);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(296, 25);
            comboBox2.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(12, 280);
            button1.Name = "button1";
            button1.Size = new Size(308, 23);
            button1.TabIndex = 2;
            button1.Text = "开始部署";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 309);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(308, 23);
            progressBar1.TabIndex = 3;
            progressBar1.Click += progressBar1_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(button2);
            groupBox3.Location = new Point(12, 367);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(308, 51);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "已部署？点此启动";
            // 
            // button2
            // 
            button2.Location = new Point(6, 22);
            button2.Name = "button2";
            button2.Size = new Size(296, 23);
            button2.TabIndex = 0;
            button2.Text = "启动项目";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 338);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(308, 23);
            textBox1.TabIndex = 5;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(textBox2);
            groupBox4.Location = new Point(12, 81);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(308, 60);
            groupBox4.TabIndex = 6;
            groupBox4.TabStop = false;
            groupBox4.Text = "填写仓库URL";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(6, 21);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(296, 23);
            textBox2.TabIndex = 0;
            textBox2.Text = "https://github.com/1Eliver/KouriChat-dev";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(button3);
            groupBox5.Location = new Point(12, 12);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(308, 63);
            groupBox5.TabIndex = 7;
            groupBox5.TabStop = false;
            groupBox5.Text = "扩展设置区";
            // 
            // button3
            // 
            button3.Location = new Point(6, 22);
            button3.Name = "button3";
            button3.Size = new Size(75, 35);
            button3.TabIndex = 0;
            button3.Text = "更新项目";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(332, 492);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(textBox1);
            Controls.Add(groupBox3);
            Controls.Add(progressBar1);
            Controls.Add(button1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = "KouriChat-Downloader";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private ComboBox comboBox1;
        private GroupBox groupBox2;
        private ComboBox comboBox2;
        private Button button1;
        private ProgressBar progressBar1;
        private GroupBox groupBox3;
        private Button button2;
        private TextBox textBox1;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private Button button3;
    }
}
