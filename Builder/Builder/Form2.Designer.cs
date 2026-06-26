namespace Builder
{
    partial class Form2
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
            components = new System.ComponentModel.Container();
            tmClock = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // tmClock
            // 
            tmClock.Interval = 50;
            tmClock.Tick += tmClock_Tick;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            MinimumSize = new Size(1600, 900);
            Name = "Form2";
            Text = "jok";
            Load += Form2_Load;
            Click += Form2_Click;
            Paint += Form2_Paint;
            Resize += Form2_Resize;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer tmClock;
        private RichTextBox rtbDialog;
    }
}