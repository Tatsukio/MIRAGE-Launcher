namespace PWKiller
{
    partial class PWKiller
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
        public void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PWKiller));
            this.MainButton = new System.Windows.Forms.Button();
            this.MainLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MainButton
            // 
            this.MainButton.Font = new System.Drawing.Font("Trebuchet MS", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainButton.Location = new System.Drawing.Point(12, 170);
            this.MainButton.Name = "MainButton";
            this.MainButton.Size = new System.Drawing.Size(275, 120);
            this.MainButton.TabIndex = 0;
            this.MainButton.Text = "End processes";
            this.MainButton.UseVisualStyleBackColor = true;
            this.MainButton.Click += new System.EventHandler(this.MainButton_Click);
            // 
            // MainLabel
            // 
            this.MainLabel.Font = new System.Drawing.Font("Trebuchet MS", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainLabel.Location = new System.Drawing.Point(12, 10);
            this.MainLabel.Name = "MainLabel";
            this.MainLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MainLabel.Size = new System.Drawing.Size(275, 150);
            this.MainLabel.TabIndex = 1;
            this.MainLabel.Text = "If ParaWorld crashes or does not terminate, press this button!";
            this.MainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PWKiller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 302);
            this.Controls.Add(this.MainLabel);
            this.Controls.Add(this.MainButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PWKiller";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PWKiller";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button MainButton;
        private System.Windows.Forms.Label MainLabel;
    }
}

