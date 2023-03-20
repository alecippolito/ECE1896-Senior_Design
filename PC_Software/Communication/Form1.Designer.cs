
namespace Communication
{
    partial class Form1
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
            this.sendButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.browseDataButton = new System.Windows.Forms.Button();
            this.dataTextbox = new System.Windows.Forms.TextBox();
            this.transmitterStatusLabel = new System.Windows.Forms.Label();
            this.sentLabel = new System.Windows.Forms.Label();
            this.warningLabel = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.openFileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(275, 82);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(122, 57);
            this.sendButton.TabIndex = 0;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // browseDataButton
            // 
            this.browseDataButton.Location = new System.Drawing.Point(49, 82);
            this.browseDataButton.Name = "browseDataButton";
            this.browseDataButton.Size = new System.Drawing.Size(195, 57);
            this.browseDataButton.TabIndex = 1;
            this.browseDataButton.Text = "Select File";
            this.browseDataButton.UseVisualStyleBackColor = true;
            this.browseDataButton.Click += new System.EventHandler(this.browseDataButton_Click);
            // 
            // dataTextbox
            // 
            this.dataTextbox.Location = new System.Drawing.Point(49, 157);
            this.dataTextbox.Name = "dataTextbox";
            this.dataTextbox.Size = new System.Drawing.Size(703, 38);
            this.dataTextbox.TabIndex = 2;
            // 
            // transmitterStatusLabel
            // 
            this.transmitterStatusLabel.AutoSize = true;
            this.transmitterStatusLabel.Location = new System.Drawing.Point(43, 223);
            this.transmitterStatusLabel.Name = "transmitterStatusLabel";
            this.transmitterStatusLabel.Size = new System.Drawing.Size(96, 32);
            this.transmitterStatusLabel.TabIndex = 3;
            this.transmitterStatusLabel.Text = "Status";
            // 
            // sentLabel
            // 
            this.sentLabel.AutoSize = true;
            this.sentLabel.Location = new System.Drawing.Point(145, 223);
            this.sentLabel.Name = "sentLabel";
            this.sentLabel.Size = new System.Drawing.Size(74, 32);
            this.sentLabel.TabIndex = 5;
            this.sentLabel.Text = "Sent";
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.Location = new System.Drawing.Point(397, 95);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(0, 32);
            this.warningLabel.TabIndex = 6;
            // 
            // openFileButton
            // 
            this.openFileButton.Location = new System.Drawing.Point(1140, 113);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(284, 112);
            this.openFileButton.TabIndex = 7;
            this.openFileButton.Text = "Open File";
            this.openFileButton.UseVisualStyleBackColor = true;
            this.openFileButton.Click += new System.EventHandler(this.openFileButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1741, 495);
            this.Controls.Add(this.openFileButton);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.sentLabel);
            this.Controls.Add(this.transmitterStatusLabel);
            this.Controls.Add(this.dataTextbox);
            this.Controls.Add(this.browseDataButton);
            this.Controls.Add(this.sendButton);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button browseDataButton;
        private System.Windows.Forms.TextBox dataTextbox;
        private System.Windows.Forms.Label transmitterStatusLabel;
        private System.Windows.Forms.Label sentLabel;
        private System.Windows.Forms.Label warningLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button openFileButton;
    }
}

