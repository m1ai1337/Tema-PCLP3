namespace Builder
{
    partial class BlockControl
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            splitContainer1 = new SplitContainer();
            label1 = new Label();
            btBack = new Button();
            splitContainer2 = new SplitContainer();
            splitContainer3 = new SplitContainer();
            groupMain = new GroupBox();
            lbBackground = new Label();
            btPath = new Button();
            tbPath = new TextBox();
            ckTransition = new CheckBox();
            groupDecizie = new GroupBox();
            decizieData = new DataGridView();
            checkDecizie = new CheckBox();
            groupDialog = new GroupBox();
            dialogData = new DataGridView();
            Nume = new DataGridViewTextBoxColumn();
            Text = new DataGridViewTextBoxColumn();
            openFileDialog1 = new OpenFileDialog();
            Nr = new DataGridViewTextBoxColumn();
            Decizie = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            groupMain.SuspendLayout();
            groupDecizie.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)decizieData).BeginInit();
            groupDialog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dialogData).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(btBack);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(638, 540);
            splitContainer1.SplitterDistance = 40;
            splitContainer1.TabIndex = 0;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(5, 13);
            label1.Margin = new Padding(5, 0, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // btBack
            // 
            btBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btBack.Location = new Point(558, 5);
            btBack.Margin = new Padding(5);
            btBack.Name = "btBack";
            btBack.Size = new Size(75, 30);
            btBack.TabIndex = 0;
            btBack.Text = "Back";
            btBack.UseVisualStyleBackColor = true;
            btBack.Click += btClose_Click_1;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupDialog);
            splitContainer2.Size = new Size(638, 496);
            splitContainer2.SplitterDistance = 319;
            splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(groupMain);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(groupDecizie);
            splitContainer3.Size = new Size(319, 496);
            splitContainer3.SplitterDistance = 80;
            splitContainer3.TabIndex = 1;
            // 
            // groupMain
            // 
            groupMain.Controls.Add(lbBackground);
            groupMain.Controls.Add(btPath);
            groupMain.Controls.Add(tbPath);
            groupMain.Controls.Add(ckTransition);
            groupMain.Dock = DockStyle.Fill;
            groupMain.Location = new Point(0, 0);
            groupMain.Name = "groupMain";
            groupMain.Size = new Size(319, 80);
            groupMain.TabIndex = 0;
            groupMain.TabStop = false;
            groupMain.Text = "Main";
            // 
            // lbBackground
            // 
            lbBackground.AutoSize = true;
            lbBackground.Location = new Point(3, 23);
            lbBackground.Name = "lbBackground";
            lbBackground.Size = new Size(74, 15);
            lbBackground.TabIndex = 3;
            lbBackground.Text = "Background:";
            // 
            // btPath
            // 
            btPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btPath.Location = new Point(238, 19);
            btPath.Name = "btPath";
            btPath.Size = new Size(75, 23);
            btPath.TabIndex = 2;
            btPath.Text = "Path";
            btPath.UseVisualStyleBackColor = true;
            btPath.Click += btPath_Click;
            // 
            // tbPath
            // 
            tbPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbPath.Location = new Point(83, 19);
            tbPath.Name = "tbPath";
            tbPath.ReadOnly = true;
            tbPath.Size = new Size(149, 23);
            tbPath.TabIndex = 1;
            // 
            // ckTransition
            // 
            ckTransition.AutoSize = true;
            ckTransition.Location = new Point(6, 48);
            ckTransition.Name = "ckTransition";
            ckTransition.Size = new Size(113, 19);
            ckTransition.TabIndex = 0;
            ckTransition.Text = "Efect de tranzitie";
            ckTransition.UseVisualStyleBackColor = true;
            ckTransition.CheckedChanged += ckTransition_CheckedChanged;
            // 
            // groupDecizie
            // 
            groupDecizie.Controls.Add(decizieData);
            groupDecizie.Controls.Add(checkDecizie);
            groupDecizie.Dock = DockStyle.Fill;
            groupDecizie.Location = new Point(0, 0);
            groupDecizie.Name = "groupDecizie";
            groupDecizie.Size = new Size(319, 412);
            groupDecizie.TabIndex = 1;
            groupDecizie.TabStop = false;
            groupDecizie.Text = "Decizie";
            // 
            // decizieData
            // 
            decizieData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            decizieData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            decizieData.Columns.AddRange(new DataGridViewColumn[] { Nr, Decizie });
            decizieData.Location = new Point(6, 47);
            decizieData.Name = "decizieData";
            decizieData.Size = new Size(307, 359);
            decizieData.TabIndex = 1;
            // 
            // checkDecizie
            // 
            checkDecizie.AutoSize = true;
            checkDecizie.Location = new Point(6, 22);
            checkDecizie.Name = "checkDecizie";
            checkDecizie.Size = new Size(61, 19);
            checkDecizie.TabIndex = 0;
            checkDecizie.Text = "Enable";
            checkDecizie.UseVisualStyleBackColor = true;
            checkDecizie.CheckedChanged += checkDecizie_CheckedChanged;
            // 
            // groupDialog
            // 
            groupDialog.Controls.Add(dialogData);
            groupDialog.Dock = DockStyle.Fill;
            groupDialog.Location = new Point(0, 0);
            groupDialog.Name = "groupDialog";
            groupDialog.Size = new Size(315, 496);
            groupDialog.TabIndex = 2;
            groupDialog.TabStop = false;
            groupDialog.Text = "Dialog";
            // 
            // dialogData
            // 
            dialogData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dialogData.Columns.AddRange(new DataGridViewColumn[] { Nume, Text });
            dialogData.Dock = DockStyle.Fill;
            dialogData.Location = new Point(3, 19);
            dialogData.Margin = new Padding(5);
            dialogData.Name = "dialogData";
            dialogData.Size = new Size(309, 474);
            dialogData.TabIndex = 1;
            // 
            // Nume
            // 
            Nume.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            Nume.DefaultCellStyle = dataGridViewCellStyle1;
            Nume.HeaderText = "Nume";
            Nume.Name = "Nume";
            Nume.SortMode = DataGridViewColumnSortMode.NotSortable;
            Nume.Width = 46;
            // 
            // Text
            // 
            Text.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            Text.DefaultCellStyle = dataGridViewCellStyle2;
            Text.HeaderText = "Text";
            Text.Name = "Text";
            Text.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "\"GIF Files|*.gif;*.GIF\";";
            // 
            // Nr
            // 
            Nr.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            Nr.HeaderText = "Nr";
            Nr.Name = "Nr";
            Nr.ReadOnly = true;
            Nr.Width = 45;
            // 
            // Decizie
            // 
            Decizie.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Decizie.HeaderText = "Decizie";
            Decizie.Name = "Decizie";
            // 
            // BlockControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "BlockControl";
            Size = new Size(638, 540);
            Load += BlockControl_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            groupMain.ResumeLayout(false);
            groupMain.PerformLayout();
            groupDecizie.ResumeLayout(false);
            groupDecizie.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)decizieData).EndInit();
            groupDialog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dialogData).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Button btBack;
        private Label label1;
        private SplitContainer splitContainer2;
        private DataGridView dialogData;
        private GroupBox groupMain;
        private CheckBox ckTransition;
        private GroupBox groupDialog;
        private SplitContainer splitContainer3;
        private GroupBox groupDecizie;
        private Label lbBackground;
        private Button btPath;
        private TextBox tbPath;
        private CheckBox checkDecizie;
        private OpenFileDialog openFileDialog1;
        private DataGridViewTextBoxColumn Nume;
        private DataGridViewTextBoxColumn Text;
        private DataGridView decizieData;
        private DataGridViewTextBoxColumn Nr;
        private DataGridViewTextBoxColumn Decizie;
    }
}
