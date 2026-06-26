
using Builder.Background;
using Builder.Dialog;
using Builder.Scene;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Builder
{
    public partial class Form2 : Form
    {
        //font
        private Font fontName;
        private Font fontText;
        //window
        private Vector2 _size = Vector2.Zero;
        private Graphics _graphics;
        private Block _b;
        private string _path = string.Empty;
        
        public void setPath(string path)
        {
            this._path = path;
        }

        public Form2()
        {
            InitializeComponent();
            
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
            


            this.UpdateStyles();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            fontName = new Font("Verdana", 12, FontStyle.Bold);
            fontText = new Font("Verdana", 10, FontStyle.Bold);
            _size = new Vector2(this.ClientSize.Width, this.ClientSize.Height);
            _b = new Block(this, tmClock, _size);
            _b.LoadFromArchive(_path); 
            _b.Start(_b.startID);
            tmClock.Enabled = true;
        }

      

        private void tmClock_Tick(object sender, EventArgs e)
        { 
            _b.Update(sender, e);
            this.Invalidate();
        }

        //draw
        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            _graphics = e.Graphics;
            _b.Paint(_graphics);
           
        }

        

        private void Form2_Resize(object sender, EventArgs e)
        {
            _size = new Vector2(this.ClientSize.Width, this.ClientSize.Height);
            _b.Resize(_size);
        }

        private void Form2_Click(object sender, EventArgs e)
        {
            _b.Click(sender, e);
        }
       
    }
}
