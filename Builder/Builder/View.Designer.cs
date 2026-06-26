namespace Builder
{
    partial class View
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btZoomIn = new Button();
            button1 = new Button();
            btZoomOut = new Button();
            btFocus = new Button();
            SuspendLayout();
            // 
            // btZoomIn
            // 
            btZoomIn.Location = new Point(3, 3);
            btZoomIn.Name = "btZoomIn";
            btZoomIn.Size = new Size(28, 28);
            btZoomIn.TabIndex = 0;
            btZoomIn.Text = "+";
            btZoomIn.UseVisualStyleBackColor = true;
            btZoomIn.Click += btZoomIn_Click;
            // 
            // button1
            // 
            button1.Location = new Point(366, 240);
            button1.Name = "button1";
            button1.Size = new Size(0, 0);
            button1.TabIndex = 1;
            button1.Text = "+";
            button1.UseVisualStyleBackColor = true;
            // 
            // btZoomOut
            // 
            btZoomOut.Location = new Point(37, 3);
            btZoomOut.Name = "btZoomOut";
            btZoomOut.Size = new Size(28, 28);
            btZoomOut.TabIndex = 2;
            btZoomOut.Text = "-";
            btZoomOut.UseVisualStyleBackColor = true;
            btZoomOut.Click += btZoomOut_Click;
            // 
            // btFocus
            // 
            btFocus.Location = new Point(71, 3);
            btFocus.Name = "btFocus";
            btFocus.Size = new Size(70, 28);
            btFocus.TabIndex = 3;
            btFocus.Text = "Focus";
            btFocus.UseVisualStyleBackColor = true;
            btFocus.Click += btFocus_Click;
            // 
            // View
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btFocus);
            Controls.Add(btZoomOut);
            Controls.Add(button1);
            Controls.Add(btZoomIn);
            Name = "View";
            Size = new Size(760, 505);
            Load += View_Load;
            Paint += OnPaint;
            MouseDoubleClick += OnMouseDoubleClick;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;
            ResumeLayout(false);
        }

        #endregion

        private Button btZoomIn;
        private Button button1;
        private Button btZoomOut;
        private Button btFocus;
    }
}
