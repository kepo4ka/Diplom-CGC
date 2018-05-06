namespace Bomber_wpf
{
    partial class StartPage
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
            this.savedGameButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mapPathLabel = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.path4_btn = new System.Windows.Forms.Button();
            this.path4_lab = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.path3_btn = new System.Windows.Forms.Button();
            this.path3_lab = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.path2_btn = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.path2_lab = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.path1_btn = new System.Windows.Forms.Button();
            this.path1_lab = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.realGameButton = new System.Windows.Forms.Button();
            this.load_custom_map = new System.Windows.Forms.Button();
            this.version_label = new System.Windows.Forms.Label();
            this.github_link = new System.Windows.Forms.LinkLabel();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // savedGameButton
            // 
            this.savedGameButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.savedGameButton.Location = new System.Drawing.Point(9, 19);
            this.savedGameButton.Name = "savedGameButton";
            this.savedGameButton.Size = new System.Drawing.Size(471, 37);
            this.savedGameButton.TabIndex = 1;
            this.savedGameButton.Text = "Запустить ранее сохранённую игру";
            this.savedGameButton.UseVisualStyleBackColor = true;
            this.savedGameButton.Click += new System.EventHandler(this.savedGameButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.groupBox2.Controls.Add(this.savedGameButton);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox2.Location = new System.Drawing.Point(12, 335);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(489, 66);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Визуализация";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.groupBox1.Controls.Add(this.mapPathLabel);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.realGameButton);
            this.groupBox1.Controls.Add(this.load_custom_map);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 317);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Игра в реальном времени";
            // 
            // mapPathLabel
            // 
            this.mapPathLabel.AutoSize = true;
            this.mapPathLabel.Location = new System.Drawing.Point(334, 283);
            this.mapPathLabel.Name = "mapPathLabel";
            this.mapPathLabel.Size = new System.Drawing.Size(132, 13);
            this.mapPathLabel.TabIndex = 17;
            this.mapPathLabel.Text = "Имя загруженной карты";
            // 
            // groupBox6
            // 
            this.groupBox6.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox6.Controls.Add(this.comboBox4);
            this.groupBox6.Controls.Add(this.label2);
            this.groupBox6.Controls.Add(this.path4_btn);
            this.groupBox6.Controls.Add(this.path4_lab);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.groupBox6.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox6.Location = new System.Drawing.Point(250, 171);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(230, 91);
            this.groupBox6.TabIndex = 16;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Игрок 4";
            // 
            // comboBox4
            // 
            this.comboBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Items.AddRange(new object[] {
            "Не использовать этого игрока",
            "Ждать ручного запуска для отладки",
            "Загрузить стратегию из файла *.cs",
            "Бот, двигающийся случайно",
            "Неподвижный Бот",
            "Бот \"туда-сюда\""});
            this.comboBox4.Location = new System.Drawing.Point(6, 18);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(218, 21);
            this.comboBox4.TabIndex = 4;
            this.comboBox4.SelectedIndexChanged += new System.EventHandler(this.comboBox4_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.Location = new System.Drawing.Point(6, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 4;
            // 
            // path4_btn
            // 
            this.path4_btn.Enabled = false;
            this.path4_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path4_btn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.path4_btn.Location = new System.Drawing.Point(6, 45);
            this.path4_btn.Name = "path4_btn";
            this.path4_btn.Size = new System.Drawing.Size(218, 24);
            this.path4_btn.TabIndex = 7;
            this.path4_btn.Text = "Выбрать файл";
            this.path4_btn.UseVisualStyleBackColor = true;
            this.path4_btn.Click += new System.EventHandler(this.path4_btn_Click);
            // 
            // path4_lab
            // 
            this.path4_lab.AutoSize = true;
            this.path4_lab.Location = new System.Drawing.Point(6, 72);
            this.path4_lab.Name = "path4_lab";
            this.path4_lab.Size = new System.Drawing.Size(0, 17);
            this.path4_lab.TabIndex = 8;
            // 
            // groupBox5
            // 
            this.groupBox5.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox5.Controls.Add(this.comboBox3);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.path3_btn);
            this.groupBox5.Controls.Add(this.path3_lab);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.groupBox5.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox5.Location = new System.Drawing.Point(10, 171);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(230, 91);
            this.groupBox5.TabIndex = 15;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Игрок 3";
            // 
            // comboBox3
            // 
            this.comboBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "Не использовать этого игрока",
            "Ждать ручного запуска для отладки",
            "Загрузить стратегию из файла *.cs",
            "Бот, двигающийся случайно",
            "Неподвижный Бот",
            "Бот \"туда-сюда\""});
            this.comboBox3.Location = new System.Drawing.Point(6, 18);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(218, 21);
            this.comboBox3.TabIndex = 4;
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.Location = new System.Drawing.Point(6, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 4;
            // 
            // path3_btn
            // 
            this.path3_btn.Enabled = false;
            this.path3_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path3_btn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.path3_btn.Location = new System.Drawing.Point(6, 45);
            this.path3_btn.Name = "path3_btn";
            this.path3_btn.Size = new System.Drawing.Size(218, 24);
            this.path3_btn.TabIndex = 5;
            this.path3_btn.Text = "Выбрать файл";
            this.path3_btn.UseVisualStyleBackColor = true;
            this.path3_btn.Click += new System.EventHandler(this.path3_btn_Click);
            // 
            // path3_lab
            // 
            this.path3_lab.AutoSize = true;
            this.path3_lab.Location = new System.Drawing.Point(7, 72);
            this.path3_lab.Name = "path3_lab";
            this.path3_lab.Size = new System.Drawing.Size(0, 17);
            this.path3_lab.TabIndex = 6;
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox4.Controls.Add(this.path2_btn);
            this.groupBox4.Controls.Add(this.comboBox2);
            this.groupBox4.Controls.Add(this.path2_lab);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.groupBox4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox4.Location = new System.Drawing.Point(250, 78);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(230, 91);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Игрок 2";
            // 
            // path2_btn
            // 
            this.path2_btn.Enabled = false;
            this.path2_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path2_btn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.path2_btn.Location = new System.Drawing.Point(6, 47);
            this.path2_btn.Name = "path2_btn";
            this.path2_btn.Size = new System.Drawing.Size(218, 24);
            this.path2_btn.TabIndex = 9;
            this.path2_btn.Text = "Выбрать файл";
            this.path2_btn.UseVisualStyleBackColor = true;
            this.path2_btn.Click += new System.EventHandler(this.path2_btn_Click_1);
            // 
            // comboBox2
            // 
            this.comboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "Не использовать этого игрока",
            "Ждать ручного запуска для отладки",
            "Загрузить стратегию из файла *.cs",
            "Бот, двигающийся случайно",
            "Неподвижный Бот",
            "Бот \"туда-сюда\""});
            this.comboBox2.Location = new System.Drawing.Point(6, 18);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(218, 21);
            this.comboBox2.TabIndex = 4;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // path2_lab
            // 
            this.path2_lab.AutoSize = true;
            this.path2_lab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path2_lab.Location = new System.Drawing.Point(6, 74);
            this.path2_lab.Name = "path2_lab";
            this.path2_lab.Size = new System.Drawing.Size(0, 13);
            this.path2_lab.TabIndex = 4;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox3.Controls.Add(this.path1_btn);
            this.groupBox3.Controls.Add(this.path1_lab);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox3.Location = new System.Drawing.Point(10, 78);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(230, 91);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Игрок 1";
            // 
            // path1_btn
            // 
            this.path1_btn.Enabled = false;
            this.path1_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path1_btn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.path1_btn.Location = new System.Drawing.Point(6, 47);
            this.path1_btn.Name = "path1_btn";
            this.path1_btn.Size = new System.Drawing.Size(218, 24);
            this.path1_btn.TabIndex = 10;
            this.path1_btn.Text = "Выбрать файл";
            this.path1_btn.UseVisualStyleBackColor = true;
            this.path1_btn.Click += new System.EventHandler(this.path1_btn_Click_1);
            // 
            // path1_lab
            // 
            this.path1_lab.AutoSize = true;
            this.path1_lab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.path1_lab.Location = new System.Drawing.Point(7, 72);
            this.path1_lab.Name = "path1_lab";
            this.path1_lab.Size = new System.Drawing.Size(0, 13);
            this.path1_lab.TabIndex = 3;
            this.path1_lab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.SystemColors.Window;
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Не использовать этого игрока",
            "Ждать ручного запуска для отладки",
            "Загрузить стратегию из файла *.cs",
            "Бот, двигающийся случайно",
            "Неподвижный Бот",
            "Бот \"туда-сюда\""});
            this.comboBox1.Location = new System.Drawing.Point(6, 18);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(218, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // realGameButton
            // 
            this.realGameButton.Location = new System.Drawing.Point(12, 19);
            this.realGameButton.Name = "realGameButton";
            this.realGameButton.Size = new System.Drawing.Size(471, 53);
            this.realGameButton.TabIndex = 0;
            this.realGameButton.Text = "Запустить игру в реальном времени";
            this.realGameButton.UseVisualStyleBackColor = true;
            this.realGameButton.Click += new System.EventHandler(this.realGameButton_Click);
            // 
            // load_custom_map
            // 
            this.load_custom_map.Location = new System.Drawing.Point(169, 268);
            this.load_custom_map.Name = "load_custom_map";
            this.load_custom_map.Size = new System.Drawing.Size(149, 43);
            this.load_custom_map.TabIndex = 0;
            this.load_custom_map.Text = "Загрузить Свою карту";
            this.load_custom_map.UseVisualStyleBackColor = true;
            this.load_custom_map.Click += new System.EventHandler(this.load_custom_map_Click);
            // 
            // version_label
            // 
            this.version_label.AutoSize = true;
            this.version_label.Location = new System.Drawing.Point(9, 410);
            this.version_label.Name = "version_label";
            this.version_label.Size = new System.Drawing.Size(0, 13);
            this.version_label.TabIndex = 3;
            // 
            // github_link
            // 
            this.github_link.AutoSize = true;
            this.github_link.Location = new System.Drawing.Point(351, 410);
            this.github_link.Name = "github_link";
            this.github_link.Size = new System.Drawing.Size(150, 13);
            this.github_link.TabIndex = 4;
            this.github_link.TabStop = true;
            this.github_link.Text = "Скачать последнюю версию";
            this.github_link.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.github_link_LinkClicked);
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 432);
            this.Controls.Add(this.github_link);
            this.Controls.Add(this.version_label);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "StartPage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bomberman Menu";
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button savedGameButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button load_custom_map;
        private System.Windows.Forms.Label path4_lab;
        private System.Windows.Forms.Button path4_btn;
        private System.Windows.Forms.Label path3_lab;
        private System.Windows.Forms.Button path3_btn;
        private System.Windows.Forms.Label path2_lab;
        private System.Windows.Forms.Label path1_lab;
        private System.Windows.Forms.Button realGameButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button path2_btn;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button path1_btn;
        private System.Windows.Forms.Label mapPathLabel;
        private System.Windows.Forms.Label version_label;
        private System.Windows.Forms.LinkLabel github_link;
    }
}