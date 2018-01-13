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
            this.realGameButton = new System.Windows.Forms.Button();
            this.savedGameButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // realGameButton
            // 
            this.realGameButton.Location = new System.Drawing.Point(13, 12);
            this.realGameButton.Name = "realGameButton";
            this.realGameButton.Size = new System.Drawing.Size(241, 37);
            this.realGameButton.TabIndex = 0;
            this.realGameButton.Text = "Запустить игру в реальном времени";
            this.realGameButton.UseVisualStyleBackColor = true;
            this.realGameButton.Click += new System.EventHandler(this.realGameButton_Click);
            // 
            // savedGameButton
            // 
            this.savedGameButton.Location = new System.Drawing.Point(13, 55);
            this.savedGameButton.Name = "savedGameButton";
            this.savedGameButton.Size = new System.Drawing.Size(240, 37);
            this.savedGameButton.TabIndex = 1;
            this.savedGameButton.Text = "Запустить ранее сохранённую игру";
            this.savedGameButton.UseVisualStyleBackColor = true;
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(265, 102);
            this.Controls.Add(this.savedGameButton);
            this.Controls.Add(this.realGameButton);
            this.Name = "StartPage";
            this.Text = "StartPage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button realGameButton;
        private System.Windows.Forms.Button savedGameButton;
    }
}