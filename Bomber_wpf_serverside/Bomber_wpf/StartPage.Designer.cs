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
            this.load_custom_map = new System.Windows.Forms.Button();
            this.checkBot4 = new System.Windows.Forms.CheckBox();
            this.checkBot3 = new System.Windows.Forms.CheckBox();
            this.checkBot2 = new System.Windows.Forms.CheckBox();
            this.checkBot1 = new System.Windows.Forms.CheckBox();
            this.path4_lab = new System.Windows.Forms.Label();
            this.path4_btn = new System.Windows.Forms.Button();
            this.path3_lab = new System.Windows.Forms.Label();
            this.path3_btn = new System.Windows.Forms.Button();
            this.path2_lab = new System.Windows.Forms.Label();
            this.path1_lab = new System.Windows.Forms.Label();
            this.path2_btn = new System.Windows.Forms.Button();
            this.path1_btn = new System.Windows.Forms.Button();
            this.realGameButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // savedGameButton
            // 
            this.savedGameButton.Location = new System.Drawing.Point(9, 19);
            this.savedGameButton.Name = "savedGameButton";
            this.savedGameButton.Size = new System.Drawing.Size(267, 37);
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
            this.groupBox2.Location = new System.Drawing.Point(12, 419);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(285, 66);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Визуализация";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.load_custom_map);
            this.groupBox1.Controls.Add(this.checkBot4);
            this.groupBox1.Controls.Add(this.checkBot3);
            this.groupBox1.Controls.Add(this.checkBot2);
            this.groupBox1.Controls.Add(this.checkBot1);
            this.groupBox1.Controls.Add(this.path4_lab);
            this.groupBox1.Controls.Add(this.path4_btn);
            this.groupBox1.Controls.Add(this.path3_lab);
            this.groupBox1.Controls.Add(this.path3_btn);
            this.groupBox1.Controls.Add(this.path2_lab);
            this.groupBox1.Controls.Add(this.path2_btn);
            this.groupBox1.Controls.Add(this.realGameButton);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 401);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Игра в реальном времени";
            // 
            // load_custom_map
            // 
            this.load_custom_map.Location = new System.Drawing.Point(6, 244);
            this.load_custom_map.Name = "load_custom_map";
            this.load_custom_map.Size = new System.Drawing.Size(272, 39);
            this.load_custom_map.TabIndex = 0;
            this.load_custom_map.Text = "Загрузить Свою карту";
            this.load_custom_map.UseVisualStyleBackColor = true;
            this.load_custom_map.Click += new System.EventHandler(this.load_custom_map_Click);
            // 
            // checkBot4
            // 
            this.checkBot4.AutoSize = true;
            this.checkBot4.Checked = true;
            this.checkBot4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBot4.Location = new System.Drawing.Point(162, 219);
            this.checkBot4.Name = "checkBot4";
            this.checkBot4.Size = new System.Drawing.Size(113, 17);
            this.checkBot4.TabIndex = 12;
            this.checkBot4.Text = "Стандартный бот";
            this.checkBot4.UseVisualStyleBackColor = true;
          //  this.checkBot4.CheckedChanged += new System.EventHandler(this.checkBot4_CheckedChanged);
            // 
            // checkBot3
            // 
            this.checkBot3.AutoSize = true;
            this.checkBot3.Checked = true;
            this.checkBot3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBot3.Location = new System.Drawing.Point(162, 174);
            this.checkBot3.Name = "checkBot3";
            this.checkBot3.Size = new System.Drawing.Size(113, 17);
            this.checkBot3.TabIndex = 11;
            this.checkBot3.Text = "Стандартный бот";
            this.checkBot3.UseVisualStyleBackColor = true;
        //    this.checkBot3.CheckedChanged += new System.EventHandler(this.checkBot3_CheckedChanged);
            // 
            // checkBot2
            // 
            this.checkBot2.AutoSize = true;
            this.checkBot2.Checked = true;
            this.checkBot2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBot2.Location = new System.Drawing.Point(162, 128);
            this.checkBot2.Name = "checkBot2";
            this.checkBot2.Size = new System.Drawing.Size(113, 17);
            this.checkBot2.TabIndex = 10;
            this.checkBot2.Text = "Стандартный бот";
            this.checkBot2.UseVisualStyleBackColor = true;
          //  this.checkBot2.CheckedChanged += new System.EventHandler(this.checkBot2_CheckedChanged);
            // 
            // checkBot1
            // 
            this.checkBot1.AutoSize = true;
            this.checkBot1.Location = new System.Drawing.Point(162, 83);
            this.checkBot1.Name = "checkBot1";
            this.checkBot1.Size = new System.Drawing.Size(113, 17);
            this.checkBot1.TabIndex = 9;
            this.checkBot1.Text = "Стандартный бот";
            this.checkBot1.UseVisualStyleBackColor = true;
        //    this.checkBot1.CheckedChanged += new System.EventHandler(this.checkBot1_CheckedChanged);
            // 
            // path4_lab
            // 
            this.path4_lab.AutoSize = true;
            this.path4_lab.Location = new System.Drawing.Point(17, 198);
            this.path4_lab.Name = "path4_lab";
            this.path4_lab.Size = new System.Drawing.Size(0, 13);
            this.path4_lab.TabIndex = 8;
            // 
            // path4_btn
            // 
            this.path4_btn.Enabled = false;
            this.path4_btn.Location = new System.Drawing.Point(10, 214);
            this.path4_btn.Name = "path4_btn";
            this.path4_btn.Size = new System.Drawing.Size(132, 24);
            this.path4_btn.TabIndex = 7;
            this.path4_btn.Text = "Выбрать файл";
            this.path4_btn.UseVisualStyleBackColor = true;
            this.path4_btn.Click += new System.EventHandler(this.path4_btn_Click);
            // 
            // path3_lab
            // 
            this.path3_lab.AutoSize = true;
            this.path3_lab.Location = new System.Drawing.Point(17, 151);
            this.path3_lab.Name = "path3_lab";
            this.path3_lab.Size = new System.Drawing.Size(0, 13);
            this.path3_lab.TabIndex = 6;
            // 
            // path3_btn
            // 
            this.path3_btn.Enabled = false;
            this.path3_btn.Location = new System.Drawing.Point(10, 167);
            this.path3_btn.Name = "path3_btn";
            this.path3_btn.Size = new System.Drawing.Size(132, 24);
            this.path3_btn.TabIndex = 5;
            this.path3_btn.Text = "Выбрать файл";
            this.path3_btn.UseVisualStyleBackColor = true;
            this.path3_btn.Click += new System.EventHandler(this.path3_btn_Click);
            // 
            // path2_lab
            // 
            this.path2_lab.AutoSize = true;
            this.path2_lab.Location = new System.Drawing.Point(17, 107);
            this.path2_lab.Name = "path2_lab";
            this.path2_lab.Size = new System.Drawing.Size(0, 13);
            this.path2_lab.TabIndex = 4;
            // 
            // path1_lab
            // 
            this.path1_lab.AutoSize = true;
            this.path1_lab.Location = new System.Drawing.Point(6, 72);
            this.path1_lab.Name = "path1_lab";
            this.path1_lab.Size = new System.Drawing.Size(0, 13);
            this.path1_lab.TabIndex = 3;
            this.path1_lab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // path2_btn
            // 
            this.path2_btn.Enabled = false;
            this.path2_btn.Location = new System.Drawing.Point(10, 123);
            this.path2_btn.Name = "path2_btn";
            this.path2_btn.Size = new System.Drawing.Size(132, 24);
            this.path2_btn.TabIndex = 2;
            this.path2_btn.Text = "Выбрать файл";
            this.path2_btn.UseVisualStyleBackColor = true;
            this.path2_btn.Click += new System.EventHandler(this.path2_btn_Click);
            // 
            // path1_btn
            // 
            this.path1_btn.Location = new System.Drawing.Point(6, 46);
            this.path1_btn.Name = "path1_btn";
            this.path1_btn.Size = new System.Drawing.Size(243, 23);
            this.path1_btn.TabIndex = 1;
            this.path1_btn.Text = "Выбрать файл";
            this.path1_btn.UseVisualStyleBackColor = true;
            this.path1_btn.Click += new System.EventHandler(this.path1_btn_Click);
            // 
            // realGameButton
            // 
            this.realGameButton.Location = new System.Drawing.Point(6, 19);
            this.realGameButton.Name = "realGameButton";
            this.realGameButton.Size = new System.Drawing.Size(272, 37);
            this.realGameButton.TabIndex = 0;
            this.realGameButton.Text = "Запустить игру в реальном времени";
            this.realGameButton.UseVisualStyleBackColor = true;
            this.realGameButton.Click += new System.EventHandler(this.realGameButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.path1_btn);
            this.groupBox3.Controls.Add(this.path1_lab);
            this.groupBox3.Location = new System.Drawing.Point(20, 294);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(255, 101);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Игрок 1";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Не использовать этого игрока",
            "Ждать ручного запуска для отладки",
            "Загрузить стратегию из файла *.cs",
            "Использовать стандартного Бота"});
            this.comboBox1.Location = new System.Drawing.Point(6, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(243, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 508);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "StartPage";
            this.Text = "Bomberman Menu";
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button savedGameButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button load_custom_map;
        private System.Windows.Forms.CheckBox checkBot4;
        private System.Windows.Forms.CheckBox checkBot3;
        private System.Windows.Forms.CheckBox checkBot2;
        private System.Windows.Forms.CheckBox checkBot1;
        private System.Windows.Forms.Label path4_lab;
        private System.Windows.Forms.Button path4_btn;
        private System.Windows.Forms.Label path3_lab;
        private System.Windows.Forms.Button path3_btn;
        private System.Windows.Forms.Label path2_lab;
        private System.Windows.Forms.Label path1_lab;
        private System.Windows.Forms.Button path2_btn;
        private System.Windows.Forms.Button path1_btn;
        private System.Windows.Forms.Button realGameButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}