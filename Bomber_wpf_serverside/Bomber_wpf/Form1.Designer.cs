namespace Bomber_wpf
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.players_listView = new System.Windows.Forms.ListView();
            this.game_timer = new System.Windows.Forms.Timer(this.components);
            this.fast_btn = new System.Windows.Forms.Button();
            this.control_btn = new System.Windows.Forms.Button();
            this.slow_btn = new System.Windows.Forms.Button();
            this.log_box = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.MaximumSize = new System.Drawing.Size(450, 450);
            this.panel1.MinimumSize = new System.Drawing.Size(450, 450);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(450, 450);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.players_listView);
            this.groupBox1.Location = new System.Drawing.Point(456, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(564, 394);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Игроки";
            // 
            // players_listView
            // 
            this.players_listView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.players_listView.ForeColor = System.Drawing.SystemColors.Info;
            this.players_listView.GridLines = true;
            this.players_listView.Location = new System.Drawing.Point(6, 19);
            this.players_listView.Name = "players_listView";
            this.players_listView.Size = new System.Drawing.Size(552, 369);
            this.players_listView.TabIndex = 0;
            this.players_listView.UseCompatibleStateImageBehavior = false;
            // 
            // fast_btn
            // 
            this.fast_btn.Location = new System.Drawing.Point(851, 400);
            this.fast_btn.Name = "fast_btn";
            this.fast_btn.Size = new System.Drawing.Size(169, 39);
            this.fast_btn.TabIndex = 3;
            this.fast_btn.Text = "Ускорить x2";
            this.fast_btn.UseVisualStyleBackColor = true;
            this.fast_btn.Click += new System.EventHandler(this.fast_btn_Click);
            // 
            // control_btn
            // 
            this.control_btn.Location = new System.Drawing.Point(631, 400);
            this.control_btn.Name = "control_btn";
            this.control_btn.Size = new System.Drawing.Size(214, 39);
            this.control_btn.TabIndex = 4;
            this.control_btn.Text = "Пауза";
            this.control_btn.UseVisualStyleBackColor = true;
            this.control_btn.Click += new System.EventHandler(this.control_btn_Click);
            // 
            // slow_btn
            // 
            this.slow_btn.Location = new System.Drawing.Point(456, 400);
            this.slow_btn.Name = "slow_btn";
            this.slow_btn.Size = new System.Drawing.Size(169, 39);
            this.slow_btn.TabIndex = 5;
            this.slow_btn.Text = "Замедлить x2";
            this.slow_btn.UseVisualStyleBackColor = true;
            this.slow_btn.Click += new System.EventHandler(this.slow_btn_Click);
            // 
            // log_box
            // 
            this.log_box.Location = new System.Drawing.Point(12, 456);
            this.log_box.Multiline = true;
            this.log_box.Name = "log_box";
            this.log_box.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log_box.Size = new System.Drawing.Size(1008, 221);
            this.log_box.TabIndex = 6;
            this.log_box.TextChanged += new System.EventHandler(this.log_box_TextChanged);
            this.log_box.DoubleClick += new System.EventHandler(this.log_box_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1032, 689);
            this.Controls.Add(this.log_box);
            this.Controls.Add(this.slow_btn);
            this.Controls.Add(this.control_btn);
            this.Controls.Add(this.fast_btn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bomberman Game";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer game_timer;
        private System.Windows.Forms.ListView players_listView;
        private System.Windows.Forms.Button fast_btn;
        private System.Windows.Forms.Button control_btn;
        private System.Windows.Forms.Button slow_btn;
        private System.Windows.Forms.TextBox log_box;
    }
}

