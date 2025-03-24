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
            groupBox2 = new GroupBox();
            comboBox2 = new ComboBox();
            button1 = new Button();
            progressBar1 = new ProgressBar();
            groupBox3 = new GroupBox();
            button2 = new Button();
            textBox1 = new TextBox();
            groupBox5 = new GroupBox();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            textBoxLog = new TextBox();
            btnCleanup = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox4 = new GroupBox();
            textBox2 = new TextBox();
            groupBox1 = new GroupBox();
            comboBox1 = new ComboBox();
            tabPage2 = new TabPage();
            groupBox6 = new GroupBox();
            comboBox3 = new ComboBox();
            label1 = new Label();
            tabPage3 = new TabPage();
            groupBox8 = new GroupBox();
            comboBox4 = new ComboBox();
            groupBox7 = new GroupBox();
            textBox3 = new TextBox();
            tabPage4 = new TabPage();
            label2 = new Label();
            groupBox9 = new GroupBox();
            textBox5 = new TextBox();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox5.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox6.SuspendLayout();
            tabPage3.SuspendLayout();
            groupBox8.SuspendLayout();
            groupBox7.SuspendLayout();
            tabPage4.SuspendLayout();
            groupBox9.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(comboBox2);
            groupBox2.Location = new Point(5, 286);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(388, 63);
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
            comboBox2.Size = new Size(373, 25);
            comboBox2.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(5, 355);
            button1.Name = "button1";
            button1.Size = new Size(388, 23);
            button1.TabIndex = 2;
            button1.Text = "开始部署";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(5, 384);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(388, 23);
            progressBar1.TabIndex = 3;
            progressBar1.Click += progressBar1_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(button2);
            groupBox3.Location = new Point(5, 442);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(388, 51);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "已部署？点此启动";
            // 
            // button2
            // 
            button2.Location = new Point(6, 22);
            button2.Name = "button2";
            button2.Size = new Size(373, 23);
            button2.TabIndex = 0;
            button2.Text = "启动项目";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(5, 413);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(388, 23);
            textBox1.TabIndex = 5;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(button5);
            groupBox5.Controls.Add(button4);
            groupBox5.Controls.Add(button3);
            groupBox5.Location = new Point(12, 12);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(381, 63);
            groupBox5.TabIndex = 7;
            groupBox5.TabStop = false;
            groupBox5.Text = "扩展设置区";
            // 
            // button5
            // 
            button5.Location = new Point(172, 22);
            button5.Name = "button5";
            button5.Size = new Size(90, 35);
            button5.TabIndex = 2;
            button5.Text = "部署视频教程";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(87, 22);
            button4.Name = "button4";
            button4.Size = new Size(79, 35);
            button4.TabIndex = 1;
            button4.Text = "报错帮助页";
            button4.UseVisualStyleBackColor = true;
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
            // textBoxLog
            // 
            textBoxLog.Location = new Point(5, 499);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(388, 133);
            textBoxLog.TabIndex = 8;
            // 
            // btnCleanup
            // 
            btnCleanup.Location = new Point(5, 638);
            btnCleanup.Name = "btnCleanup";
            btnCleanup.Size = new Size(388, 23);
            btnCleanup.TabIndex = 9;
            btnCleanup.Text = "清理项目";
            btnCleanup.UseVisualStyleBackColor = true;
            btnCleanup.Click += btnCleanup_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(1, 81);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(396, 199);
            tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Location = new Point(4, 26);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(388, 169);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Dev方式部署";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(textBox2);
            groupBox4.Location = new Point(6, 6);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(365, 62);
            groupBox4.TabIndex = 8;
            groupBox4.TabStop = false;
            groupBox4.Text = "填写仓库URL";
            // 
            // textBox2
            // 
            textBox2.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            textBox2.Location = new Point(6, 22);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(353, 28);
            textBox2.TabIndex = 0;
            textBox2.Text = "https://github.com/1Eliver/KouriChat-dev";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Location = new Point(6, 74);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(365, 58);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "选择下载分支";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(6, 22);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(353, 25);
            comboBox1.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(groupBox6);
            tabPage2.Controls.Add(label1);
            tabPage2.Location = new Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(388, 169);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "内置版本部署";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(comboBox3);
            groupBox6.Location = new Point(7, 91);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(372, 67);
            groupBox6.TabIndex = 1;
            groupBox6.TabStop = false;
            groupBox6.Text = "选择内置版本";
            // 
            // comboBox3
            // 
            comboBox3.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(6, 22);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(360, 29);
            comboBox3.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(342, 85);
            label1.TabIndex = 0;
            label1.Text = "Tip：\r\n内置版本部署是不支持通过downloader更新功能进行更新的捏\r\n如果需要更新请选择托管源或者Dev方式部署\r\n（指不能在本菜单栏使用更新\r\n内置版本部署之后可以使用别的如Url直链部署进行更新）\r\n";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(groupBox8);
            tabPage3.Controls.Add(groupBox7);
            tabPage3.Location = new Point(4, 26);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(388, 169);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "托管源方式部署";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(comboBox4);
            groupBox8.Location = new Point(13, 88);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(366, 69);
            groupBox8.TabIndex = 1;
            groupBox8.TabStop = false;
            groupBox8.Text = "输入部署版本";
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(6, 38);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(354, 25);
            comboBox4.TabIndex = 0;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(textBox3);
            groupBox7.Location = new Point(13, 13);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(366, 69);
            groupBox7.TabIndex = 0;
            groupBox7.TabStop = false;
            groupBox7.Text = "输入托管源base_url";
            // 
            // textBox3
            // 
            textBox3.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            textBox3.Location = new Point(6, 31);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(354, 23);
            textBox3.TabIndex = 0;
            textBox3.Text = "https://static.ciallo.ac.cn";
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(label2);
            tabPage4.Controls.Add(groupBox9);
            tabPage4.Location = new Point(4, 26);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(388, 169);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Url直链部署";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(265, 85);
            label2.TabIndex = 1;
            label2.Text = "请从官方群或官方文档处获取Url\r\n除官方提供的Url进行安装的版本官方无托管责任\r\n请细心辨别\r\n如若从自称开发人员处获取Url\r\n请到官方群求证是否是真正的开发组成员\r\n";
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(textBox5);
            groupBox9.Location = new Point(7, 88);
            groupBox9.Name = "groupBox9";
            groupBox9.Size = new Size(377, 78);
            groupBox9.TabIndex = 0;
            groupBox9.TabStop = false;
            groupBox9.Text = "请输入项目包Url地址";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(6, 33);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(365, 23);
            textBox5.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(401, 666);
            Controls.Add(tabControl1);
            Controls.Add(btnCleanup);
            Controls.Add(textBoxLog);
            Controls.Add(groupBox5);
            Controls.Add(textBox1);
            Controls.Add(groupBox3);
            Controls.Add(progressBar1);
            Controls.Add(button1);
            Controls.Add(groupBox2);
            MinimumSize = new Size(348, 600);
            Name = "Form1";
            Text = "KouriChat-Downloader";
            Load += Form1_Load;
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            groupBox6.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            groupBox8.ResumeLayout(false);
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            groupBox9.ResumeLayout(false);
            groupBox9.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox groupBox2;
        private ComboBox comboBox2;
        private Button button1;
        private ProgressBar progressBar1;
        private GroupBox groupBox3;
        private Button button2;
        private TextBox textBox1;
        private GroupBox groupBox5;
        private Button button3;
        private TextBox textBoxLog;
        private Button btnCleanup;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private GroupBox groupBox4;
        private TextBox textBox2;
        private GroupBox groupBox1;
        private ComboBox comboBox1;
        private TabPage tabPage2;
        private GroupBox groupBox6;
        private ComboBox comboBox3;
        private Label label1;
        private TabPage tabPage3;
        private GroupBox groupBox8;
        private GroupBox groupBox7;
        private TextBox textBox3;
        private Button button5;
        private Button button4;
        private TabPage tabPage4;
        private Label label2;
        private GroupBox groupBox9;
        private TextBox textBox5;
        private Button button6;
        private ComboBox comboBox4;
    }
}
